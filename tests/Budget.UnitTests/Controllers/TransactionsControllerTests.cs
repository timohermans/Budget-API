using Budget.Api.Controllers;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Application.UseCases.UpdateTransactionCashbackDate;
using Budget.Domain;
using Budget.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Budget.UnitTests.Controllers;

public class TransactionsControllerTests
{
    private readonly ITransactionsFileJobStartUseCase _useCaseSubstitute;
    private readonly TransactionsController _controller;

    public TransactionsControllerTests()
    {
        _useCaseSubstitute = Substitute.For<ITransactionsFileJobStartUseCase>();
        _controller = new TransactionsController(_useCaseSubstitute, Substitute.For<IUpdateTransactionCashbackDateUseCase>(), Substitute.For<ITransactionRepository>());
    }

    [Fact]
    public async Task Upload_WhenFileIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Upload(null!);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file was uploaded.", badRequest.Value);
    }

    [Fact]
    public async Task Upload_WhenFileIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var fileSubstitute = Substitute.For<IFormFile>();
        fileSubstitute.Length.Returns(0);

        // Act
        var result = await _controller.Upload(fileSubstitute);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file was uploaded.", badRequest.Value);
    }

    [Fact]
    public async Task Upload_WithValidFile_ReturnsOkResult()
    {
        // Arrange
        var content = "test content";
        var fileName = "test.csv";
        var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var fileSubstitute = Substitute.For<IFormFile>();
        fileSubstitute.FileName.Returns(fileName);
        fileSubstitute.ContentType.Returns("text/csv");
        fileSubstitute.Length.Returns(ms.Length);
        fileSubstitute.OpenReadStream().Returns(ms);

        _useCaseSubstitute.HandleAsync(Arg.Any<TransactionsFileJobStartCommand>())
            .Returns(Task.FromResult(Result<TransactionsFileJobStartResponse>.Success(
                new TransactionsFileJobStartResponse
                {
                    JobId = Guid.NewGuid(),
                })));

        // Act
        var result = await _controller.Upload(fileSubstitute);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        await _useCaseSubstitute.Received(1)
            .HandleAsync(Arg.Is<TransactionsFileJobStartCommand>(c =>
                c.File.FileName == fileName &&
                c.File.ContentType == "text/csv" &&
                c.File.Size == ms.Length
            ));
    }

    [Fact]
    public async Task Upload_WhenUseCaseFails_ReturnsBadRequest()
    {
        // Arrange
        var fileSubstitute = Substitute.For<IFormFile>();
        fileSubstitute.Length.Returns(100);
        fileSubstitute.FileName.Returns("valid.csv");
        fileSubstitute.ContentType.Returns("text/csv");
        fileSubstitute.OpenReadStream().Returns(new MemoryStream());
        var errorMessage = "Processing failed";
        _useCaseSubstitute.HandleAsync(Arg.Any<TransactionsFileJobStartCommand>())
            .Returns(Task.FromResult(Result<TransactionsFileJobStartResponse>.Failure(errorMessage)));

        // Act
        var result = await _controller.Upload(fileSubstitute);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequest.Value);
    }
}