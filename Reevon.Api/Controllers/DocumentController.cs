using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
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
    public IActionResult Xml([FromForm] DocumentCSVParse form)
    {
        var validator = new CSVParseValidator();
        ValidationResult result = validator.Validate(form);
        if (!result.IsValid)
        {
            ApiError error = ApiError.FromValidation(result);
            return BadRequest(error);
        }

        Stream stream = form.Document.OpenReadStream();
        Parser.CsvParser parser = new(stream);
        ParseResult parseResult = parser.Parse(form.Key);
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
    public IActionResult Json([FromForm] DocumentCSVParse form)
    {
        var validator = new CSVParseValidator();
        ValidationResult result = validator.Validate(form);
        if (!result.IsValid)
        {
            ApiError error = ApiError.FromValidation(result);
            return BadRequest(error);
        }

        Stream stream = form.Document.OpenReadStream();
        var parser = new Parser.CsvParser(stream);
        ParseResult parseResult = parser.Parse(form.Key);
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
    public IActionResult CsvXml([FromForm] DocumentXMLParse form)
    {
        var validator = new XMLParseValidator();
        ValidationResult result = validator.Validate(form);
        if (!result.IsValid)
        {
            ApiError error = ApiError.FromValidation(result);
            return BadRequest(error);
        }

        Stream stream = form.Document.OpenReadStream();
        
        string csvContent = ConvertXmlToCsv("stream");
        
        // stream -> XMLLibrary () => List<CLients>
        // list iterarte -> decrypt creadit card prop
        // csv Library to auto output csv string
        // return csvContent
        
        return Content(csvContent, "text/csv");
    }

    [HttpPost]
    public IActionResult CsvJson([FromForm] DocumentJSONParse form)
    {
        var validator = new JSONParseValidator();
        ValidationResult result = validator.Validate(form);
        if (!result.IsValid)
        {
            ApiError error = ApiError.FromValidation(result);
            return BadRequest(error);
        }

        Stream stream = form.Document.OpenReadStream();
        try
        {
            var clients = JsonSerializer.Deserialize<List<Client>>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
            return Ok(clients);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ApiError error = ApiError.FromString("The supplied JSON file is not valid");
            return BadRequest(error);
        }
    }

    private static string ConvertXmlToCsv(string xmlContent)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        var csvBuilder = new StringBuilder();

        csvBuilder.AppendLine("Document,Name,LastName,Card,Rank,Phone,Poligone");

        XmlNodeList? clientNodes = xmlDoc.SelectNodes("//Client");

        foreach (XmlNode clientNode in clientNodes)
        {
            string document = clientNode.SelectSingleNode("Document")?.InnerText ?? "";
            string firstName = clientNode.SelectSingleNode("Name")?.InnerText ?? "";
            string lastName = clientNode.SelectSingleNode("LastName")?.InnerText ?? "";
            string card = clientNode.SelectSingleNode("Card")?.InnerText ?? "";
            string type = clientNode.SelectSingleNode("Rank")?.InnerText ?? "";
            string phone = clientNode.SelectSingleNode("Phone")?.InnerText ?? "";
            string polygon = clientNode.SelectSingleNode("Polygone")?.InnerText ?? "";

            csvBuilder.AppendLine($"{document},{firstName},{lastName},{card},{type},{phone},{polygon}");
        }

        return csvBuilder.ToString();
    }

    private static string ConvertJsonToCsv(string jsonContent)
    {
        List<Client>? clients = JsonSerializer.Deserialize<List<Client>>(jsonContent);

        var csvBuilder = new StringBuilder();

        csvBuilder.AppendLine("Document,Name,LastName,Card,Rank,Phone,Poligone");

        foreach (Client client in clients)
        {
            string document = client.Document ?? "";
            string name = client.Name ?? "";
            string lastName = client.LastName ?? "";
            string card = client.Card ?? "";
            string rank = client.Rank ?? "";
            string phone = client.Phone ?? "";
            string poligone = client.Poligone ?? "";

            csvBuilder.AppendLine($"{document},{name},{lastName},{card},{rank},{phone},{poligone}");
        }

        return csvBuilder.ToString();
    }
}