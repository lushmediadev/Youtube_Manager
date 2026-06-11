using System.ComponentModel;
using TqkLibrary.WpfUi;

namespace YoutubeManager.UI.ViewModels
{
    public class CustomColViewModel : BaseViewModel
    {
        public delegate void CustomColViewModelUpdateNameCallback();

        readonly int index;
        public CustomColViewModel(int index)
        {
            this.index = index;
        }
        internal event CustomColViewModelUpdateNameCallback NameChange;
        internal bool IsShow
        {
            get { return Singleton.Setting.Data.CustomColumns[index].IsShow; }
            set
            {
                Singleton.Setting.Data.CustomColumns[index].IsShow = value;
                if (Singleton.Setting.Data.CustomColumns[index].Size == 0) Singleton.Setting.Data.CustomColumns[index].Size = 100;
                Singleton.Setting.TriggerSave();
                NotifyPropertyChange("ColSize");
            }
        }

        public string Name
        {
            get { return Singleton.Setting.Data.CustomColumns[index].Name; }
            set { Singleton.Setting.Data.CustomColumns[index].Name = Check(value, $"CustomColumn {index}"); Singleton.Setting.TriggerSave(); NameChange?.Invoke(); NotifyPropertyChange(); }
        }

        bool _IsEditing = false;
        public bool IsEditing
        {
            get { return _IsEditing; }
            set { _IsEditing = value; NotifyPropertyChange(); }
        }

        public int ColSize
        {
            get
            {
                return IsShow ? Singleton.Setting.Data.CustomColumns[index].Size : 0;
            }
            set { Singleton.Setting.Data.CustomColumns[index].Size = value; Singleton.Setting.TriggerSave(); NotifyPropertyChange(); }
        }

        string Check(string input, string default_)
        {
            if (string.IsNullOrWhiteSpace(input)) return default_;
            else return input;
        }

        public ListSortDirection ListSortDirection { get; set; } = ListSortDirection.Descending;
    }
}
