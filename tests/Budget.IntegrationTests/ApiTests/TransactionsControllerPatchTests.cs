using Budget.Api.Controllers;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Application.UseCases.UpdateTransactionCashbackDate;
using Budget.Domain.Entities;
using Budget.Infrastructure.Database;
using Budget.Infrastructure.Database.Repositories;
using Castle.Core.Logging;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Budget.IntegrationTests.ApiTests;

public class TransactionsControllerPatchTests(CustomWebApplicationFactory<Program> fixture) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task UpdateCashbackForDate_ShouldUpdateDateForCashback()
    {
        // Arrange
        await using var db = fixture.CreateContext();
        await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
        
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
        var controller = CreateController(db);
        
        // Act
        var result = await controller.UpdateCashbackForDate(id, newCashbackDate);
        
        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        
        var response = okResult!.Value as UpdateTransactionCashbackDateUseCase.Response;
        Assert.NotNull(response);
        Assert.Equal(response!.Id, id);
        Assert.Equal(newCashbackDate, response.CashbackForDate);
        
        var updatedTransaction = await db.Transactions.FindAsync([id], TestContext.Current.CancellationToken);
        Assert.Equal(newCashbackDate, updatedTransaction!.CashbackForDate);
    }
    
    private TransactionsController CreateController(BudgetDbContext dbContext)
    {
        var transactionRepository = new TransactionRepository(dbContext);
        var fileJobStartUseCase = Substitute.For<ITransactionsFileJobStartUseCase>();
        var updateCashbackDateUseCase = new UpdateTransactionCashbackDateUseCase(transactionRepository);
        
        return new TransactionsController(
            fileJobStartUseCase,
            updateCashbackDateUseCase,
            transactionRepository);
    }
}
