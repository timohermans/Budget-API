using Budget.Api.Controllers;
using Budget.Application.UseCases;
using Budget.Domain.Entities;
using Budget.Domain.Repositories;
using Budget.Infrastructure.Database;
using Budget.Infrastructure.Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Budget.IntegrationTests.ApiTests
{
    public class TransactionsControllerGetTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;

        public TransactionsControllerGetTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private TransactionsController CreateController(BudgetContext dbContext)
        {
            var useCase = Substitute.For<ITransactionsFileJobStartUseCase>();
            var transactionRepo = new TransactionRepository(dbContext);
            return new TransactionsController(useCase, transactionRepo);
        }

        [Fact]
        public async Task GetTransactions_IncludesAndExcludesCorrectDates()
        {
            // Arrange
            await using var db = _fixture.CreateContext();
            await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
                new Transaction { Id = 2, FollowNumber = 2, Iban = "NL01TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
                new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 4, 1), BalanceAfterTransaction = 600 }
            };

            await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var controller = CreateController(db);

            // Act
            var result = await controller.GetTransactions(new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 31), null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTransactions = Assert.IsType<List<Transaction>>(okResult.Value);

            // Assert
            Assert.Equal(2, returnedTransactions.Count);
            Assert.Contains(returnedTransactions, t => t.Id == 1);
            Assert.Contains(returnedTransactions, t => t.Id == 2);
            Assert.DoesNotContain(returnedTransactions, t => t.Id == 3);
        }

        [Fact]
        public async Task GetTransactions_FiltersByIban()
        {
            // Arrange
            await using var db = _fixture.CreateContext();
            await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
                new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
                new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
                new Transaction { Id = 4, FollowNumber = 4, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 4, 3), BalanceAfterTransaction = 600 }
            };

            await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var controller = CreateController(db);

            // Act
            var result = await controller.GetTransactions(new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 31), "NL01TEST");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTransactions = Assert.IsType<List<Transaction>>(okResult.Value);

            // Assert
            Assert.Equal(2, returnedTransactions.Count);
            Assert.Contains(returnedTransactions, t => t.Id == 1);
            Assert.Contains(returnedTransactions, t => t.Id == 3);
            Assert.DoesNotContain(returnedTransactions, t => t.Id == 2);
            Assert.DoesNotContain(returnedTransactions, t => t.Id == 4);
        }

        [Fact]
        public async Task GetAllDistinctIbans_ReturnsDistinctIbans()
        {
            // Arrange
            await using var db = _fixture.CreateContext();
            await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
                new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
                new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 }
            };

            await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var controller = CreateController(db);

            // Act
            var result = await controller.GetAllDistinctIbans();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedIbans = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal(2, returnedIbans.Count);
            Assert.Contains("NL01TEST", returnedIbans);
            Assert.Contains("NL02TEST", returnedIbans);
        }
    }
}
