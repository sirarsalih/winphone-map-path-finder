using System;
using System.Linq;
using System.Xml.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using PortableClassLibrary.Messages;
using PortableClassLibrary.Services.Contracts;
using System.Diagnostics;

namespace PortableClassLibrary.ViewModels
{
    public class FirstViewModel
        : MvxViewModel
    {
        private readonly IAzureCloudService _azureCloudService;
        private IDisposable _token;
        public IMvxMessenger MvxMessenger { get; private set; }

        public FirstViewModel(IAzureCloudService azureCloudService)
        {
            _azureCloudService = azureCloudService;
            MvxMessenger = Mvx.Resolve<IMvxMessenger>();
            _token = MvxMessenger.SubscribeOnMainThread<CoordinatesMessage>(OnCoordinatesMessageReceived);
        }

        private void OnCoordinatesMessageReceived(CoordinatesMessage coordinatesMessage)
        {
            _azureCloudService.BeginGetRequestForSolving(coordinatesMessage.Coordinates, this);
        }

        private string _result;

        public string Result
        {
            get { return _result; }
            set
            {
                _result = value; 
                RaisePropertyChanged(() => Result);
            }
        }

        private string _armaCoordinates;

        public string ArmaCoordinates
        {
            get { return _armaCoordinates; }
            set
            {
                _armaCoordinates = value;
                RaisePropertyChanged(() => ArmaCoordinates);
            }
        }

        private string _rasPiResult;

        public string RasPiResult
        {
            get { return _rasPiResult; }
            set
            {
                _rasPiResult = value;
                RaisePropertyChanged(() => RasPiResult);
            }
        }
    }
}
