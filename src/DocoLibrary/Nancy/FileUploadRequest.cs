using Nancy;
using System.Collections.Generic;

namespace DocoLibrary.FileUpload
{
    public class FileUploadRequest
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public IList<string> Tags { get; set; }

        public HttpFile File { get; set; }
    }
}