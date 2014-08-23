using Cirrious.MvvmCross.Plugins.Messenger;

namespace PortableClassLibrary.Messages
{
    public class CoordinatesMessage : MvxMessage
    {
        public CoordinatesMessage(object sender) : base(sender) { }

        public string Coordinates { get; set; }
    }
}
