using System.Text;
using CsvParser;
using Reevon.Api.Mapping;
using Reevon.Api.Models;

namespace Reevon.Api.Parser;

public class CsvParser
{
    private readonly CsvReader _reader;
    public ClientColumnMap[] _map;

    public CsvParser(Stream ss)
    {
        _reader = new CsvReader(ss, Encoding.UTF8);
        _map = ClientColumnMap.DefaultMap();
    }

    private bool HasCorrectColumnCount()
    {
        _reader.MoveNext();
        ICsvReaderRow? record = _reader.Current;
        return record?.Count == Client.Columns.Length;
    }

    private bool hasHeaders()
    {
        string value = _reader.Current?[0] ?? "";
        return Client.Columns.Any(column => string.Compare(value, column) == 0);
    }
    
    private static bool IsSame(string value, string column)
    {
        return string.Compare(value, column, StringComparison.CurrentCultureIgnoreCase) == 0;
    }
    
    private bool PrepareHeaders()
    {
        var newMap = new ClientColumnMap[_map.Length];
        if (!hasHeaders()) return true;
        for (int index = 0; index < _reader.Current.Count; index++)
        {
            string headName = _reader.Current[index].Trim().ToLower();
            string? header = Client.Columns.FirstOrDefault(column => IsSame(headName, column));
            if (header == null) return false;
            newMap[index] = new ClientColumnMap
            {
                Index = index,
                Name = header
            };
        }
        _map = newMap;
        return true;
    }

    public ParseResult Parse()
    {
        ParseResult result = new();
        if (!HasCorrectColumnCount())
        {
            result.Errors.Add("Invalid number of columns");
            return result;
        }

        if (!PrepareHeaders())
        {
            result.Errors.Add("Error during headers parsing");
            return result;
        }
        return result;
    }
}