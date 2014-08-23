using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Windows.Devices.Geolocation;
using ASPC2014.Windows.Phone.App.Helpers;
using ASPC2014.Windows.Phone.App.Services;
using ASPC2014.Windows.Phone.App.Services.Contracts;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.WindowsPhone.Views;
using Microsoft.Phone.Maps.Controls;
using PortableClassLibrary.Messages;
using PortableClassLibrary.Services;
using PortableClassLibrary.Services.Contracts;

namespace ASPC2014.Windows.Phone.App.Views
{
    public partial class FirstView : MvxPhonePage
    {
        public static string Coordinates { get; private set; }
        private static Dictionary<int, Point> CoordinatesDictionary { get; set; }
        public GeoCoordinateCollection GeoCoordinateCollection { get; set; }
        private static int Counter { get; set; }
        private string Results { get; set; }
        public IMvxMessenger MvxMessenger { get; private set; }
        public IArmaService ArmaService { get; set; }
        public IAzureCloudService AzureCloudService { get; set; }
        public IRasPiService RasPiService { get; set; }

        public FirstView()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            InitializeComponent();
            GetCurrentLocationAndDrawPointAndAddToMap();
            //SetLemnosLocationOnMap(); //TODO: Used for Arma 3
            Map.Tap += MapTap;
            ResultTextBox.IsEnabled = false;
            ArmaCoordinatesTextBox.IsEnabled = false;
            RasPiResultTextBox.IsEnabled = false;
            SolveButton.Foreground = new SolidColorBrush(Colors.Gray);
            CoordinatesDictionary = new Dictionary<int, Point>();
            MvxMessenger = Mvx.Resolve<IMvxMessenger>();
            GeoCoordinateCollection = new GeoCoordinateCollection();
            ArmaService = new ArmaService();
            AzureCloudService = new AzureCloudService();
            RasPiService = new RasPiService();
        }

        private void MapTap(object sender, GestureEventArgs gestureEventArgs)
        {
            ResetTheMapIfGotResults();
            var coordinate = GetPointFrom(gestureEventArgs);
            DrawAndAddToMap(coordinate);
            if (CoordinatesDictionary.Count >= 2)
            {
                SolveButton.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private async void GetCurrentLocationAndDrawPointAndAddToMap()
        {
            var geolocator = new Geolocator();
            try
            {
                var geoCoordinate = await SetCurrentLocationOnMap(geolocator);
                //MarkOnMap(geoCoordinate, Colors.Blue);
            }
            catch (UnauthorizedAccessException e)
            {
            }
        }

        private void MarkOnMap(GeoCoordinate geoCoordinate, Color color)
        {
            var ellipse = new Ellipse
            {
                Fill = new SolidColorBrush(color),
                Height = 10,
                Width = 10,
                Opacity = 50
            };

            var mapOverlay = new MapOverlay
            {
                Content = ellipse,
                PositionOrigin = new Point(0.5, 0.5),
                GeoCoordinate = geoCoordinate
            };
            var mapLayer = new MapLayer { mapOverlay };
            Map.Layers.Add(mapLayer);
        }

        private async Task<GeoCoordinate> SetCurrentLocationOnMap(Geolocator myGeolocator)
        {
            var myGeoposition = await myGeolocator.GetGeopositionAsync();
            var myGeopositionCoordinate = myGeoposition.Coordinate;
            var myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeopositionCoordinate);
            Map.Center = myGeoCoordinate;
            Map.ZoomLevel = 15;
            return myGeoCoordinate;
        }

        private GeoCoordinate SetLemnosLocationOnMap()
        {
            var lemnosGeoCoordinate = new GeoCoordinate(39.878880, 25.231705);
            Map.Center = lemnosGeoCoordinate;
            Map.ZoomLevel = 13;
            return lemnosGeoCoordinate;
        }

        private void SolveButtonClick(object sender, RoutedEventArgs e)
        {
            if (CoordinatesDictionary.Count < 2) return;
            var coordinatesMessage = new CoordinatesMessage(this) { Coordinates = Coordinates };
            MvxMessenger.Publish(coordinatesMessage);
        }

        private void TextBoxResultsOnTextChanged(object sender, TextChangedEventArgs e)
        {
            Results = ((TextBox)sender).Text;
            var pathList = GetPath(Results);
            DrawPathAndAddToGeoCoordinateCollectionAndAddToMap(pathList);
            BeginGetRequestForConvertingLatLongToArmaCoordinates();
            BeginGetRequestForFlyingDrone(pathList);
        }

        private void BeginGetRequestForConvertingLatLongToArmaCoordinates()
        {
            var latLongList = GeoCoordinateCollection.Select(geoCoordinate => geoCoordinate.Latitude + "," + geoCoordinate.Longitude).ToList();
            AzureCloudService.BeginGetRequestForConverting(latLongList);
        }

        private void BeginGetRequestForFlyingDrone(IEnumerable<string> pathList)
        {
            var xyPathList = pathList.Select(point => CoordinatesDictionary[Convert.ToInt32(point, CultureInfo.InvariantCulture)]).Select(point => String.Join(",", point.X, point.Y)).ToList();
            RasPiService.BeginGetRequestForFlyingDrone(xyPathList, AzureCloudService.GetFirstViewModel());
        }

        private void TextBoxArmaCoordinatesOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var armaCoordinates = ((TextBox)sender).Text.Split('\n');
            var armaCommand = BuildArmaCommand(armaCoordinates);
            ArmaService.StartTcpListenerAndSendCommandToArma(armaCommand);
        }

