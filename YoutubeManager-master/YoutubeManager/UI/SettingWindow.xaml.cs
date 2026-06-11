using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using YoutubeManager.DataClass;
using YoutubeManager.Works;
using YoutubeManager.UI.ViewModels;
namespace YoutubeManager.UI
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        readonly List<string> KeysFailed = new List<string>();
        readonly SettingWindowViewModel settingWindowViewModel;
        readonly WorkQueue<CheckWork> checkWorks = new WorkQueue<CheckWork>();
        public SettingWindow()
        {
            InitializeComponent();
            this.settingWindowViewModel = this.DataContext as SettingWindowViewModel;
            checkWorks.OnWorkComplete += CheckWorks_OnWorkComplete;
            checkWorks.OnRunComplete += CheckWorks_OnRunComplete;
        }

        private void CheckWorks_OnRunComplete()
        {
            settingWindowViewModel.ApiCheckMessage = string.Empty;
            if (KeysFailed.Count == 0)
            {
                MessageBox.Show("Không có key chết", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                string keys = string.Join("\r\n", KeysFailed);
                Clipboard.SetText(keys);
                if (MessageBox.Show($"Có {KeysFailed.Count} key chết\r\nĐã lưu vào clipboard\r\nBấm Yes để tự động xóa\r\n\r\n{keys}", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    string temp = settingWindowViewModel.ApiKeys;
                    KeysFailed.ForEach(x => temp = temp.Replace(x, string.Empty));
                    settingWindowViewModel.ApiKeys = string.Join("\r\n", temp.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)));
                }
            }
        }

        private async Task CheckWorks_OnWorkComplete(Task task, WorkEventArgs<CheckWork> workEventArgs)
        {
            CurrentChecked++;
            settingWindowViewModel.ApiCheckMessage = $"({CurrentChecked}/{KeyCount})";
            if (!workEventArgs.Work.IsSuccess) KeysFailed.Add(workEventArgs.Work.ApiKey);
        }

        int KeyCount = 0;
        int CurrentChecked = 0;


        private void btn_checkapi_Click(object sender, RoutedEventArgs e)
        {
            if (!checkWorks.IsRunning)
            {
                SettingData.LoadApiKey();
                var keys = SettingData.GetAllKey();
                KeyCount = keys.Count;
                CurrentChecked = 0;
                settingWindowViewModel.ApiCheckMessage = $"({CurrentChecked}/{KeyCount})";
                KeysFailed.Clear();
                checkWorks.ShutDown();
                foreach (var key in keys) checkWorks.Add(new CheckWork(key));
                checkWorks.MaxRun = 1;
            }
            else
            {
                checkWorks.ShutDown();
            }
        }
    }
}
