using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PortableClassLibrary.Messages;
using PortableClassLibrary.Models;
using PortableClassLibrary.Services.Contracts;
using PortableClassLibrary.ViewModels;


namespace PortableClassLibrary.Services
{
    public class AzureCloudService : HttpService, IAzureCloudService
    {
        private static FirstViewModel _firstViewModel;

        public IMvxMessenger MvxMessenger { get; private set; }

        public AzureCloudService()
        {
            MvxMessenger = Mvx.Resolve<IMvxMessenger>();
        }

        public FirstViewModel GetFirstViewModel()
        {
            return _firstViewModel ?? new FirstViewModel(new AzureCloudService());
        }

        public void BeginGetRequestForConverting(List<string> latLongList)
        {
            var request = CreateUrlAndHttpWebRequest("http://sirarscloudservice.cloudapp.net/Home/ConvertToArmaCoordinates?", latLongList);
            request.BeginGetResponse(ConversionCallBack, request);
        }

        public void BeginGetRequestForSolving(string coordinates, FirstViewModel firstViewModel)
        {
            _firstViewModel = firstViewModel;
            var coordinatesList = coordinates.Split(' ');
            var request = CreateUrlAndHttpWebRequest("http://sirarscloudservice.cloudapp.net/Home/Solve?", coordinatesList);
            request.BeginGetResponse(SolutionCallBack, request);
        }

        private void SolutionCallBack(IAsyncResult result)
        {
            var request = result.AsyncState as HttpWebRequest;
            if (request == null) return;
            try
            {
                var response = request.EndGetResponse(result);
                var streamResponse = response.GetResponseStream();
                var streamReader = new StreamReader(streamResponse);
                var myResponse = streamReader.ReadToEnd();
                var json = (JObject)JsonConvert.DeserializeObject(myResponse);
                var results = json["results"].ToString().Replace(",)", ")").Replace(" P", "\nP");
                _firstViewModel.Result = results;
            }
            catch {
                _firstViewModel.Result = "Error!";
            }
        }

        private void ConversionCallBack(IAsyncResult result)
        {
            var request = result.AsyncState as HttpWebRequest;
            if (request == null) return;
            try
            {
                var response = request.EndGetResponse(result);
                var streamResponse = response.GetResponseStream();
                var streamReader = new StreamReader(streamResponse);
                var myResponse = streamReader.ReadToEnd();
                var json = (JObject)JsonConvert.DeserializeObject(myResponse);
                var armaCoordinates = json["armaCoordinates"].ToObject<List<ArmaCoordinate>>();
                var armaCoordinateString = CreateString(armaCoordinates);
                _firstViewModel.ArmaCoordinates = armaCoordinateString;
            }
            catch
            {
                _firstViewModel.ArmaCoordinates = "Error!";
            }
        }

        private static string CreateString(IEnumerable<ArmaCoordinate> armaCoordinates)
        {
            var armaCoordinateString = "";
            var counter = 0;
            foreach (var armaCoordinate in armaCoordinates)
            {
                if (counter != 0)
                {
                    armaCoordinateString += "\n" + armaCoordinate.X + "," + armaCoordinate.Y;
                }
                else
                {
                    armaCoordinateString += armaCoordinate.X + "," + armaCoordinate.Y;
                }
                counter++;
            }
            return armaCoordinateString;
        }
    }
}