        private void DrawPathAndAddToGeoCoordinateCollectionAndAddToMap(IEnumerable<string> pathList)
        {
            var mapPolyline = new MapPolyline();
            GeoCoordinateCollection = new GeoCoordinateCollection();
            foreach (var point in pathList.Select(point => CoordinatesDictionary[Convert.ToInt32(point, CultureInfo.InvariantCulture)]))
            {
                GeoCoordinateCollection.Add(Map.ConvertViewportPointToGeoCoordinate(point));
            }
            mapPolyline.Path = GeoCoordinateCollection;
            mapPolyline.StrokeColor = Colors.Red;
            mapPolyline.StrokeThickness = 2;
            Map.MapElements.Add(mapPolyline);
        }

        private Point GetPointFrom(GestureEventArgs e)
        {
            var coordinate = e.GetPosition(Map);
            CoordinatesDictionary.Add(Counter, coordinate);
            Coordinates += coordinate + " ";
            Counter++;
            return coordinate;
        }

        private static string BuildArmaCommand(string[] armaCoordinates)
        {
            var armaCommand = "";
            for (var i = 0; i < armaCoordinates.Length; i++)
            {
                armaCommand += "group testuav addWaypoint " + "[[" + armaCoordinates[i] + ",50]," + i +
                               "] setWayPointType \"MOVE\" setWaypointSpeed \"FULL\";";
            }
            return armaCommand;
        }

        private void DrawAndAddToMap(Point coordinate)
        {
            var geoCoordinate = Map.ConvertViewportPointToGeoCoordinate(coordinate);
            MarkOnMap(geoCoordinate, Colors.Red);
        }

        private IEnumerable<string> GetPath(string results)
        {
            var path = results.Split('\n').Last().Replace("(", "").Replace(")", "").Replace("Path: ", "");
            var pathList = path.Split(',');
            return pathList;
        }

        private void ResetMap()
        {
            RemoveCoordinatesFromMapAndResetStaticFields();
            GetCurrentLocationAndDrawPointAndAddToMap();
            //SetLemnosLocationOnMap(); //TODO: Used for Arma 3
        }

        private void ResetTheMapIfGotResults()
        {
            if (!string.IsNullOrEmpty(Results))
            {
                ResetMap();
            }
        }

        private void RemoveCoordinatesFromMapAndResetStaticFields()
        {
            Results = string.Empty;
            Coordinates = string.Empty;
            Counter = 0;
            SolveButton.Foreground = new SolidColorBrush(Colors.Gray);
            CoordinatesDictionary.Clear();
            Map.Layers.Clear();
            Map.MapElements.Clear();
        }
    }
}