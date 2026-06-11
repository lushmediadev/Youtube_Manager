using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;

namespace YoutubeManager.UI.ViewModels
{
    public class SettingWindowViewModel : BaseViewModel
    {
        public string ApiKeys
        {
            get { return Singleton.Setting.Data.ApiKeys; }
            set { Singleton.Setting.Data.ApiKeys = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }

        public int ThreadCount
        {
            get { return Singleton.Setting.Data.ThreadCount; }
            set { Singleton.Setting.Data.ThreadCount = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }


        string _ApiCheckMessage;
        public string ApiCheckMessage
        {
            get { return _ApiCheckMessage; }
            set { _ApiCheckMessage = value; NotifyPropertyChange(); }
        }
    }
}
