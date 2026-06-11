using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;
using TqkLibrary.WpfUi.Interfaces;
using TqkLibrary.WpfUi.ObservableCollections;
using YoutubeManager.DataClass;

namespace YoutubeManager.UI.ViewModels
{
    public class GroupYoutubeViewModel : BaseViewModel, IViewModel<GroupYoutubeData>
    {
        public GroupYoutubeViewModel(GroupYoutubeData Data)
        {
            this.Data = Data;
        }
        public GroupYoutubeData Data { get; }

        public event ChangeCallBack<GroupYoutubeData> Change;
        public void ChannelChangeCall()
        {
            Change?.Invoke(this, Data);
        }


        public string Name
        {
            get { return Data.Name; }
            set { Data.Name = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }

        bool _IsEditing = false;
        public bool IsEditing
        {
            get { return _IsEditing; }
            set { _IsEditing = value; NotifyPropertyChange(); }
        }

        bool _IsVisible = true;
        public bool IsVisible
        {
            get { return _IsVisible; }
            set { _IsVisible = value; NotifyPropertyChange(); }
        }
    }
}
