using System.Xml.Serialization;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Reevon.Api.Contracts.Request;
using Reevon.Api.Contracts.Response;
using Reevon.Api.Models;
using Reevon.Api.Parser;
using Reevon.Api.Validation;

namespace Reevon.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DocumentController : ControllerBase
{
    [HttpPost]
    public IActionResult Xml([FromForm] DocumentParse form)
    {
        var validator = new DocumentParseValidator();
        ValidationResult result = validator.Validate(form);
        if (!result.IsValid)
        {
            ApiError error = ApiError.FromValidation(result);
            return BadRequest(error);
        }
        
        Stream stream = form.Document.OpenReadStream();
        Parser.CsvParser parser = new(stream);
        ParseResult parseResult = parser.Parse();
        if (parseResult.Errors.Any())
        {
            var fileError = new ApiError
            {
                Code = 400,
                Message = "File has errors",
            };
            fileError.ValidationErrors.Add("Document", parseResult.Errors);
            return BadRequest(fileError);
        }
        string content = SerializeToXml(parseResult.Clients);
        return Content(content, "application/xml");
    }
    
    private static string SerializeToXml(List<Client> clients)
    {
        var serializer = new XmlSerializer(typeof(List<Client>));
        using var writer = new StringWriter();
        serializer.Serialize(writer, clients);
        return writer.ToString();
    }

    [HttpPost]
    public IActionResult Json([FromForm] DocumentParse form)
    {
        var validator = new DocumentParseValidator();
        ValidationResult result = validator.Validate(form);
        if (!result.IsValid)
        {
            ApiError error = ApiError.FromValidation(result);
            return BadRequest(error);
        }

        Stream stream = form.Document.OpenReadStream();
        var parser = new Parser.CsvParser(stream);
        ParseResult parseResult = parser.Parse();
        if (parseResult.Errors.Any())
        {
            var fileError = new ApiError
            {
                Code = 400,
                Message = "File has errors",
            };
            fileError.ValidationErrors.Add("Document", parseResult.Errors);
            return BadRequest(fileError);
        }

        return Ok(parseResult.Clients);
    }

    [HttpPost]
    public IActionResult Csv([FromForm] DocumentParse form)
    {
        // TODO: Generate CSV file through XML/JSON 
        return Ok();
    }
    
}