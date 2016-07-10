using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Linq;

namespace DocoLibrary.LibraryDirectory
{
    public class DynamoDirectory : ILibraryDirectory
    {
        private readonly IAmazonDynamoDB m_DynamoDb;
        private readonly string m_TableName;
        private readonly string m_IdKey;
        private readonly string m_TimestampKey;
        private readonly string m_NameKey;
        private readonly string m_UrlKey;

        public DynamoDirectory(
            IAmazonDynamoDB dynamoDb, 
            string tableName,
            string idKey,
            string timestampKey,
            string nameKey,
            string urlKey)
        {
            m_DynamoDb = dynamoDb;
            m_TableName = tableName;

            m_IdKey = idKey;
            m_TimestampKey = timestampKey;
            m_NameKey = nameKey;
            m_UrlKey = urlKey;
        }

        public IEnumerable<LibraryItem> Items
        {
            get
            {
                Dictionary<string, AttributeValue> lastKey = null;
                var results = new List<LibraryItem>();

                do
                {
                    var scanRequest = new ScanRequest(m_TableName)
                    {
                        ExclusiveStartKey = lastKey
                    };

                    var response = m_DynamoDb.Scan(scanRequest);
                    results.AddRange(response.Items.Select(item =>
                    {
                        return new LibraryItem
                        {
                            Id = item[m_IdKey].S,
                            Timestamp = long.Parse(item[m_TimestampKey].N),
                            Name = item[m_NameKey].S,
                            Url = item[m_UrlKey].S
                        };
                    }));

                    lastKey = response.LastEvaluatedKey;
                } while (lastKey != null && lastKey.Count != 0);

                return results;
            }
        }

        public IEnumerable<LibraryItem> Add(LibraryItem item)
        {
            var id = new AttributeValue(item.Id);
            var timestamp = new AttributeValue
            {
                N = item.Timestamp.ToString()
            };
            var name = new AttributeValue(item.Name);
            var url = new AttributeValue(item.Url);

            var attributes = new Dictionary<string, AttributeValue>
            {
                { m_IdKey, id },
                { m_TimestampKey, timestamp },
                { m_NameKey, name },
                { m_UrlKey, url }
            };

            var items = Items.ToList();

            var putRequest = new PutItemRequest(m_TableName, attributes);
            var response = m_DynamoDb.PutItem(putRequest);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                return null;

            return items.Concat(new[] { item });
        }
    }
}