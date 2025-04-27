using Budget.Application.UseCases.UpdateTransactionCashbackDate;
using Budget.Domain.Entities;
using System.Net.Http.Json;

namespace Budget.IntegrationTests.ApiTests;

public class TransactionsControllerPatchTests(DatabaseAssemblyFixture fixture) : IClassFixture<DatabaseAssemblyFixture>
{
    [Fact]
    public async Task UpdateCashbackForDate_ShouldUpdateDateForCashback()
    {
        // Arrange
        await using var app = await fixture.CreateApiApp(nameof(UpdateCashbackForDate_ShouldUpdateDateForCashback), TestContext.Current.CancellationToken);
        var (client, db) = app;

        var transaction = new Transaction
        {
            FollowNumber = 1,
            Iban = "NL12ABCD3456789012",
            Currency = "EUR",
            Amount = 100,
            DateTransaction = new DateOnly(2023, 1, 1),
            BalanceAfterTransaction = 500,
            Description = "Test transaction",
            CashbackForDate = null
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        db.ChangeTracker.Clear();

        var id = transaction.Id;

        var newCashbackDate = new DateOnly(2023, 1, 15);
        // Act
        var result = await client.PatchAsJsonAsync($"/transactions/{id}/cashback-date", new { CashbackForDate = newCashbackDate.ToString("yyyy-MM-dd") }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<UpdateTransactionCashbackDateUseCase.Response>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        Assert.Equal(response!.Id, id);
        Assert.Equal(newCashbackDate, response.CashbackForDate);

        var updatedTransaction = await db.Transactions.FindAsync([id], TestContext.Current.CancellationToken);
        Assert.Equal(newCashbackDate, updatedTransaction!.CashbackForDate);
    }
}
