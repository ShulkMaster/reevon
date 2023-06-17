namespace Reevon.Api.Contracts.Request;

public class DocumentXMLParse
{
    public string Separator { get; set; } = "";
    public string Key { get; set; } = "";
    public IFormFile Document { get; set; } = null!;
}