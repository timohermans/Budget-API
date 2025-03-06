using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Domain.Commands;
using Budget.Domain.Enums;
using Budget.Domain.Repositories;
using MassTransit;

namespace Budget.Worker.Consumers;

public class ProcessTransactionsFileConsumer(
    ITransactionsFileJobRepository repo,
    TransactionsFileEtlUseCase useCase,
    ILogger<ProcessTransactionsFile> logger)
    : IConsumer<ProcessTransactionsFile>
{
    public async Task Consume(ConsumeContext<ProcessTransactionsFile> context)
    {
        logger.LogInformation("Going to process transactions file job {JobId}", context.Message.JobId);
        var job = await repo.GetByIdAsync(context.Message.JobId);

        if (job == null || job.Status is JobStatus.Completed or JobStatus.Failed)
        {
            logger.LogInformation("Job is already picked up by a previous process");
            return;
        }

        job.Status = JobStatus.Processing;
        await repo.SaveChangesAsync();

        if (!File.Exists(job.StoredFilePath))
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = $"File {job.StoredFilePath} does not exist";
            await repo.SaveChangesAsync();
            return;
        }

        var fileStream = File.OpenRead(job.StoredFilePath);

        var result = await useCase.HandleAsync(fileStream);

        if (result.IsFailure)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = result.Error;
        }
        else
        {
            job.Status = JobStatus.Completed;
        }

        await repo.SaveChangesAsync();
    }
}