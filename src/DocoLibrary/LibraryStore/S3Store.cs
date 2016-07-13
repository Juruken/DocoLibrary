using System;
using System.Threading.Tasks;
using DocoLibrary.LibraryDirectory;
using Nancy;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace DocoLibrary.LibraryStore
{
    /// <summary>
    /// Stores files in the cloud.
    /// </summary>
    public class S3Store : ILibraryStore
    {
        private readonly IAmazonS3 m_AmazonS3;
        private readonly string m_BucketName;
        private const string c_UrlFormat = "https://s3-ap-southeast-2.amazonaws.com/{0}/{1}";

        public S3Store(IAmazonS3 amazonS3, string bucketName)
        {
            m_AmazonS3 = amazonS3;
            m_BucketName = bucketName;
        }

        public async Task<LibraryItem> SaveAsync(string name, HttpFile file)
        {
            // Create the file name
            var fExt = file.GetExtension();
            var id = Guid.NewGuid().ToString();
            var fName = id + fExt;

            // Create an S3 upload request.
            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = m_BucketName,
                InputStream = file.Value,
                Key = fName,
                CannedACL = S3CannedACL.PublicRead
            };

            // Upload to S3.
            var transferUtility = new TransferUtility(m_AmazonS3);
            await transferUtility.UploadAsync(uploadRequest);

            // Return info about the item (URL & Name)
            return new LibraryItem
            {
                Id = id,
                Timestamp = DateTime.UtcNow.Ticks,
                Name = name,
                Url = string.Format(c_UrlFormat, m_BucketName, fName)
            };
        }
    }
}