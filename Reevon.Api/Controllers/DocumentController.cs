using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Reevon.Api.Contracts.Request;
using Reevon.Api.Contracts.Response;
using Reevon.Api.Helper;
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

        char separator = form.Separator[0];
        Stream stream = form.Document.OpenReadStream();
        Parser.CsvParser parser = new(stream, separator);
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
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
        };
        using var mmStream = new MemoryStream();
        using var xmlWriter = XmlWriter.Create(mmStream, settings);
        var serializer = new XmlSerializer(typeof(List<Client>));
        serializer.Serialize(xmlWriter, clients);
        return Encoding.UTF8.GetString(mmStream.ToArray());
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

        char separator = form.Separator[0];
        Stream stream = form.Document.OpenReadStream();
        var parser = new Parser.CsvParser(stream, separator);
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

        var clients = new List<Client>();

        try
        {
            var xmlDocument = XDocument.Load(form.Document.OpenReadStream());
            var descendants = xmlDocument.Descendants("Client");
            foreach (XElement clientElement in descendants)
            {
                string card = clientElement.Element("Card")?.Value ?? "";
                var client = new Client
                {
                    Document = clientElement.Element("Document")?.Value ?? "",
                    Name = clientElement.Element("Name")?.Value ?? "",
                    LastName = clientElement.Element("LastName")?.Value ?? "",
                    Rank = clientElement.Element("Rank")?.Value ?? "",
                    Phone = clientElement.Element("Phone")?.Value ?? "",
                    Poligone = clientElement.Element("Poligone")?.Value ?? "",
                    Card = EncryptionHelper.Decrypt(card, form.Key),
                };
                clients.Add(client);
            }

            var parser = new CVSWriter<Client>(clients)
            {
                Separator = form.Separator[0]
            };
            string dataString = parser.Write();

            return Content(dataString, "text/csv");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ApiError error = ApiError.FromString("The supplied XML file is not valid");
            return BadRequest(error);
        }
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

            if (clients is null)
            {
                ApiError error = ApiError.FromString("Could not parse the json");
                return BadRequest(error);
            }

            foreach (Client client in clients)
            {
                client.Card = EncryptionHelper.Decrypt(client.Card, form.Key);
            }

            var parser = new CVSWriter<Client>(clients)
            {
                Separator = form.Separator[0]
            };
            string dataString = parser.Write();

            return Content(dataString, "text/csv");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ApiError error = ApiError.FromString("The supplied JSON file is not valid");
            return BadRequest(error);
        }
    }
}