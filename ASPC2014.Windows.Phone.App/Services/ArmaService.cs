using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using ASPC2014.Windows.Phone.App.Services.Contracts;


namespace ASPC2014.Windows.Phone.App.Services
{
    public class ArmaService : IArmaService
    {
        private string _command;
        private StreamSocketListener _streamSocketListener;
        private string _port;

        public async void StartTcpListenerAndSendCommandToArma(string command)
        {
            _command = command;
            _port = "7845";
            _streamSocketListener = new StreamSocketListener();
            _streamSocketListener.ConnectionReceived += OnConnectionReceived;
            await TryBindServiceNameAsync(_port);
        }

        private async Task TryBindServiceNameAsync(string port)
        {
            try
            {
                await _streamSocketListener.BindServiceNameAsync(port);
            }
            catch (Exception)
            {
                Debug.WriteLine("Port " + port + "is busy. Please restart ARMA.");
            }
        }

        private async void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.WriteLine("Connected to ARMA.");
            await Task.Run(() => HandleRequest(args.Socket));
        }

        private async void HandleRequest(StreamSocket socket)
        {
            await ReadRequestFromArma(socket.InputStream);
            await TryWriteResponseToArma(socket.OutputStream);
            socket.Dispose();
            _streamSocketListener.Dispose();
        }

        private async Task TryWriteResponseToArma(IOutputStream outputStream)
        {
            var dataWriter = new DataWriter(outputStream);
            try
            {
                dataWriter.WriteString(_command);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to send command to ARMA." + ex);
            }
            await dataWriter.StoreAsync();
        }

        private static async Task ReadRequestFromArma(IInputStream inputStream)
        {
            var dataReader = new DataReader(inputStream);
            await StreamReadLine(dataReader);
        }

        private static async Task<string> StreamReadLine(IDataReader reader)
        {
            var data = "";
            while (true)
            {
                await reader.LoadAsync(1);
                var nextChar = reader.ReadByte();
                if (nextChar == '\n') { break; }
                if (nextChar == '\r') { continue; }
                data += Convert.ToChar(nextChar);
            }
            return data;
        }
    }
}
