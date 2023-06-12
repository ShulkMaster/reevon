using Reevon.Api.Models;

namespace Reevon.Api.Parser;

public class ParseResult
{
    public List<Client> Clients { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}