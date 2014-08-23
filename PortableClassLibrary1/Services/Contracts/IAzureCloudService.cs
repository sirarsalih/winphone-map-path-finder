using System.Collections.Generic;
using PortableClassLibrary.ViewModels;

namespace PortableClassLibrary.Services.Contracts
{
    public interface IAzureCloudService
    {
        void BeginGetRequestForSolving(string coordinates, FirstViewModel firstViewModel);
        void BeginGetRequestForConverting(List<string> latLongList);
        FirstViewModel GetFirstViewModel();
    }
}
