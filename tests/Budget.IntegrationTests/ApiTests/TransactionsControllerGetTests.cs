using Budget.Api.Models;
using Budget.Domain.Contracts;
using Budget.Domain.Entities;
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
        await using var app = await _fixture.CreateApiApp(nameof(GetTransactions_IncludesAndExcludesCorrectDates), TestContext.Current.CancellationToken);
        var (client, db) = app;

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

    [Fact]
    public async Task GetTransactions_FiltersByIban()
    {
        // Arrange
        await using var app = await _fixture.CreateApiApp(nameof(GetTransactions_IncludesAndExcludesCorrectDates), TestContext.Current.CancellationToken);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
            new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
            new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
            new Transaction { Id = 4, FollowNumber = 4, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 4, 3), BalanceAfterTransaction = 600 }
        };

        await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await client.GetAsync($"/transactions?startDate={new DateOnly(2025, 3, 1).ToString("yyyy-MM-dd")}&endDate={new DateOnly(2025, 3, 31).ToString("yyyy-MM-dd")}&iban=NL01TEST", TestContext.Current.CancellationToken);

        result.EnsureSuccessStatusCode();

        var returnedTransactions = await result.Content.ReadFromJsonAsync<List<TransactionResponseModel>>(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(returnedTransactions);
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
        await using var app = await _fixture.CreateApiApp(nameof(GetTransactions_IncludesAndExcludesCorrectDates), TestContext.Current.CancellationToken);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
            new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
            new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 }
        };

        await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await client.GetAsync("/transactions/ibans", TestContext.Current.CancellationToken);

        // Assert
        var ibansResult = await result.Content.ReadFromJsonAsync<IEnumerable<string>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(ibansResult);
        Assert.Equal(2, ibansResult.Count());
        Assert.Contains("NL01TEST", ibansResult);
        Assert.Contains("NL02TEST", ibansResult);
    }

    [Fact]
    public async Task GetAllDistinctIbans_ReturnsDistinctIbansOrderedByFrequency()
    {
        // Arrange
        await using var app = await _fixture.CreateApiApp(nameof(GetTransactions_IncludesAndExcludesCorrectDates), TestContext.Current.CancellationToken);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
            new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
            new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
            new Transaction { Id = 4, FollowNumber = 4, Iban = "NL03TEST", Currency = "EUR", Amount = 400, DateTransaction = new DateOnly(2025, 3, 4), BalanceAfterTransaction = 1000 },
            new Transaction { Id = 5, FollowNumber = 5, Iban = "NL01TEST", Currency = "EUR", Amount = 500, DateTransaction = new DateOnly(2025, 3, 5), BalanceAfterTransaction = 1500 }
        };

        await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await client.GetAsync("/transactions/ibans", TestContext.Current.CancellationToken);

        // Assert
        result.EnsureSuccessStatusCode();
        var returnedIbans = await result.Content.ReadFromJsonAsync<IEnumerable<string>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(returnedIbans);
        Assert.Equal(3, returnedIbans.Count());
        Assert.Equal("NL01TEST", returnedIbans.ElementAt(0));
        Assert.Equal("NL02TEST", returnedIbans.ElementAt(1));
        Assert.Equal("NL03TEST", returnedIbans.ElementAt(2));
    }

    [Fact]
    public async Task GetCashFlowPerIbanAsync_ReturnsCorrectCashflow()
    {
        // Arrange
        await using var app = await _fixture.CreateApiApp(nameof(GetTransactions_IncludesAndExcludesCorrectDates), TestContext.Current.CancellationToken);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
            new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
            new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
            new Transaction { Id = 4, FollowNumber = 4, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 400 },
            new Transaction { Id = 5, FollowNumber = 5, Iban = "NL03TEST", Currency = "EUR", Amount = 400, DateTransaction = new DateOnly(2025, 3, 4), BalanceAfterTransaction = 1000 },
            new Transaction { Id = 6, FollowNumber = 6, Iban = "NL01TEST", Currency = "EUR", Amount = 500, DateTransaction = new DateOnly(2025, 3, 5), BalanceAfterTransaction = 1500 }
        };

        await db.Transactions.AddRangeAsync(transactions, TestContext.Current.CancellationToken);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var actionResult = await client.GetAsync($"/transactions/cashflow-per-iban?startDate={new DateOnly(2025, 3, 1).ToString("yyyy-MM-dd")}&endDate={new DateOnly(2025, 3, 31).ToString("yyyy-MM-dd")}", TestContext.Current.CancellationToken);

        // Assert
        actionResult.EnsureSuccessStatusCode();
        var result = await actionResult.Content.ReadFromJsonAsync<CashflowDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("NL01TEST", result.Iban);
        Assert.Equal(3, result.BalancesPerDate.Count());
        Assert.Collection(result.BalancesPerDate,
            bpd =>
            {
                Assert.Equal(new DateOnly(2025, 3, 1), bpd.Date);
                Assert.Equal(100, bpd.Balance);
            },
            bpd =>
            {
                Assert.Equal(new DateOnly(2025, 3, 3), bpd.Date);
                Assert.Equal(400, bpd.Balance);
            },
            bpd =>
            {
                Assert.Equal(new DateOnly(2025, 3, 5), bpd.Date);
                Assert.Equal(1500, bpd.Balance);
            });
    }
}
