using System;
using System.IO;
using System.Linq.Expressions;
using Nancy;
using Nancy.Conventions;
using Nancy.Responses;
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

            RegisterLocal(container);

            ////RegisterAws(container);
        }

        private static void RegisterLocal(TinyIoCContainer container)
        {
            // Create a local store to use for the library store.
            container.Register<ILibraryStore>(new LocalStore("C:\\"));

            // Create an in memory directory for the library directory.
            container.Register<ILibraryDirectory>(new InMemoryDirectory());
        }

        private void RegisterAws(TinyIoCContainer container)
        {
            var dynamoDirectory = new DynamoDirectory(
                m_DynamoDb, c_TableName,
                c_IdAttribute, c_TimestampAttribute, c_NameAttribute, c_UrlAttribute);
            container.Register<ILibraryDirectory>(dynamoDirectory);

            container.Register<ILibraryStore>(new S3Store(new Amazon.S3.AmazonS3Client(), c_BucketName));
        }

        private void EnsureTableExists()
        {
            // Define table schema. This includes keys and attributes.
            // Not all attributes need to be defined, but all keys do.
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

            // Create table, including Name, Schema, and initial capacity.
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

            // Wait until the table creationi has finished.
            DescribeTableResponse response;
            do
            {
                Thread.Sleep(300);
                response = m_DynamoDb.DescribeTable(c_TableName);
            } while (response.Table.TableStatus == "CREATING");
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);

            conventions.StaticContentsConventions.Add((context, s) =>
            {
                var requestPath = context.Request.Path;
                if (!requestPath.StartsWith("/uploads", StringComparison.InvariantCultureIgnoreCase))
                    return null;

                var filePath = string.Format("c:{0}", requestPath.Replace('/', '\\'));
                var mimeType = MimeTypes.GetMimeType(filePath);

                try
                {
                    var fileStream = new FileStream(filePath, FileMode.Open);
                    {
                        return new Response
                        {
                            ContentType = mimeType,
                            StatusCode = HttpStatusCode.OK,
                            Contents = stream =>
                            {
                                fileStream.CopyTo(stream);
                                fileStream.Close();
                            }
                        };
                    }
                }
                catch
                {
                    return new Response {StatusCode = HttpStatusCode.NotFound};
                }
            });

            //conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/uploads", "c:\\uploads"));
        }
    }
}