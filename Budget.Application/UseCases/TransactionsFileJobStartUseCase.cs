using Budget.Application.Settings;
using Budget.Domain;
using Microsoft.Extensions.Logging;

namespace Budget.Application.UseCases;

public class TransactionsFileJobStartUseCase(
    ILogger<TransactionsFileJobStartUseCase> logger,
    FileValidationSettings fileSettings)
{
    public class Command
    {
        public byte[] Content { get; init; }
        public string FileName { get; init; }
        public string ContentType { get; init; }
        public long Size { get; init; }
    }

    public class Response
    {
        public int JobId { get; set; }
    }

    public async Task<Result<Response>> HandleAsync(Command command)
    {
        var (isFileValid, error) = ValidateFile(command);
        // TODO: File validation
        // TODO: Store file
        // TODO: Save as Job in DB
        // TODO: MassTransit code here

        return Result<Response>.Success(new Response());
    }

    private (bool isValid, string error) ValidateFile(Command file)
    {
        try
        {
            if (file.Size == 0)
            {
                return (false, "No file was provided.");
            }

            var maxSizeInBytes = fileSettings.MaxSizeMb * 1024 * 1024; // Default 10MB
            if (file.Size > maxSizeInBytes)
            {
                return (false, $"File size exceeds maximum allowed size of {maxSizeInBytes / 1024 / 1024}MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".csv")
            {
                return (false, "Only CSV files are allowed.");
            }

            var allowedMimeTypes = new[]
            {
                "text/csv",
                "application/csv",
                "text/plain",
                "application/vnd.ms-excel"
            };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, "Invalid file type. Only CSV files are allowed.");
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating file {FileName}", file.FileName);
            return (false, "An error occurred while validating the file.");
        }
    }
}