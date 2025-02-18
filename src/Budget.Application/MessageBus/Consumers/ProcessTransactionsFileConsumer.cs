using Budget.Domain.Commands;
using Budget.Domain.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Budget.Application.MessageBus.Consumers;

public class ProcessTransactionsFileConsumer(IBudgetContext db, ILogger<ProcessTransactionsFile> logger)
    : IConsumer<ProcessTransactionsFile>
{
    public async Task Consume(ConsumeContext<ProcessTransactionsFile> context)
    {
        logger.LogInformation("Going to process transactions file job {JobId}", context.Message.JobId);
        var job = await db.TransactionsFileJobs.FirstOrDefaultAsync(j => j.Id == context.Message.JobId);

        if (job == null || job.Status is JobStatus.Completed or JobStatus.Failed)
        {
            logger.LogInformation("Job is already picked up by a previous process");
        }

        job.Status = JobStatus.Processing;
        await db.SaveChangesAsync();
    }
}