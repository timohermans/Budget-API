using Budget.Api.Models;
using Budget.Domain.Entities;
using Budget.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace Budget.IntegrationTests.ApiTests;

public class TransactionsControllerGetTests : IClassFixture<DatabaseAssemblyFixture>
{
    private readonly DatabaseAssemblyFixture _fixture;

    public TransactionsControllerGetTests(DatabaseAssemblyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetTransactions_IncludesAndExcludesCorrectDates()
    {
        // Arrange
        var clientFactory = await CustomWebApplicationFactory<Program>.CreateApiClientAsync(_fixture.ConnectionString, nameof(GetTransactions_IncludesAndExcludesCorrectDates));
        var client = clientFactory.CreateClient();

        using var scope = clientFactory.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();

        var transactions = new List<Transaction>
        {
                new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
                new Transaction { Id = 2, FollowNumber = 2, Iban = "NL01TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
                new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 4, 1), BalanceAfterTransaction = 600 }
        };

        db.Transactions.AddRange(transactions);
        db.SaveChanges();

        // Act
        var result = await client.GetAsync($"/transactions?startDate={new DateOnly(2025, 3, 1).ToString("yyyy-MM-dd")}&endDate={new DateOnly(2025, 3, 31).ToString("yyyy-MM-dd")}", TestContext.Current.CancellationToken);

        result.EnsureSuccessStatusCode();

        // Assert
        var returnedTransactions = await result.Content.ReadFromJsonAsync<List<TransactionResponseModel>>(TestContext.Current.CancellationToken);
        Assert.NotNull(returnedTransactions);
        Assert.Equal(2, returnedTransactions.Count);
        Assert.Contains(returnedTransactions, t => t.Id == 1);
        Assert.Contains(returnedTransactions, t => t.Id == 2);
        Assert.DoesNotContain(returnedTransactions, t => t.Id == 3);
    }

    // TODO: adjust the rest of the tests

    //[Fact]
    //public async Task GetTransactions_FiltersByIban()
    //{
    //    // Arrange
    //    await using var db = _fixture.CreateContext();
    //    await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

    //    var transactions = new List<Transaction>
    //    {
    //        new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
    //        new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
    //        new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
    //        new Transaction { Id = 4, FollowNumber = 4, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 4, 3), BalanceAfterTransaction = 600 }
    //    };

    //    await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
    //    await db.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var controller = CreateClient(db);

    //    // Act
    //    var result = await controller.GetTransactions(new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 31), "NL01TEST");
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    var returnedTransactions = Assert.IsType<List<TransactionResponseModel>>(okResult.Value);

    //    // Assert
    //    Assert.Equal(2, returnedTransactions.Count);
    //    Assert.Contains(returnedTransactions, t => t.Id == 1);
    //    Assert.Contains(returnedTransactions, t => t.Id == 3);
    //    Assert.DoesNotContain(returnedTransactions, t => t.Id == 2);
    //    Assert.DoesNotContain(returnedTransactions, t => t.Id == 4);
    //}

    //[Fact]
    //public async Task GetAllDistinctIbans_ReturnsDistinctIbans()
    //{
    //    // Arrange
    //    await using var db = _fixture.CreateContext();
    //    await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

    //    var transactions = new List<Transaction>
    //    {
    //        new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
    //        new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
    //        new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 }
    //    };

    //    await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
    //    await db.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var controller = CreateClient(db);

    //    // Act
    //    var result = await controller.GetAllDistinctIbans();

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    var returnedIbans = Assert.IsType<List<string>>(okResult.Value);
    //    Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
    //    Assert.Equal(2, returnedIbans.Count);
    //    Assert.Contains("NL01TEST", returnedIbans);
    //    Assert.Contains("NL02TEST", returnedIbans);
    //}

    //[Fact]
    //public async Task GetAllDistinctIbans_ReturnsDistinctIbansOrderedByFrequency()
    //{
    //    // Arrange
    //    await using var db = _fixture.CreateContext();
    //    await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

    //    var transactions = new List<Transaction>
    //    {
    //        new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
    //        new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
    //        new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
    //        new Transaction { Id = 4, FollowNumber = 4, Iban = "NL03TEST", Currency = "EUR", Amount = 400, DateTransaction = new DateOnly(2025, 3, 4), BalanceAfterTransaction = 1000 },
    //        new Transaction { Id = 5, FollowNumber = 5, Iban = "NL01TEST", Currency = "EUR", Amount = 500, DateTransaction = new DateOnly(2025, 3, 5), BalanceAfterTransaction = 1500 }
    //    };

    //    await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
    //    await db.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var controller = CreateClient(db);

    //    // Act
    //    var result = await controller.GetAllDistinctIbans();

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    var returnedIbans = Assert.IsType<List<string>>(okResult.Value);
    //    Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
    //    Assert.Equal(3, returnedIbans.Count);
    //    Assert.Equal("NL01TEST", returnedIbans[0]);
    //    Assert.Equal("NL02TEST", returnedIbans[1]);
    //    Assert.Equal("NL03TEST", returnedIbans[2]);
    //}

    //[Fact]
    //public async Task GetCashFlowPerIbanAsync_ReturnsCorrectCashflow()
    //{
    //    // Arrange
    //    await using var db = _fixture.CreateContext();
    //    await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

    //    var transactions = new List<Transaction>
    //    {
    //        new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
    //        new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
    //        new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
    //        new Transaction { Id = 4, FollowNumber = 4, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 400 },
    //        new Transaction { Id = 5, FollowNumber = 5, Iban = "NL03TEST", Currency = "EUR", Amount = 400, DateTransaction = new DateOnly(2025, 3, 4), BalanceAfterTransaction = 1000 },
    //        new Transaction { Id = 6, FollowNumber = 6, Iban = "NL01TEST", Currency = "EUR", Amount = 500, DateTransaction = new DateOnly(2025, 3, 5), BalanceAfterTransaction = 1500 }
    //    };

    //    await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
    //    await db.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var controller = CreateClient(db);

    //    // Act
    //    var actionResult = await controller.GetCashflowPerIban(new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 31), null);

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    //    var result = Assert.IsType<CashflowDto>(okResult.Value);
    //    Assert.Equal("NL01TEST", result.Iban);
    //    Assert.Equal(3, result.BalancesPerDate.Count());
    //    Assert.Collection(result.BalancesPerDate,
    //        bpd =>
    //        {
    //            Assert.Equal(new DateOnly(2025, 3, 1), bpd.Date);
    //            Assert.Equal(100, bpd.Balance);
    //        },
    //        bpd =>
    //        {
    //            Assert.Equal(new DateOnly(2025, 3, 3), bpd.Date);
    //            Assert.Equal(400, bpd.Balance);
    //        },
    //        bpd =>
    //        {
    //            Assert.Equal(new DateOnly(2025, 3, 5), bpd.Date);
    //            Assert.Equal(1500, bpd.Balance);
    //        });
    //}
}
