using Budget.Api.Models;
using Budget.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsFileJobController : ControllerBase
{
    private readonly ITransactionsFileJobRepository _repository;

    public TransactionsFileJobController(ITransactionsFileJobRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var job = await _repository.GetByIdAsync(id);
        if (job == null)
        {
            return NotFound("TransactionsFileJob not found.");
        }

        return Ok(new TransactionsFileJobResponseModel(job));
    }
}
