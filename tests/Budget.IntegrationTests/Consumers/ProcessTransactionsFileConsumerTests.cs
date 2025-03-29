using Budget.Application.Settings;
using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain;
using Budget.Domain.Commands;
using Budget.Domain.Entities;
using Budget.Domain.Enums;
using Budget.Infrastructure.Database;
using Budget.Infrastructure.Database.Repositories;
using Budget.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Budget.IntegrationTests.Consumers
{
    public class ProcessTransactionsFileConsumerTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ILogger<ProcessTransactionsFile> loggerMock;
        private readonly FileStorageSettings fileStorageSettings;
        private readonly ConsumeContext<ProcessTransactionsFile> contextMock;
        private readonly TransactionsFileJob job = new TransactionsFileJob
        {
            Id = Guid.NewGuid(),
            Status = JobStatus.Pending,
            FileContent = File.ReadAllBytes("./Data/transactions-1.csv"),
            OriginalFileName = "transactions-1.csv"
        };

        public ProcessTransactionsFileConsumerTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            loggerMock = NullLogger<ProcessTransactionsFile>.Instance;
            contextMock = Substitute.For<ConsumeContext<ProcessTransactionsFile>>();
            fileStorageSettings = fixture.FileStorageSettings;
        }

        private ProcessTransactionsFileConsumer CreateConsumer(BudgetDbContext dbContext)
        {
            var repo = new TransactionsFileJobRepository(dbContext);
            var transactionRepo = new TransactionRepository(dbContext);
            var useCase = new TransactionsFileEtlUseCase(transactionRepo, NullLogger<TransactionsFileEtlUseCase>.Instance);
            contextMock.Message.Returns(new ProcessTransactionsFile { JobId = job.Id });
            return new ProcessTransactionsFileConsumer(repo, useCase, loggerMock);
        }

        [Fact]
        public async Task Consume_GoodJobAndFile_CompletesJobSuccessfully()
        {
            // Arrange
            await using var db = _fixture.CreateContext();
            await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
            await db.TransactionsFileJobs.AddAsync(job, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var consumer = CreateConsumer(db);

            // Act
            await consumer.Consume(contextMock);

            // Assert
            var updatedJob = await db.TransactionsFileJobs.FindAsync([job.Id], TestContext.Current.CancellationToken);
            Assert.NotNull(updatedJob);
            Assert.Equal(JobStatus.Completed, updatedJob.Status);
            Assert.Null(updatedJob.ErrorMessage);

            var transactions = db.Transactions.ToList();
            Assert.Equal(5, transactions.Count); // Assuming there are 5 transactions in the CSV file

            // Additional assertions to verify the transactions
            Assert.Collection(transactions,
                t => Assert.Equal(4000.00m, t.Amount),
                t => Assert.Equal(2000.00m, t.Amount),
                t => Assert.Equal(-800.00m, t.Amount),
                t => Assert.Equal(-1000.00m, t.Amount),
                t => Assert.Equal(-90.75m, t.Amount)
            );
        }
    }
}
