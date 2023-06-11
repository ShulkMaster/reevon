using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Reevon.Api.Contracts.Request;

namespace Reevon.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DocumentController : ControllerBase
{
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(ILogger<DocumentController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult  Xml([FromForm] DocumentParse form)
    {
        string xmlContent = @$"
<root>
    <name>John Doe</name>
    <age>30</age>
    <separator>{form.Separator}</separator>
    <key>{form.Key}</key>
    <file>{form.Document?.FileName}</file>
</root>";
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlContent);
        return Content(xmlDocument.OuterXml, "application/xml");
    }
    
    [HttpPost]
    public IActionResult  Json([FromForm] DocumentParse form)
    {
        var jsonContent = new
        {
            name = "John Doe",
            age = 30,
            separator = form.Separator,
            key = form.Key,
            file = form.Document?.FileName
        };
        return Ok(jsonContent);
    }
}