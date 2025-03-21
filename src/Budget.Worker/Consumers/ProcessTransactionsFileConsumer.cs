using Budget.Application.Interfaces;
using Budget.Application.Settings;
using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Domain.Commands;
using Budget.Domain.Enums;
using Budget.Domain.Repositories;
using MassTransit;

namespace Budget.Worker.Consumers;

public class ProcessTransactionsFileConsumer(
    ITransactionsFileJobRepository repo,
    ITransactionsFileEtlUseCase useCase,
    ILogger<ProcessTransactionsFile> logger,
    FileStorageSettings fileStorageSettings,
    IFileSystem fileSystem)
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

        var filePathAbsolute = Path.Combine(fileStorageSettings.BasePath ?? "/", job.StoredFilePath);

        job.Status = JobStatus.Processing;
        await repo.SaveChangesAsync();

        if (!fileSystem.FileExists(filePathAbsolute))
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = $"File {filePathAbsolute} does not exist";
            await repo.SaveChangesAsync();
            return;
        }

        await using var fileStream = fileSystem.OpenRead(filePathAbsolute);

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