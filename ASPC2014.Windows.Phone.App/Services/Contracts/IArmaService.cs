namespace ASPC2014.Windows.Phone.App.Services.Contracts
{
    public interface IArmaService
    {
        void StartTcpListenerAndSendCommandToArma(string command);
    }
}