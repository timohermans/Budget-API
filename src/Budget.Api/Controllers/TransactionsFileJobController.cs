using Budget.Api.Models;
using Budget.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionsFileJobController : ControllerBase
{
    private readonly ITransactionsFileJobRepository _repository;

    public TransactionsFileJobController(ITransactionsFileJobRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id:guid}", Name = "GetTransactionsFileJob")]
    [ProducesResponseType<TransactionsFileJobResponseModel>(StatusCodes.Status200OK)]
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
