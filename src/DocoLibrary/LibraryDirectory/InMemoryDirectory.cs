using System.Collections.Generic;

namespace DocoLibrary.LibraryDirectory
{
    /// <summary>
    /// Stores directory info in local memory.
    /// </summary>
    public class InMemoryDirectory : ILibraryDirectory
    {
        private readonly IList<LibraryItem> m_Items = new List<LibraryItem>();

        public IEnumerable<LibraryItem> Items
        {
            get { return m_Items; }
        }

        public IEnumerable<LibraryItem> Add(LibraryItem item)
        {
            m_Items.Add(item);
            return m_Items;
        }
    }
}