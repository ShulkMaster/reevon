using System.Text;
using CsvParser;
using Reevon.Api.Mapping;
using Reevon.Api.Models;

namespace Reevon.Api.Parser;

using ColumIndex = Dictionary<string, int>;

public class CsvParser
{
    private readonly CsvReader _reader;
    private ColumIndex _map;

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

    private bool HasHeaders()
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
        ColumIndex newMap = new();
        if (!HasHeaders()) return true;
        for (int index = 0; index < _reader.Current.Count; index++)
        {
            string headName = _reader.Current[index].Trim().ToLower();
            string? header = Client.Columns.FirstOrDefault(column => IsSame(headName, column));
            if (header == null || newMap.ContainsKey(header)) return false;
            newMap.Add(header, index);
        }

        _map = newMap;
        _reader.MoveNext();
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

        do
        {
            ICsvReaderRow? row = _reader.Current;
            if (row == null) continue;
            var client = new Client();
            SetDocument(client, row);
            SetNames(client, row);
            SetCard(client, row);
            SetOtherFields(client, row);
            result.Clients.Add(client);
        } while (_reader.MoveNext());

        return result;
    }

    private void SetDocument(Client client, ICsvReaderRow row)
    {
        int index = _map[Client.DocumentColumn];
        client.Document = row[index].Trim();
    }
    
    private void SetNames(Client client, ICsvReaderRow row)
    {
        int index = _map[Client.NameColumn];
        client.Name = row[index].Trim();
        index = _map[Client.LastNameColumn];
        client.LastName = row[index].Trim();
    }
    
    private void SetCard(Client client, ICsvReaderRow row)
    {
        // Todo add encryption
        int index = _map[Client.CardColumn];
        client.Card = row[index].Trim();
    }
    
    private void SetOtherFields(Client client, ICsvReaderRow row)
    {
        int index = _map[Client.RankColumn];
        client.Rank = row[index].Trim();
        index = _map[Client.PhoneColumn];
        client.Phone = row[index].Trim();
        index = _map[Client.PoligoneColumn];
        client.Poligone = row[index].Trim();
    }
}