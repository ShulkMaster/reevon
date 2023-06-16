using System.Text.Json;
using Reevon.Api.Mapping;
using Reevon.Api.Models;

namespace Reevon.Api.Parser
{
    public class JsonParser
    {
        private readonly StreamReader _reader;
        private Dictionary<string, int> _map;

        public JsonParser(Stream stream)
        {
            _reader = new StreamReader(stream);
            _map = ClientColumnMap.DefaultMap();
        }

        public ParseResult Parse()
        {
            ParseResult result = new ParseResult();
            string json = _reader.ReadToEnd(); // Leer el contenido del flujo y convertirlo en una cadena
            List<Client> clients = JsonSerializer.Deserialize<List<Client>>(json);

            foreach (Client client in clients)
            {
                SetOtherFields(client);
                result.Clients.Add(client);
            }

            return result;
        }

        private void SetOtherFields(Client client)
        {
            foreach (var kvp in _map)
            {
                string propertyName = kvp.Key;
                int columnIndex = kvp.Value;

                string propertyValue = GetPropertyValue(propertyName);
                SetProperty(client, propertyName, propertyValue);
            }
        }

        private string GetPropertyValue(string propertyName)
        {
            string[] propertyPath = propertyName.Split('.');
            using (var jsonDocument = JsonDocument.Parse(_reader.BaseStream))
            {
                JsonElement propertyElement = jsonDocument.RootElement;

                foreach (string path in propertyPath)
                {
                    propertyElement = propertyElement.GetProperty(path);
                }

                return propertyElement.GetString();
            }
        }

        private void SetProperty(Client client, string propertyName, string propertyValue)
        {
            switch (propertyName)
            {
                case nameof(Client.Document):
                    client.Document = propertyValue;
                    break;
                case nameof(Client.Name):
                    client.Name = propertyValue;
                    break;
                case nameof(Client.LastName):
                    client.LastName = propertyValue;
                    break;
                case nameof(Client.Card):
                    client.Card = propertyValue;
                    break;
                case nameof(Client.Rank):
                    client.Rank = propertyValue;
                    break;
                case nameof(Client.Phone):
                    client.Phone = propertyValue;
                    break;
                case nameof(Client.Poligone):
                    client.Poligone = propertyValue;
                    break;
            }
        }
    }
}
