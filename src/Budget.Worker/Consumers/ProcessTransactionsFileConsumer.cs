using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain.Commands;
using Budget.Domain.Enums;
using Budget.Domain.Repositories;
using MassTransit;

namespace Budget.Worker.Consumers;

public class ProcessTransactionsFileConsumer(
    ITransactionsFileJobRepository repo,
    ITransactionsFileEtlUseCase useCase,
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

        if (job.FileContent.Length == 0)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = "File content is empty";
            await repo.SaveChangesAsync();
            return;
        }

        await using var fileStream = new MemoryStream(job.FileContent);

        var result = await useCase.HandleAsync(fileStream);

        if (result.IsFailure)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = $"UseCase failed with message: {result.Error}";
        }
        else
        {
            job.Status = JobStatus.Completed;
        }

        await repo.SaveChangesAsync();
    }
}
