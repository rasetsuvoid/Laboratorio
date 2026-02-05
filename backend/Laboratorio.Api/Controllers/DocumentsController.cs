using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laboratorio.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(Policy = "Session")]
public sealed class DocumentsController : ControllerBase
{
    [HttpGet("getDocuments")]
    public IActionResult GetDocuments()
    {
        var documents = new[]
        {
            new DocumentType("CC", "Cédula de ciudadanía"),
            new DocumentType("CE", "Cédula de extranjería"),
            new DocumentType("TI", "Tarjeta de identidad"),
            new DocumentType("NIT", "Número de identificación tributaria"),
            new DocumentType("PAS", "Pasaporte")
        };

        return Ok(documents);
    }
}

public sealed record DocumentType(string Code, string Name);
