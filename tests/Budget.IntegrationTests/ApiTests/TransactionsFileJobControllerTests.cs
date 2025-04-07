using Budget.Api.Controllers;
using Budget.Api.Models;
using Budget.Domain.Entities;
using Budget.Infrastructure;
using Budget.Infrastructure.Database;
using Budget.Infrastructure.Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Budget.IntegrationTests.ApiTests;

public class TransactionsFileJobControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _fixture;

    public TransactionsFileJobControllerTests(CustomWebApplicationFactory<Program> fixture)
    {
        _fixture = fixture;
    }

    private TransactionsFileJobController CreateController(BudgetDbContext dbContext)
    {
        var repository = new TransactionsFileJobRepository(dbContext);
        return new TransactionsFileJobController(repository);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenJobExists()
    {
        // Arrange
        await using var db = _fixture.CreateContext();
        await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var job = new TransactionsFileJob
        {
            Id = Guid.NewGuid(),
            FileContent = new byte[] { 1, 2, 3, 4 },
            OriginalFileName = "TestFile.csv",
            CreatedAt = DateTime.UtcNow,
            Status = Domain.Enums.JobStatus.Pending
        };
        db.TransactionsFileJobs.Add(job);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        db.ChangeTracker.Clear();

        var controller = CreateController(db);

        // Act
        var result = await controller.GetById(job.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseModel = Assert.IsType<TransactionsFileJobResponseModel>(okResult.Value);
        Assert.Equal(job.Id, responseModel.Id);
        Assert.Equal(job.OriginalFileName, responseModel.OriginalFileName);
        Assert.Equal("Pending", responseModel.Status);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        await using var db = _fixture.CreateContext();
        await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        var controller = CreateController(db);

        // Act
        var result = await controller.GetById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
