using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using PortableClassLibrary.Services.Contracts;
using PortableClassLibrary.ViewModels;

namespace PortableClassLibrary.Services
{
    public class RasPiService : HttpService, IRasPiService
    {
        private FirstViewModel _firstViewModel;

        public void BeginGetRequestForFlyingDrone(IEnumerable<string> coordinates, FirstViewModel firstViewModel)
        {
            _firstViewModel = firstViewModel;
            var request = CreateUrlAndHttpWebRequest("https://raspi.sirars.com/fly?", coordinates);
            request.BeginGetResponse(CallBack, request);
        }

        private void CallBack(IAsyncResult result)
        {
            var request = result.AsyncState as HttpWebRequest;
            if (request == null) return;
            try
            {
                var response = request.EndGetResponse(result);
                var streamResponse = response.GetResponseStream();
                var streamReader = new StreamReader(streamResponse);
                var myResponse = streamReader.ReadToEnd();
                var myResponseList = myResponse.Replace("<br>", "X\n");
                var myResponseListUpdate = myResponseList.Replace("<h1>", "");
                var myResponseListUpdateAgain = myResponseListUpdate.Replace("</h1>", "X\n");
                var myResList = myResponseListUpdateAgain.Split('X');
                _firstViewModel.RasPiResult = myResList[0];
                for (var i = 2; i < myResList.Length; i++)
                {
                    _firstViewModel.RasPiResult += myResList[i];
                }
            }
            catch
            {
                _firstViewModel.RasPiResult = "Error!";
            }
        }
    }
}
