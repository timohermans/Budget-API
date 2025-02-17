using Budget.Application.Settings;
using Budget.Domain;
using Microsoft.Extensions.Logging;

namespace Budget.Application.UseCases.TransactionsFileJobStart;

public class FileStorer(FileStorageSettings settings, ILogger logger)
{
    public async Task<Result<string>> Store(TransactionsFileJobStartUseCase.FileModel file)
    {
        ArgumentNullException.ThrowIfNull(settings.BasePath);

        try
        {
            var basePath = settings.BasePath;
            var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.GetFullPath(Path.Combine(basePath, uniqueFileName));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException());

            await File.WriteAllBytesAsync(fullPath, file.Content);

            return Result<string>.Success(uniqueFileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error storing file {FileName}", file.FileName);
            return Result<string>.Failure($"Error storing file {file.FileName}");
        }
    }
}