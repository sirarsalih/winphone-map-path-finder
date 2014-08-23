using Ninject;
using PortableClassLibrary.Services;
using PortableClassLibrary.Services.Contracts;

namespace ASPC2014.Windows.Phone.App.Bootstrap
{
    public static class IoCContainter
    {
        private static readonly IKernel Kernel = new StandardKernel();

        public static void Initialize()
        {
            Kernel.Bind<IAzureCloudService>().To<AzureCloudService>();
        }

        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
    }
}
