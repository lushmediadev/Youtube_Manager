using TqkLibrary.WpfUi;
using YoutubeManager.DataClass;

namespace YoutubeManager.UI.ViewModels
{
    public class ChannelCustomColumn : BaseViewModel
    {
        public delegate void ChannelCustomColumnSaveCall();

        public event ChannelCustomColumnSaveCall SaveCall;
        readonly int index;
        readonly ChannelData Data;
        public ChannelCustomColumn(int index, ChannelData Data)
        {
            this.index = index;
            this.Data = Data;
        }

        public string Text
        {
            get { return Data.CustomColDatas[index]; }
            set { Data.CustomColDatas[index] = value; SaveCall?.Invoke(); NotifyPropertyChange(); }
        }

        bool _IsEditing = false;
        public bool IsEditing
        {
            get { return _IsEditing; }
            set { _IsEditing = value; NotifyPropertyChange(); }
        }
    }
}
