using Budget.Application.Interfaces;
using Budget.Application.Settings;
using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain;
using Budget.Domain.Commands;
using Budget.Domain.Enums;
using Budget.Domain.Repositories;
using Budget.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Budget.UnitTests.Workers
{
    public class ProcessTransactionsFileConsumerTests
    {
        private readonly ITransactionsFileJobRepository repoMock;
        private readonly ITransactionsFileEtlUseCase useCaseMock;
        private readonly ILogger<ProcessTransactionsFile> loggerMock;
        private readonly IFileSystem fileSystemMock;
        private readonly FileStorageSettings fileStorageSettings;
        private readonly ConsumeContext<ProcessTransactionsFile> contextMock;
        private readonly TransactionsFileJob job = new TransactionsFileJob
        {
            Id = Guid.NewGuid(),
            Status = JobStatus.Pending,
            StoredFilePath = "file.txt",
            OriginalFileName = "file.txt"
        };


        public ProcessTransactionsFileConsumerTests()
        {
            repoMock = Substitute.For<ITransactionsFileJobRepository>();
            useCaseMock = Substitute.For<ITransactionsFileEtlUseCase>();
            loggerMock = Substitute.For<ILogger<ProcessTransactionsFile>>();
            fileSystemMock = Substitute.For<IFileSystem>();
            contextMock = Substitute.For<ConsumeContext<ProcessTransactionsFile>>();

            fileStorageSettings = new FileStorageSettings { BasePath = "/test/base/path" };
            fileSystemMock.FileExists(Arg.Any<string>()).Returns(true);
        }

        private ProcessTransactionsFileConsumer CreateConsumer()
        {
            repoMock.GetByIdAsync(Arg.Any<Guid>()).Returns(job);
            contextMock.Message.Returns(new ProcessTransactionsFile { JobId = job.Id });
            return new ProcessTransactionsFileConsumer(repoMock, useCaseMock, loggerMock, fileStorageSettings, fileSystemMock);
        }

        [Fact]
        public async Task Consume_FileNotOnDisk_FailsAndSavesErrorInDb()
        {
            // Arrange
            fileSystemMock.FileExists(Arg.Any<string>()).Returns(false);

            var consumer = CreateConsumer();

            // Act
            await consumer.Consume(contextMock);

            // Assert
            var expectedFilePath = Path.Combine(fileStorageSettings.BasePath!, job.StoredFilePath);
            Assert.Equal(JobStatus.Failed, job.Status);
            Assert.Equal($"File {expectedFilePath} does not exist", job.ErrorMessage);
        }


        [Fact]
        public async Task Consume_FileOnDisk_UsesCorrectPath()
        {
            // Arrange
            useCaseMock.HandleAsync(Arg.Any<Stream>()).Returns(Result.Success());
            var consumer = CreateConsumer();

            // Act
            await consumer.Consume(contextMock);

            // Assert
            var expectedFilePath = Path.Combine(fileStorageSettings.BasePath!, job.StoredFilePath);
            fileSystemMock.Received().FileExists(expectedFilePath);
            fileSystemMock.Received().OpenRead(expectedFilePath);
        }

        [Fact]
        public async Task Consume_GoodJobAndFile_CompletesJobSuccessfully()
        {
            // Arrange
            useCaseMock.HandleAsync(Arg.Any<Stream>()).Returns(Result.Success());
            var consumer = CreateConsumer();

            // Act
            await consumer.Consume(contextMock);

            // Assert
            Assert.Equal(JobStatus.Completed, job.Status);
            Assert.Null(job.ErrorMessage);
        }

        [Theory]
        [InlineData(JobStatus.Completed)]
        [InlineData(JobStatus.Failed)]
        public async Task Consume_JobAlreadyCompletedOrFailed_DoesNotProcess(JobStatus status)
        {
            // Arrange
            job.Status = status;
            var consumer = CreateConsumer();

            // Act
            await consumer.Consume(contextMock);

            // Assert
            loggerMock.Received().LogInformation("Job is already picked up by a previous process");
            await repoMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Consume_UseCaseFails_FailsAndSavesErrorInDb()
        {
            // Arrange
            var consumer = CreateConsumer();
            useCaseMock.HandleAsync(Arg.Any<Stream>()).Returns(Result.Failure("UseCase failed"));

            // Act
            await consumer.Consume(contextMock);

            // Assert
            Assert.Equal(JobStatus.Failed, job.Status);
            Assert.Equal("UseCase failed with message: UseCase failed", job.ErrorMessage);
        }
    }
}