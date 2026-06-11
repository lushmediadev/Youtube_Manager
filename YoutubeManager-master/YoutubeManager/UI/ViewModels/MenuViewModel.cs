using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using TqkLibrary.WpfUi;
using YoutubeManager.Attributes;
using YoutubeManager.DataClass;
using YoutubeManager.Enums;

namespace YoutubeManager.UI.ViewModels
{
    public sealed class MenuViewModel : BaseViewModel
    {
        public MenuViewModel(MenuAction action)
        {
            this.Action = action;

            _ActionText = action.GetAttribute<NameAttribute>()?.Name ?? action.ToString();
            Img = action.GetAttribute<ResourceImageAttribute>()?.Bitmap?.ToImageSource();
        }

        readonly CustomColViewModel? customColViewModel;
        public MenuViewModel(MenuAction action, CustomColViewModel customColViewModel) : this(action)
        {
            this.customColViewModel = customColViewModel;
            customColViewModel.NameChange += CustomColViewModel_NameChange;
        }

        private void CustomColViewModel_NameChange()
        {
            NotifyPropertyChange(nameof(ActionText));
        }

        public MenuViewModel(MenuAction action, IEnumerable<MenuViewModel> childs) : this(action)
        {
            childs.ToList().ForEach(x => Childs.Add(x));
        }

        public bool IsCheckable { get; set; } = false;

        private bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; NotifyPropertyChange(); }
        }

        private bool _IsChecked = false;
        public bool IsChecked
        {
            get
            {
                if (customColViewModel == null) return _IsChecked;
                else return customColViewModel.IsShow;
            }
            set
            {
                if (customColViewModel == null) _IsChecked = value;
                else customColViewModel.IsShow = value;
                NotifyPropertyChange();
            }
        }

        public MenuAction Action { get; }


        string _ActionText;
        public string ActionText
        {
            get
            {
                if (customColViewModel == null) return _ActionText;
                else return customColViewModel.Name;
            }
        }
        public ImageSource? Img { get; }

        public ObservableCollection<MenuViewModel> Childs { get; } = new ObservableCollection<MenuViewModel>();
    }
}
