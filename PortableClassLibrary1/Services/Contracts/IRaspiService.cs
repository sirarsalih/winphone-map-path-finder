using System.Collections.Generic;
using PortableClassLibrary.ViewModels;

namespace PortableClassLibrary.Services.Contracts
{
    public interface IRasPiService
    {
        void BeginGetRequestForFlyingDrone(IEnumerable<string> coordinates, FirstViewModel firstViewModel);
    }
}
