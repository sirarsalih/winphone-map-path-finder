using Cirrious.CrossCore.Plugins;

namespace ASPC2014.Windows.Phone.App.Bootstrap
{
    public class MessengerPluginBootstrap
        : MvxPluginBootstrapAction<Cirrious.MvvmCross.Plugins.Messenger.PluginLoader>
    {
        public MessengerPluginBootstrap()
        {
            Cirrious.MvvmCross.Plugins.Messenger.PluginLoader.Instance.EnsureLoaded();
        }
    }
}