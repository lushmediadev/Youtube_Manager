using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;

namespace YoutubeManager.UI.ViewModels
{
    public class AddChannelWindowViewModel : BaseViewModel
    {
        string _ChannelLinks
#if DEBUG
            = "https://www.youtube.com/@relaxyoursoul0110";
#else
            = string.Empty;
#endif
        public string ChannelLinks
        {
            get { return _ChannelLinks; }
            set { _ChannelLinks = value; NotifyPropertyChange(); }
        }
    }
}
