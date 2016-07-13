using DocoLibrary.FileUpload;
using Nancy;
using Nancy.ModelBinding;
using DocoLibrary.LibraryStore;
using DocoLibrary.LibraryDirectory;
using System.Linq;

namespace DocoLibrary
{
    public class FileUploadResponse
    {
        public string Identifier { get; set; }
    }

    public class FileUploadModule : NancyModule
    {
        private readonly ILibraryStore m_LibraryStore;
        private readonly ILibraryDirectory m_LibraryDirectory;

        public FileUploadModule(ILibraryStore store, ILibraryDirectory directory)
            : base("/file")
        {
            m_LibraryStore = store;
            m_LibraryDirectory = directory;

            Get["/list"] = _ =>
            {
                // Get a listing of all the items in the library directory.
                var result = m_LibraryDirectory.Items.OrderBy(i => i.Timestamp);

                return Negotiate
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithModel(result);
            };

            Post["/upload", true] = async (x, ct) =>
            {
                // Get the content of the upload.
                var request = this.Bind<FileUploadRequest>();

                // Save the uploaded item to the library store.
                var libraryItem = await m_LibraryStore.SaveAsync(request.Title, request.File);

                // Update our directory with information about the item (url & name)
                var response = m_LibraryDirectory.Add(libraryItem).OrderBy(i => i.Timestamp);
                
                return Negotiate
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithModel(response);
            };
        }
    }
}