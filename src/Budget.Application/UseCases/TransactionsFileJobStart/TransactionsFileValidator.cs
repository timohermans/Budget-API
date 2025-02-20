using Budget.Application.Settings;
using Budget.Domain;
using Microsoft.Extensions.Logging;

namespace Budget.Application.UseCases;

public class TransactionsFileValidator(FileStorageSettings settings, ILogger logger)
{
    public Result IsValid(TransactionsFileJobStartUseCase.FileModel file)
    {
        try
        {
            if (file.Size == 0)
            {
                return Result.Failure("No file was provided.");
            }

            var maxSizeInBytes = settings.MaxSizeMb * 1024 * 1024; // Default 10MB
            if (file.Size > maxSizeInBytes)
            {
                return Result.Failure($"File size exceeds maximum allowed size of {settings.MaxSizeMb}MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".csv")
            {
                return Result.Failure("Only CSV files are allowed.");
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
                return Result.Failure("Invalid file type. Only CSV files are allowed.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating file {FileName}", file.FileName);
            return Result.Failure("An error occurred while validating the file.");
        }
    }
}