using System.Collections.Generic;
using System.Linq;
using System.Net;
using PortableClassLibrary.Services.Contracts;

namespace PortableClassLibrary.Services
{
    public class HttpService : IHttpService
    {
        public HttpWebRequest CreateUrlAndHttpWebRequest(string url, IEnumerable<string> parameters)
        {
            var finalUrl = parameters.Where(coordinate => !string.IsNullOrEmpty(coordinate)).Aggregate(url, (current, coordinate) => current + ("&c=" + coordinate));
            var request = (HttpWebRequest)WebRequest.Create(finalUrl);
            return request;
        }
    }
}
