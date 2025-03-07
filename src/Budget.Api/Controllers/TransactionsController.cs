using Budget.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class TransactionsController(ITransactionsFileJobStartUseCase useCase) : ControllerBase
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

    private byte[] GetFileBytesFrom(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}