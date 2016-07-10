﻿using System;
using DocoLibrary.LibraryDirectory;
using System.IO;
using Nancy;
using System.Threading.Tasks;

namespace DocoLibrary.LibraryStore
{
    public class LocalStore : ILibraryStore
    {
        private readonly string m_Path;

        public LocalStore(string path)
        {
            m_Path = path;
        }

        public async Task<LibraryItem> SaveAsync(string name, HttpFile file)
        {
            var fExt = file.GetExtension();
            var id = Guid.NewGuid().ToString();
            var fName = id + fExt;
            var stream = file.Value;

            var originalDirectory = new DirectoryInfo(string.Format("{0}uploads", m_Path));

            string pathString = Path.Combine(originalDirectory.ToString(), "library");

            var fileName1 = Path.GetFileName(fName);

            bool isExists = Directory.Exists(pathString);

            if (!isExists)
                Directory.CreateDirectory(pathString);

            var path = string.Format("{0}\\{1}", pathString, fName);

            using (var fileStream = File.Create(path))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream);
            }

            var createTime = File.GetCreationTimeUtc(path);

            return new LibraryItem
            {
                Id = id,
                Timestamp = createTime.Ticks,
                Name = name,
                Url = string.Format("/uploads/library/{0}", fName)
            };
        }
    }
}