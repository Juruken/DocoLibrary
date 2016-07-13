using System;
using DocoLibrary.LibraryDirectory;
using System.IO;
using Nancy;
using System.Threading.Tasks;

namespace DocoLibrary.LibraryStore
{
    /// <summary>
    /// Stores files on the local file system.
    /// </summary>
    public class LocalStore : ILibraryStore
    {
        private readonly string m_Path;

        public LocalStore(string path)
        {
            m_Path = path;
        }

        public async Task<LibraryItem> SaveAsync(string name, HttpFile file)
        {
            // Create the file name.
            var fExt = file.GetExtension();
            var id = Guid.NewGuid().ToString();
            var fName = id + fExt;

            // Make sure the folder for the library exists.
            var originalDirectory = new DirectoryInfo(string.Format("{0}uploads", m_Path));
            var pathString = Path.Combine(originalDirectory.ToString(), "library");

            if (!Directory.Exists(pathString))
                Directory.CreateDirectory(pathString);

            var path = string.Format("{0}\\{1}", pathString, fName);

            // Write the uploaded file to disk.
            using (var fileStream = File.Create(path))
            {
                var stream = file.Value;
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream);
            }

            // Return info about the item (URL & Name)
            return new LibraryItem
            {
                Id = id,
                Timestamp = File.GetCreationTimeUtc(path).Ticks,
                Name = name,
                Url = string.Format("/uploads/library/{0}", fName)
            };
        }
    }
}