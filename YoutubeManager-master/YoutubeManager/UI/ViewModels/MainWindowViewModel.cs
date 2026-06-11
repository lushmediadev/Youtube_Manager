using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TqkLibrary.WpfUi;
using YoutubeManager.DataClass;
using YoutubeManager.Enums;

namespace YoutubeManager.UI.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        readonly Dictionary<ChannelData, ChannelViewModel> dict_channelviewmodels = new Dictionary<ChannelData, ChannelViewModel>();

        public MainWindowViewModel()
        {
            Channels = new ChannelViewModelSaveObservableCollection(x =>
            {
                if (!dict_channelviewmodels.ContainsKey(x)) dict_channelviewmodels[x] = new ChannelViewModel(x);
                return dict_channelviewmodels[x];
            }, Singleton.ListChannelPath);
            //Channels.Load();
            for (int i = 0; i < Singleton.MaxCol; i++) CustomCols.Add(new CustomColViewModel(i));


            List<MenuViewModel> cols = new List<MenuViewModel>();
            for (int i = 0; i < Singleton.MaxCol; i++) cols.Add(new MenuViewModel(MenuAction.CustomColumn, CustomCols[i]) { IsCheckable = true });
            ChannelsMenu.Add(new MenuViewModel(MenuAction.Column, cols));
        }

        public GroupYoutubeViewModelSaveObservableCollection Groups { get; }
            = new GroupYoutubeViewModelSaveObservableCollection(Directory.GetCurrentDirectory() + "\\Datas\\Groups.json");

        public ObservableCollection<MenuViewModel> GroupsMenu { get; } = new ObservableCollection<MenuViewModel>()
        {
            new MenuViewModel(MenuAction.Add),
            new MenuViewModel(MenuAction.Edit),
            new MenuViewModel(MenuAction.Delete)
        };


        public ChannelViewModelSaveObservableCollection Channels { get; }

        public ObservableCollection<MenuViewModel?> ChannelsMenu { get; } = new ObservableCollection<MenuViewModel?>()
        {
            new MenuViewModel(MenuAction.Add),
            null,
            new MenuViewModel(MenuAction.CheckChannelSelected),
            new MenuViewModel(MenuAction.Open),
            new MenuViewModel(MenuAction.CopyAddress),
            new MenuViewModel(MenuAction.Delete),
            null,
            new MenuViewModel(MenuAction.MoveToTop),
            new MenuViewModel(MenuAction.MoveToBottom),
            null,
            new MenuViewModel(MenuAction.CheckChannelInGroup),
            new MenuViewModel(MenuAction.CheckAllChannel),
            null,
            new MenuViewModel(MenuAction.DeleteAllDeadInGroup),
            new MenuViewModel(MenuAction.DeleteAllDead),
            null,
            new MenuViewModel(MenuAction.ExportTxt),
            null,
        };

        public GridLength ColGridMain
        {
            get
            {
                return new GridLength(Singleton.Setting.Data.ColGridMain);
            }
            set
            {
                Singleton.Setting.Data.ColGridMain = value.Value;
                Singleton.Setting.TriggerSave();
                NotifyPropertyChange();
            }
        }

        public int GroupNameColLength
        {
            get { return Singleton.Setting.Data.GroupColName; }
            set { Singleton.Setting.Data.GroupColName = value; NotifyPropertyChange(); }
        }

        #region ChannelCol
        public int ChannelColIcon
        {
            get { return Singleton.Setting.Data.ChannelColIcon; }
            set { Singleton.Setting.Data.ChannelColIcon = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }
        public int ChannelColTitle
        {
            get { return Singleton.Setting.Data.ChannelColTitle; }
            set { Singleton.Setting.Data.ChannelColTitle = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }
        public int ChannelColLastTime
        {
            get { return Singleton.Setting.Data.ChannelColLastTime; }
            set { Singleton.Setting.Data.ChannelColLastTime = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }
        public int ChannelColVideoCount
        {
            get { return Singleton.Setting.Data.ChannelColVideoCount; }
            set { Singleton.Setting.Data.ChannelColVideoCount = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }
        public int ChannelColSubscriberCount
        {
            get { return Singleton.Setting.Data.ChannelColSubscriberCount; }
            set { Singleton.Setting.Data.ChannelColSubscriberCount = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }
        public int ChannelColViewCount
        {
            get { return Singleton.Setting.Data.ChannelColViewCount; }
            set { Singleton.Setting.Data.ChannelColViewCount = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }
        public int ChannelColViewChangedCount
        {
            get { return Singleton.Setting.Data.ChannelColViewChangedCount; }
            set { Singleton.Setting.Data.ChannelColViewChangedCount = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }
        #endregion


        public List<CustomColViewModel> CustomCols { get; } = new List<CustomColViewModel>();


        int _ProgressValue = 0;
        public int ProgressValue
        {
            get { return _ProgressValue; }
            set { _ProgressValue = value; NotifyPropertyChange(); }
        }

        int _ProgressMax = 0;
        public int ProgressMax
        {
            get { return _ProgressMax; }
            set
            {
                _ProgressMax = value;
                if (value == 0) ProgressIsVisibility = false;
                else ProgressIsVisibility = true;
                NotifyPropertyChange();
            }
        }

        string _ProgressText = "";
        public string ProgressText
        {
            get { return _ProgressText; }
            set { _ProgressText = value; NotifyPropertyChange(); }
        }

        bool _ProgressIsVisibility = false;
        public bool ProgressIsVisibility
        {
            get { return _ProgressIsVisibility; }
            set { _ProgressIsVisibility = value; NotifyPropertyChange(); }
        }

        public bool IsFindById
        {
            get { return Singleton.Setting.Data.IsFindById; }
            set { Singleton.Setting.Data.IsFindById = value; NotifyPropertyChange(); Singleton.Setting.TriggerSave(); }
        }

        public int ChannelColSTT
        {
            get { return Singleton.Setting.Data.ChannelColSTT; }
            set
            {
                Singleton.Setting.Data.ChannelColSTT = value;
                Singleton.Setting.TriggerSave();
                NotifyPropertyChange();
            }
        }
    }
}
