using Budget.Application.Settings;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Domain;
using Budget.Domain.Commands;
using Budget.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Budget.Application.UseCases;

public interface ITransactionsFileJobStartUseCase
{
    Task<Result<TransactionsFileJobStartUseCase.Response>> HandleAsync(TransactionsFileJobStartUseCase.Command command);
}

public class TransactionsFileJobStartUseCase(
    ITransactionsFileJobRepository repo,
    IPublishEndpoint endpoint,
    ILogger<TransactionsFileJobStartUseCase> logger,
    FileStorageSettings fileSettings,
    TimeProvider timeProvider) : ITransactionsFileJobStartUseCase
{
    public class FileModel
    {
        public required byte[] Content { get; init; }
        public required string FileName { get; init; }
        public required string ContentType { get; init; }
        public long Size { get; init; }
    }

    public class Command
    {
        public required FileModel File { get; init; }
    }

    public class Response
    {
        public int JobId { get; set; }
    }

    public async Task<Result<Response>> HandleAsync(Command command)
    {
        var fileValidator = new TransactionsFileValidator(fileSettings, logger);
        var fileStorer = new FileStorer(fileSettings, logger);
        
        var validateResult = fileValidator.IsValid(command.File);

        if (validateResult.IsFailure)
        {
            return Result<Response>.Failure(validateResult.Error);
        }
        
        var fileStoreResult = await fileStorer.Store(command.File);

        if (fileStoreResult.IsFailure)
        {
            return Result<Response>.Failure(fileStoreResult.Error);
        }

        var job = new TransactionsFileJob
        {
            Id = NewId.NextGuid(),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            StoredFilePath = fileStoreResult.Value,
            OriginalFileName = command.File.FileName,
        };
        await repo.AddAsync(job);
        await repo.SaveChangesAsync();
        
        await endpoint.Publish<ProcessTransactionsFile>(new
        {
            JobId = job.Id
        });

        return Result<Response>.Success(new Response());
    }
}