using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Reevon.Api.Encryption;

namespace Reevon.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class KeyController : ControllerBase
{
    [HttpPost]
    public IActionResult Generate([FromBody] JsonElement requestBody)
    {
        if (requestBody.TryGetProperty("id", out JsonElement idElement) && idElement.ValueKind == JsonValueKind.String &&
            requestBody.TryGetProperty("secret", out JsonElement secretElement) && secretElement.ValueKind == JsonValueKind.String)
        {
            string? id = idElement.GetString();
            string? secret = secretElement.GetString();

            string result = KeyGenerator.GenerateKeyPair(id, secret);

            return Ok(result);
        }

        return BadRequest("Invalid request body");
    }
}
