using Budget.Domain.Repositories;
using Budget.Api.Models;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class TransactionsController(ITransactionsFileJobStartUseCase useCase, ITransactionRepository transactionRepository) : ControllerBase
{
    [HttpPost("upload")]
    [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        var command = new TransactionsFileJobStartUseCase.Command
        {
            File = new TransactionsFileJobStartUseCase.FileModel
            {
                Size = file.Length,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = GetFileBytesFrom(file)
            }
        };

        var result = await useCase.HandleAsync(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string? iban)
    {
        var transactions = await transactionRepository.GetTransactionsByDateRangeAsync(startDate, endDate, iban);

        var response = transactions.Select(transaction => new TransactionResponseModel
        {
            Id = transaction.Id,
            FollowNumber = transaction.FollowNumber,
            Iban = transaction.Iban,
            Amount = transaction.Amount,
            DateTransaction = transaction.DateTransaction,
            NameOtherParty = transaction.NameOtherParty,
            IbanOtherParty = transaction.IbanOtherParty,
            AuthorizationCode = transaction.AuthorizationCode,
            Description = transaction.Description,
            CashbackForDate = transaction.CashbackForDate
        }).ToList();

        return Ok(response);
    }

    [HttpGet("ibans")]
    public async Task<IActionResult> GetAllDistinctIbans()
    {
        var ibans = await transactionRepository.GetAllDistinctIbansAsync();

        return Ok(ibans);
    }

    [HttpGet("cashflow-per-iban")]
    public async Task<IActionResult> GetCashflowPerIban([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string? iban)
    {
        var cashFlowPerIban = await transactionRepository.GetCashFlowPerIbanAsync(startDate, endDate, iban);

        return Ok(cashFlowPerIban);
    }

    private byte[] GetFileBytesFrom(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}