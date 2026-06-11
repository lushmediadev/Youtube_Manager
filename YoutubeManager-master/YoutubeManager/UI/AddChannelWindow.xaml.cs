using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TqkLibrary.Queues.TaskQueues;
using TqkLibrary.WpfUi;
using YoutubeManager.Works;
using YoutubeManager.UI.ViewModels;

namespace YoutubeManager.UI
{
    /// <summary>
    /// Interaction logic for AddChannelWindow.xaml
    /// </summary>
    public partial class AddChannelWindow : Window
    {
        public string Urls { get { return addChannelWindowViewModel.ChannelLinks; } }
        public bool IsAdd { get; private set; } = false;

        readonly AddChannelWindowViewModel addChannelWindowViewModel;
        public AddChannelWindow()
        {
            this.addChannelWindowViewModel = new AddChannelWindowViewModel();
            this.DataContext = this.addChannelWindowViewModel;
            InitializeComponent();
        }


        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            IsAdd = true;
            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            IsAdd = false;
            this.Close();
        }
    }
}
