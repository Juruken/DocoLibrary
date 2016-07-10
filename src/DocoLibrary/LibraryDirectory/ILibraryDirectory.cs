using System.Collections.Generic;

namespace DocoLibrary.LibraryDirectory
{
    public class LibraryItem
    {
        public string Id { get; set; }
        public long Timestamp { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public interface ILibraryDirectory
    {
        IEnumerable<LibraryItem> Items { get; }
        IEnumerable<LibraryItem> Add(LibraryItem item);
    }
}
