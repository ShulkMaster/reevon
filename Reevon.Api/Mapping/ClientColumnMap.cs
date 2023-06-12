using Reevon.Api.Models;

namespace Reevon.Api.Mapping;

public sealed class ClientColumnMap
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;

    public static ClientColumnMap[] DefaultMap()
    {
        return Client.Columns.Select((name, index) => new ClientColumnMap
        {
            Index = index,
            Name = name,
        }).ToArray();
    }
}