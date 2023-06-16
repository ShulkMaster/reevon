using System.Xml;
using Reevon.Api.Mapping;
using Reevon.Api.Models;
using Reevon.Api.System;

namespace Reevon.Api.Parser
{
    using ColumIndex = Dictionary<string, int>;

    public class XmlParser
    {
        private readonly XmlReader _xmlReader;
        private ColumIndex _map;

        public XmlParser(Stream ss)
        {
            _xmlReader = XmlReader.Create(ss);
            _map = ClientColumnMap.DefaultMap();
        }

        private bool HasCorrectElementCount()
        {
            return _xmlReader.ReadToFollowing("Client");
        }

        private bool HasHeaders()
        {
            return true;
        }

        private static bool IsSame(string value, string column)
        {
            return string.Equals(value, column, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool PrepareHeaders()
        {
            ColumIndex newMap = new();
            if (!HasHeaders()) return true;
            
            return false;
        }

        public ParseResult Parse(string key)
        {
            ParseResult result = new();
            if (!HasCorrectElementCount())
            {
                result.Errors.Add("Invalid number of elements");
                return result;
            }

            if (!PrepareHeaders())
            {
                result.Errors.Add("Error during headers parsing");
                return result;
            }

            do
            {
                var client = new Client();
                ReadDocument(client);
                ReadNames(client);
                ReadCard(client,key);
                ReadOtherFields(client);
                result.Clients.Add(client);
            } while (HasNextElement());

            return result;
        }

        private void ReadDocument(Client client)
        {
            _xmlReader.ReadToDescendant("Document");
            client.Document = _xmlReader.ReadElementContentAsString().Trim();
        }
        
        private void ReadNames(Client client)
        {
            _xmlReader.ReadToNextSibling("Name");
            client.Name = _xmlReader.ReadElementContentAsString().Trim();
            _xmlReader.ReadToNextSibling("LastName");
            client.LastName = _xmlReader.ReadElementContentAsString().Trim();
        }
        
        private void ReadCard(Client client, string key)
        {
            _xmlReader.ReadToNextSibling("Card");
            string encryptedCardNumber = _xmlReader.ReadElementContentAsString().Trim();
            string decryptedCardNumber = EncryptionManager.Decrypt(encryptedCardNumber, key);
            client.Card = decryptedCardNumber;
        }

        private void ReadOtherFields(Client client)
        {
            _xmlReader.ReadToNextSibling("Rank");
            client.Rank = _xmlReader.ReadElementContentAsString().Trim();
            _xmlReader.ReadToNextSibling("Phone");
            client.Phone = _xmlReader.ReadElementContentAsString().Trim();
            _xmlReader.ReadToNextSibling("Poligone");
            client.Poligone = _xmlReader.ReadElementContentAsString().Trim();
        }

        private bool HasNextElement()
        {
            return _xmlReader.ReadToNextSibling("Client");
        }
    }
}
