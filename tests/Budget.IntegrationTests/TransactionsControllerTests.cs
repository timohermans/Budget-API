using System.Net.Mime;
using Budget.Api.Controllers;
using Budget.Application.Settings;
using Budget.Application.UseCases;
using Budget.Domain.Commands;
using Budget.Infrastructure.Database.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Budget.IntegrationTests;

public class TransactionsControllerTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact]
    public async Task Upload_CorrectFile_SavesCorrectly()
    {
        var fileStream = new MemoryStream(await File.ReadAllBytesAsync("Data/transactions-1.csv"));
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        object? publishedMessage = null;
        publishEndpoint.When(p => p.Publish<ProcessTransactionsFile>(Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(args => publishedMessage = args.Arg<object>());
        await using var db = fixture.CreateContext();
        await db.Database.BeginTransactionAsync();
        var fileSettings = new FileStorageSettings { BasePath = "/Users/timohermans/Dev/tmp/dump", MaxSizeMb = 10 };
        var controller = new TransactionsController(
            new TransactionsFileJobStartUseCase(
                new TransactionsFileJobRepository(db),
                publishEndpoint,
                NullLogger<TransactionsFileJobStartUseCase>.Instance,
                fileSettings,
                TimeProvider.System
            ));
        var formFile = new FormFile(fileStream, 0, fileStream.Length, "transactions", "transactions.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };

        var result = await controller.Upload(formFile);

        db.ChangeTracker.Clear();
        var job = await db.TransactionsFileJobs.FirstOrDefaultAsync();

        Assert.Equal(typeof(OkObjectResult), result.GetType());
        Assert.NotNull(job);
        await publishEndpoint.Received()
            .Publish<
                ProcessTransactionsFile>(Arg.Any<object>(),
                Arg.Any<CancellationToken>());
        Assert.Null(job.ErrorMessage);
        Assert.Equal("transactions.csv", job.OriginalFileName);
        Assert.True(File.Exists(Path.Combine(fileSettings.BasePath, job.StoredFilePath)));
        File.Delete(Path.Combine(fileSettings.BasePath, job.StoredFilePath));
        Assert.NotNull(publishedMessage);
        Assert.Equivalent(new ProcessTransactionsFile { JobId = job.Id }, publishedMessage);
    }
}