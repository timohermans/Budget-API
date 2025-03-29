using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain;
using Budget.Domain.Commands;
using Budget.Domain.Entities;
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
        private readonly ConsumeContext<ProcessTransactionsFile> contextMock;
        private readonly TransactionsFileJob job = new()
        {
            Id = Guid.NewGuid(),
            Status = JobStatus.Pending,
            FileContent = new byte[1000],
            OriginalFileName = "file.txt"
        };


        public ProcessTransactionsFileConsumerTests()
        {
            repoMock = Substitute.For<ITransactionsFileJobRepository>();
            useCaseMock = Substitute.For<ITransactionsFileEtlUseCase>();
            loggerMock = Substitute.For<ILogger<ProcessTransactionsFile>>();
            contextMock = Substitute.For<ConsumeContext<ProcessTransactionsFile>>();
        }

        private ProcessTransactionsFileConsumer CreateConsumer()
        {
            repoMock.GetByIdAsync(Arg.Any<Guid>()).Returns(job);
            contextMock.Message.Returns(new ProcessTransactionsFile { JobId = job.Id });
            return new ProcessTransactionsFileConsumer(repoMock, useCaseMock, loggerMock);
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