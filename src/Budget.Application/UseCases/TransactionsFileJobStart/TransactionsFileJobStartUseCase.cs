using Budget.Application.Settings;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Domain;
using Budget.Domain.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Budget.Application.UseCases;

public class TransactionsFileJobStartUseCase(
    IBudgetContext db,
    IPublishEndpoint endpoint,
    ILogger<TransactionsFileJobStartUseCase> logger,
    FileStorageSettings fileSettings,
    TimeProvider timeProvider)
{
    public class FileModel
    {
        public byte[] Content { get; init; }
        public string FileName { get; init; }
        public string ContentType { get; init; }
        public long Size { get; init; }
    }

    public class Command
    {
        public FileModel File { get; init; }
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
            CreatedAt = timeProvider.GetUtcNow().DateTime,
            StoredFilePath = fileStoreResult.Value,
            OriginalFileName = command.File.FileName,
        };
        await db.TransactionsFileJobs.AddAsync(job);
        await db.SaveChangesAsync();
        
        await endpoint.Publish<ProcessTransactionsFile>(new
        {
            JobId = job.Id
        });

        return Result<Response>.Success(new Response());
    }
}