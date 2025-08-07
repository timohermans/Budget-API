using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Domain.Commands;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Budget.IntegrationTests.ApiTests;

public class TransactionsControllerUploadTests(DatabaseAssemblyFixture fixture) : IClassFixture<DatabaseAssemblyFixture>
{
    [Fact]
    public async Task Upload_CorrectFile_SavesCorrectly()
    {
        object? publishedMessage = null;
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        publishEndpoint.When(p => p.Publish<ProcessTransactionsFile>(Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(args => publishedMessage = args.Arg<object>());

        await using var app = await fixture.CreateApiApp(
            nameof(Upload_CorrectFile_SavesCorrectly),
            services =>
            {
                services.AddSingleton(publishEndpoint);
            },
            TestContext.Current.CancellationToken);
        var (client, db) = app;

        var fileStream = new MemoryStream(await File.ReadAllBytesAsync("Data/transactions-1.csv", TestContext.Current.CancellationToken));
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        using var formData = new MultipartFormDataContent
        {
            { fileContent, "file", "transactions.csv" }
        };

        var response = await client.PostAsync("/transactions/upload", formData, TestContext.Current.CancellationToken); // how to add a file 

        response.EnsureSuccessStatusCode();

        var jobResponse = await response.Content.ReadFromJsonAsync<TransactionsFileJobStartResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(jobResponse);
        var job = await db.TransactionsFileJobs.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(job);
        Assert.Equal(job.Id, jobResponse?.JobId);
        await publishEndpoint.Received()
            .Publish<ProcessTransactionsFile>(Arg.Any<object>(), Arg.Any<CancellationToken>());
        Assert.Null(job.ErrorMessage);
        Assert.Equal("transactions.csv", job.OriginalFileName);
        Assert.True(fileStream.ToArray().SequenceEqual(job.FileContent));
        Assert.NotNull(publishedMessage);
        Assert.Equivalent(new ProcessTransactionsFile { JobId = job.Id }, publishedMessage);
    }
}