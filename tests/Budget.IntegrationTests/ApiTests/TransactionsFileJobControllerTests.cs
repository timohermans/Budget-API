using Budget.Api.Controllers;
using Budget.Api.Models;
using Budget.Domain.Entities;
using Budget.Infrastructure.Database;
using Budget.Infrastructure.Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Budget.IntegrationTests.ApiTests;

public class TransactionsFileJobControllerTests : IClassFixture<DatabaseAssemblyFixture>
{
    private readonly DatabaseAssemblyFixture _fixture;

    public TransactionsFileJobControllerTests(DatabaseAssemblyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenJobExists()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        await using var app = await _fixture.CreateApiApp(
            nameof(GetById_ReturnsOk_WhenJobExists),
            TestContext.Current.CancellationToken);
        var (client, db) = app;

        var job = new TransactionsFileJob
        {
            Id = jobId,
            FileContent = [1, 2, 3, 4],
            OriginalFileName = "TestFile.csv",
            CreatedAt = DateTime.UtcNow,
            Status = Domain.Enums.JobStatus.Pending
        };
        db.TransactionsFileJobs.Add(job);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var response = await client.GetAsync($"/TransactionsFileJob/{jobId}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseModel = await response.Content.ReadFromJsonAsync<TransactionsFileJobResponseModel>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(responseModel);
        Assert.Equal(job.Id, responseModel.Id);
        Assert.Equal(job.OriginalFileName, responseModel.OriginalFileName);
        Assert.Equal("Pending", responseModel.Status);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        await using var app = await _fixture.CreateApiApp(
            nameof(GetById_ReturnsNotFound_WhenJobDoesNotExist),
            null,
            TestContext.Current.CancellationToken);
        var (client, _) = app;

        // Act
        var response = await client.GetAsync($"/TransactionsFileJob/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
