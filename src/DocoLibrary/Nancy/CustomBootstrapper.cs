using System;
using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;
using DocoLibrary.LibraryStore;
using DocoLibrary.LibraryDirectory;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Threading;
using Nancy.Bootstrapper;

namespace DocoLibrary.Nancy
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        private const string c_TableName = "docoLibraryDemo";
        private const string c_IdAttribute = "id";
        private const string c_TimestampAttribute = "timestamp";
        private const string c_NameAttribute = "name";
        private const string c_UrlAttribute = "url";
        private const long c_ReadCapacity = 5;
        private const long c_WriteCapacity = 1;
        private readonly IAmazonDynamoDB m_DynamoDb = new AmazonDynamoDBClient();

        private const string c_BucketName = "schoeller-docolibrarydemo";

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            EnsureTableExists();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            ////RegisterLocal(container);

            RegisterAws(container);
        }

        private void RegisterAws(TinyIoCContainer container)
        {
            var dynamoDirectory = new DynamoDirectory(
                m_DynamoDb, c_TableName,
                c_IdAttribute, c_TimestampAttribute, c_NameAttribute, c_UrlAttribute);
            container.Register<ILibraryDirectory>(dynamoDirectory);
            container.Register<ILibraryStore>(new S3Store(new Amazon.S3.AmazonS3Client(), c_BucketName));
        }

        private static void RegisterLocal(TinyIoCContainer container)
        {
            // Create a local store to use for the library store.
            container.Register<ILibraryStore>(new LocalStore(AppDomain.CurrentDomain.BaseDirectory));

            // Create an in memory directory for the library directory.
            container.Register<ILibraryDirectory>(new InMemoryDirectory());
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);

            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/uploads"));
        }

        private void EnsureTableExists()
        {
            var keySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement(c_IdAttribute, KeyType.HASH),
                new KeySchemaElement(c_TimestampAttribute, KeyType.RANGE)
            };

            var attributeDefinition = new List<AttributeDefinition>
            {
                new AttributeDefinition(c_IdAttribute, ScalarAttributeType.S),
                new AttributeDefinition(c_TimestampAttribute, ScalarAttributeType.N)
            };

            var createTableRequest = new CreateTableRequest(
                c_TableName, 
                keySchema,
                attributeDefinition,
                new ProvisionedThroughput(c_ReadCapacity, c_WriteCapacity));

            try
            {
                m_DynamoDb.CreateTable(createTableRequest);
            }
            catch(ResourceInUseException ex)
            {
                if (ex.ErrorCode != "ResourceInUseException")
                    throw;
            }

            DescribeTableResponse response;
            do
            {
                Thread.Sleep(300);
                response = m_DynamoDb.DescribeTable(c_TableName);
            } while (response.Table.TableStatus == "CREATING");
        }
    }
}