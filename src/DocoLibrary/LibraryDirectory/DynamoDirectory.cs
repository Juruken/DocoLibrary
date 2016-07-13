using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Linq;

namespace DocoLibrary.LibraryDirectory
{
    /// <summary>
    /// Stores directory information in the cloud.
    /// </summary>
    public class DynamoDirectory : ILibraryDirectory
    {
        // Client to connect with DynamoDB
        private readonly IAmazonDynamoDB m_DynamoDb;

        // Name of the DynamoDB table to use.
        private readonly string m_TableName;

        // DynamoDB attribute names.
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
            // Scans for all items in the table.
            get
            {
                // The furthest key we've scanned from the table.
                Dictionary<string, AttributeValue> lastKey = null;
                var results = new List<LibraryItem>();

                // Responses are paged. Loop until all pages are returned.
                do
                {
                    // Create a new scan request, starting from the last key read.
                    var scanRequest = new ScanRequest(m_TableName)
                    {
                        ExclusiveStartKey = lastKey
                    };

                    // Scan the table.
                    var response = m_DynamoDb.Scan(scanRequest);

                    // Concatenate paged results.
                    results.AddRange(response.Items.Select(item =>
                    {
                        // Read & Parse DynamoDB attribute properties.
                        return new LibraryItem
                        {
                            Id = item[m_IdKey].S,
                            Timestamp = long.Parse(item[m_TimestampKey].N),
                            Name = item[m_NameKey].S,
                            Url = item[m_UrlKey].S
                        };
                    }));

                    // Update the furthest key scanned.
                    lastKey = response.LastEvaluatedKey;
                } while (lastKey != null && lastKey.Count != 0);

                return results;
            }
        }

        public IEnumerable<LibraryItem> Add(LibraryItem item)
        {
            // Set all of the attribute values.
            var id = new AttributeValue(item.Id);
            var timestamp = new AttributeValue
            {
                N = item.Timestamp.ToString()
            };
            var name = new AttributeValue(item.Name);
            var url = new AttributeValue(item.Url);

            // Assign each attribute value to its corresponding key.
            var attributes = new Dictionary<string, AttributeValue>
            {
                { m_IdKey, id },
                { m_TimestampKey, timestamp },
                { m_NameKey, name },
                { m_UrlKey, url }
            };

            // Get the items from before the add, and concatenate the one we're adding.
            // Due to eventual consistancy, if we just get the items after we add ours,
            // there's no guarantee that our newly added one will be returned yet.
            var items = Items.ToList();

            // Execute the request to put the new item into the table.
            var putRequest = new PutItemRequest(m_TableName, attributes);
            var response = m_DynamoDb.PutItem(putRequest);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                return null;

            return items.Concat(new[] { item });
        }
    }
}