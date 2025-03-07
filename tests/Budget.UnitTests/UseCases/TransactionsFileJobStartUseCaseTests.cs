using Budget.Application.Settings;
using Budget.Application.UseCases;
using Budget.Domain;
using Budget.Domain.Commands;
using Budget.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Budget.UnitTests.UseCases;

public class TransactionsFileJobStartUseCaseTests : IDisposable
{
    private readonly ITransactionsFileJobRepository _repo = Substitute.For<ITransactionsFileJobRepository>();
    private readonly IPublishEndpoint _endpoint = Substitute.For<IPublishEndpoint>();

    private readonly ILogger<TransactionsFileJobStartUseCase> _logger =
        Substitute.For<ILogger<TransactionsFileJobStartUseCase>>();

    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private readonly FileStorageSettings _fileSettings = new FileStorageSettings
    {
        MaxSizeMb = 10,
        BasePath = @"/Volumes/ssd/Dev/tmp/budget-tests",
    };

    private TransactionsFileJobStartUseCase CreateSut()
    {
        if (_fileSettings.BasePath != null && !Path.Exists(_fileSettings.BasePath))
        {
            Directory.CreateDirectory(_fileSettings.BasePath);
        }

        return new(
            _repo,
            _endpoint,
            _logger,
            _fileSettings,
            _timeProvider);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_SavesJobAndPublishesEvent()
    {
        // Arrange
        TransactionsFileJob? capturedJob = null;
        _repo.When(x => x.AddAsync(Arg.Any<TransactionsFileJob>()))
            .Do(x => capturedJob = x.Arg<TransactionsFileJob>());
        var testTime = new DateTimeOffset(new DateTime(2025, 1, 1));
        _timeProvider.GetUtcNow().Returns(testTime);

        var sut = CreateSut();
        var validFile = new TransactionsFileJobStartUseCase.FileModel
        {
            Content = new byte[100],
            FileName = "valid.csv",
            ContentType = "text/csv",
            Size = 100
        };

        // Act
        var result = await sut.HandleAsync(new TransactionsFileJobStartUseCase.Command { File = validFile });

        // Assert
        Assert.True(result.IsSuccess);
        await _repo.Received(1).AddAsync(Arg.Is<TransactionsFileJob>(j =>
            j.CreatedAt == testTime.UtcDateTime));
        await _repo.Received(1).SaveChangesAsync();
        await _endpoint.Received(1).Publish<ProcessTransactionsFile>(Arg.Any<object>(), Arg.Any<CancellationToken>());
        Assert.NotNull(capturedJob);
        Assert.NotEqual(Guid.Empty, capturedJob.Id);
        Assert.Equal(testTime, capturedJob.CreatedAt, TimeSpan.FromMilliseconds(100));
        Assert.NotNull(capturedJob.StoredFilePath);
        Assert.Equal("valid.csv", capturedJob.OriginalFileName);
        Assert.True(File.Exists(Path.Combine(_fileSettings.BasePath!, capturedJob.StoredFilePath)));
    }

    [Fact]
    public async Task HandleAsync_InvalidFileTooLarge_ReturnsFailure()
    {
        // Arrange
        _fileSettings.MaxSizeMb = 1;
        var sut = CreateSut();
        var invalidFile = new TransactionsFileJobStartUseCase.FileModel
        {
            Content = [],
            FileName = "test.csv",
            ContentType = "text/plain",
            Size = 999999999
        };

        // Act
        var result = await sut.HandleAsync(new TransactionsFileJobStartUseCase.Command { File = invalidFile });

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("File size exceeds maximum", result.Error);
    }

    public void Dispose()
    {
        if (_fileSettings.BasePath != null && Path.Exists(_fileSettings.BasePath))
            Directory.Delete(_fileSettings.BasePath, true);
    }
}