using Reevon.Api.Models;

namespace Reevon.Api.Mapping;

public sealed class ClientColumnMap
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;

    public static Dictionary<string, int> DefaultMap()
    {
        Dictionary<string, int> map = new();
        for(int index = 0; index < Client.Columns.Length; index++)
        {
            map.Add(Client.Columns[index], index);
        }

        return map;
    }
}