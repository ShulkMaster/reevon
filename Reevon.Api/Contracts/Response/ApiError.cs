namespace Reevon.Api.Contracts.Response;

public sealed class ApiError
{
    public int Code { get; set; }
    public string Message { get; set; } = "";
    public Dictionary<string, IEnumerable<string>> ValidationErrors { get; set; } = new();
}