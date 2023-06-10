using Microsoft.AspNetCore.Mvc;
using Reevon.Api.Contracts.Request;

namespace Reevon.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(ILogger<DocumentController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("[action]")]
    public DocumentParse Xml([FromForm] DocumentParse form)
    {
        return form;
        // return new DocumentParse
        // {
        //     Separator = form["separator"],
        //     Key = form["key"],
        // };
    }
}