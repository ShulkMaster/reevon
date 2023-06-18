using System.Text;
using System.Text.Json;
using System.Xml;
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

        Stream stream = form.Document.OpenReadStream();
        Parser.CsvParser parser = new(stream,form.Separator);
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
        var parser = new Parser.CsvParser(stream,form.Separator);
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
        if (form.Document.ContentType != "application/xml")
        {
            ApiError error = ApiError.FromString("The supplied file is not an XML");
            return BadRequest(error);
        }

        try
        {
            var encoding = Encoding.GetEncoding("UTF-16");
            var streamReader = new StreamReader(form.Document.OpenReadStream(), encoding);
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(streamReader);

            var arrayOfClientElement = xmlDocument.DocumentElement;
            var clientNodes = arrayOfClientElement.GetElementsByTagName("Client");

            var clients = new List<Client>();
            foreach (XmlNode clientNode in clientNodes)
            {
                var clientSerializer = new XmlSerializer(typeof(Client));
                var client = (Client)clientSerializer.Deserialize(new XmlNodeReader(clientNode));
                clients.Add(client);
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