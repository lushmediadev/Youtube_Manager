using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TqkLibrary.WpfUi;
using TqkLibrary.WpfUi.Interfaces;
using TqkLibrary.WpfUi.ObservableCollections;
using YoutubeManager.DataClass;

namespace YoutubeManager.UI.ViewModels
{
    public class ChannelViewModel : BaseViewModel, IViewModel<ChannelData>
    {
        public ChannelViewModel(ChannelData Data)
        {
            if (Data.CustomColDatas == null) Data.CustomColDatas = new List<string>();
            for (int i = Data.CustomColDatas.Count; i < Singleton.MaxCol; i++) Data.CustomColDatas.Add(string.Empty);
            for (int i = 0; i < Singleton.MaxCol; i++)
            {
                ChannelCustomColumn channelCustomColumn = new ChannelCustomColumn(i, Data);
                channelCustomColumn.SaveCall += ChannelCustomColumn_SaveCall;
                CustomColumnDatas.Add(channelCustomColumn);
            }
            this.Data = Data;
            ChangeBackground(Data.IsLive);
        }

        private void ChannelCustomColumn_SaveCall()
        {
            Change?.Invoke(this, Data);
        }

        public ChannelData Data { get; }

        public event ChangeCallBack<ChannelData> Change;

        int _STT = 0;
        public int STT
        {
            get { return _STT; }
            set { _STT = value; NotifyPropertyChange(); }
        }


        ImageSource _Icon;
        public ImageSource Icon
        {
            get { return _Icon; }
            set { _Icon = value; NotifyPropertyChange(); }
        }

        Brush _BackGround = Brushes.MediumPurple;
        public Brush BackGround
        {
            get { return _BackGround; }
            private set { _BackGround = value; NotifyPropertyChange(); }
        }

        public void ChangeBackground(bool IsLive)
        {
            if (IsRequestLimitExceeded)
            {
                BackGround = new SolidColorBrush(Color.FromArgb(255, 223, 138, 227));
            }
            else
            {
                if (IsLive) BackGround = new SolidColorBrush(Colors.LightGreen);
                else BackGround = new SolidColorBrush(Colors.LightPink);
                Data.IsLive = IsLive;
                Change?.Invoke(this, Data);
            }
        }

        public string Title
        {
            get { return Data.Title; }
            set { Data.Title = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }

        public DateTime? LastTime
        {
            get { return Data.LastTime; }
            set { Data.LastTime = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }

        public ulong? VideoCount
        {
            get { return Data.VideoCount; }
            set { Data.VideoCount = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }

        public ulong? SubscriberCount
        {
            get { return Data.SubscriberCount; }
            set { Data.SubscriberCount = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }
        public ulong? ViewCount
        {
            get { return Data.ViewCount; }
            set { Data.ViewCount = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }
        public ulong? ViewCountChanged
        {
            get { return Data.ViewCountChanged; }
            set { Data.ViewCountChanged = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }
        public TimeSpan? DayDiffUpdate
        {
            get { return Data.DayDiffUpdate; }
            set { Data.DayDiffUpdate = value; NotifyPropertyChange(); Change?.Invoke(this, Data); }
        }
        public bool IsRequestLimitExceeded
        {
            get { return Data.IsRequestLimitExceeded; }
            set
            {
                Data.IsRequestLimitExceeded = value;
                NotifyPropertyChange();
                Change?.Invoke(this, Data);
                ChangeBackground(Data.IsLive);
            }
        }

        public List<ChannelCustomColumn> CustomColumnDatas { get; } = new List<ChannelCustomColumn>();

    }
}
