using DocoLibrary.LibraryDirectory;
using Nancy;
using System.Threading.Tasks;
using System.Web;

namespace DocoLibrary.LibraryStore
{
    public interface ILibraryStore
    {
        Task<LibraryItem> SaveAsync(string name, HttpFile file);
    }
}
