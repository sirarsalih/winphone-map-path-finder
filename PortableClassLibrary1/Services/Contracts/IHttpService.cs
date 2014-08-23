using System.Collections.Generic;
using System.Net;

namespace PortableClassLibrary.Services.Contracts
{
    interface IHttpService
    {
        HttpWebRequest CreateUrlAndHttpWebRequest(string url, IEnumerable<string> parameters);
    }
}
