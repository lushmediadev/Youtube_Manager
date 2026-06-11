using Google.Apis.YouTube.v3.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TqkLibrary.Queues.TaskQueues;
using TqkLibrary.WpfUi;
using TqkLibrary.WpfUi.UserControls;
using YoutubeManager.DataClass;
using YoutubeManager.Enums;
using YoutubeManager.Works;
using YoutubeManager.UI.ViewModels;

namespace YoutubeManager.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly WorkQueue<ChannelLoadWork> ChannelWorks;
        readonly MainWindowViewModel mainWindowViewModel;
        public MainWindow()
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Datas");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Datas\\Images");


            if (Singleton.Setting.Data.CustomColumns == null) Singleton.Setting.Data.CustomColumns = new List<DataClass.CustomColumn>();
            for (int i = Singleton.Setting.Data.CustomColumns.Count; i < 10; i++)
            {
                Singleton.Setting.Data.CustomColumns.Add(new CustomColumn()
                {
                    IsShow = false,
                    Name = $"CustomColumn {i}",
                    Size = 0
                });
            }
            Singleton.Setting.TriggerSave();

            Singleton.IconLoad.OnWorkComplete += IconLoad_OnWorkComplete;

            InitializeComponent();
            this.mainWindowViewModel = this.DataContext as MainWindowViewModel ?? throw new InvalidOperationException();
            ChannelWorks = new WorkQueue<ChannelLoadWork>();
            ChannelWorks.OnWorkComplete += ChannelWorks_OnWorkComplete;
            ChannelWorks.OnRunComplete += ChannelWorks_OnRunComplete;
            SettingData.LoadApiKey();
        }

        private void ChannelWorks_OnRunComplete()
        {
            ChannelWorks.MaxRun = 0;
            mainWindowViewModel.ProgressMax = 0;
            mainWindowViewModel.ProgressValue = 0;
            mainWindowViewModel.ProgressText = string.Empty;
        }

        private async Task ChannelWorks_OnWorkComplete(Task task, WorkEventArgs<ChannelLoadWork> workEventArgs)
        {
            mainWindowViewModel.ProgressValue++;
            mainWindowViewModel.ProgressText = $"Nhấn để hủy ({mainWindowViewModel.ProgressValue}/{mainWindowViewModel.ProgressMax})";
            this.Dispatcher.Invoke(() => workEventArgs.Work.channelViewModel.IsRequestLimitExceeded = workEventArgs.Work.IsRequestLimitExceeded);

            if (workEventArgs.Work.ChannelListResponse == null) return;
            Channel? channel = workEventArgs.Work.ChannelListResponse?.Items?.FirstOrDefault();
            if (channel != null)
            {
                workEventArgs.Work.channelViewModel.Title = channel.Snippet.Title;
                workEventArgs.Work.channelViewModel.VideoCount = channel.Statistics.VideoCount;
                workEventArgs.Work.channelViewModel.SubscriberCount = channel.Statistics.SubscriberCount;
                if (workEventArgs.Work.channelViewModel.ViewCount.HasValue && workEventArgs.Work.channelViewModel.LastTime.HasValue)
                {
                    workEventArgs.Work.channelViewModel.DayDiffUpdate = DateTime.Now - workEventArgs.Work.channelViewModel.LastTime.Value;
                    workEventArgs.Work.channelViewModel.ViewCountChanged = channel.Statistics.ViewCount - workEventArgs.Work.channelViewModel.ViewCount;
                }
                else
                {
                    workEventArgs.Work.channelViewModel.DayDiffUpdate = null;
                }
                workEventArgs.Work.channelViewModel.LastTime = DateTime.Now;
                workEventArgs.Work.channelViewModel.ViewCount = channel.Statistics.ViewCount;

                workEventArgs.Work.channelViewModel.Data.IsLive = true;
                this.Dispatcher.Invoke(() => workEventArgs.Work.channelViewModel.ChangeBackground(true));
                workEventArgs.Work.channelViewModel.Data.ImageUrl = channel.Snippet.Thumbnails.Default__.Url;
                if (string.IsNullOrEmpty(workEventArgs.Work.channelViewModel.Data.Id))
                {
                    if (mainWindowViewModel.Channels.Any(x => x.Data.GroupId == workEventArgs.Work.channelViewModel.Data.GroupId && channel.Id.Equals(x.Data.Id)))
                    {
                        this.Dispatcher.Invoke(() => mainWindowViewModel.Channels.Remove(workEventArgs.Work.channelViewModel));//dispatcher
                        return;
                    }
                    else workEventArgs.Work.channelViewModel.Data.Id = channel.Id;
                }
                Singleton.IconLoad.Add(new IconLoadWork(workEventArgs.Work.channelViewModel) { ForceDownload = true });
            }
            else
            {
                workEventArgs.Work.channelViewModel.Data.IsLive = false;
                this.Dispatcher.Invoke(() => workEventArgs.Work.channelViewModel.ChangeBackground(false));
            }
        }

        public bool ThumbnailCallback() => false;
        private async Task IconLoad_OnWorkComplete(Task task, WorkEventArgs<IconLoadWork> workEventArgs)
        {
            if ((workEventArgs.Work.ForceDownload || workEventArgs.Work.channelViewModel.Icon == null) && File.Exists(workEventArgs.Work.channelViewModel.Data.ImageLocal))
            {
                try
                {
                    using Bitmap bitmap = (Bitmap)Bitmap.FromFile(workEventArgs.Work.channelViewModel.Data.ImageLocal);
                    Bitmap thump = (Bitmap)bitmap.GetThumbnailImage(32, 32, ThumbnailCallback, IntPtr.Zero);
                    this.Dispatcher.Invoke(() =>
                    {
                        if (thump != null)
                        {
                            try
                            {
                                workEventArgs.Work.channelViewModel.Icon = thump.ToImageSource();
                            }
                            finally
                            {
                                thump.Dispose();
                            }
                        }
                    });

                }
                catch (Exception)
                {

                }
            }
        }

        //void SetThump(object obj_queue, object obj_thump)
        //{
        //  if(obj_queue is IconLoadQueue queue && obj_thump is Bitmap thump)
        //  {
        //    try
        //    {
        //      queue.channelViewModel.Icon = thump.ToImageSource();
        //    }
        //    finally
        //    {
        //      thump.Dispose();
        //    }
        //  }

        //}


        #region Groups
        private void LV_Group_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            MenuViewModel menuViewModel = menuItem.DataContext as MenuViewModel;
            var groupSelecteds = LV_Groups.SelectedItems.Cast<GroupYoutubeViewModel>().ToList();
            switch (menuViewModel.Action)
            {
                case MenuAction.Add:
                    GroupYoutubeData data = null;
                    do
                    {
                        data = new GroupYoutubeData() { GroupId = Guid.NewGuid(), Name = "New Item" };
                    }
                    while (mainWindowViewModel.Groups.Any(x => x.Data.GroupId == data.GroupId));

                    mainWindowViewModel.Groups.Add(new GroupYoutubeViewModel(data) { IsEditing = true });
                    break;
                case MenuAction.Delete:
                    if (groupSelecteds.Count != 0 &&
                      MessageBox.Show("Bạn có chắc chắc muốn xóa nhóm: " + groupSelecteds.First().Name, "Xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        mainWindowViewModel.Channels.Where(x => x.Data.GroupId == groupSelecteds.First().Data.GroupId).ToList().ForEach(x => mainWindowViewModel.Channels.Remove(x));
                        mainWindowViewModel.Groups.Remove(groupSelecteds.First());
                    }
                    break;
                case MenuAction.Edit:
                    if (groupSelecteds.Count != 0)
                    {
                        groupSelecteds.First().IsEditing = true;
                    }
                    break;
            }
        }

        private void LV_Groups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GroupYoutubeViewModel groupYoutubeViewModel = LV_Groups.SelectedItem as GroupYoutubeViewModel;
            if (groupYoutubeViewModel != null)
            {
                if (string.IsNullOrEmpty(tb_searchGroup.Text))
                {
                    mainWindowViewModel.Channels.ShowGroup(groupYoutubeViewModel.Data.GroupId);
                }
            }
        }

        private void tb_searchGroup_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                foreach (var group in mainWindowViewModel.Groups) group.IsVisible = true;
            }
            else
            {
                foreach (var group in mainWindowViewModel.Groups) group.IsVisible = group.Name.ToLower().Contains(textBox.Text.ToLower());
            }
        }

        private void btn_searchGroupsClear_Click(object sender, RoutedEventArgs e)
        {
            tb_searchGroup.Clear();
        }

        private void LV_Groups_KeyDown(object sender, KeyEventArgs e)
        {
            var groupSelecteds = (sender as ListView).SelectedItems.Cast<GroupYoutubeViewModel>().ToList();
            if (groupSelecteds.Count == 0) return;
            switch (e.Key)
            {
                case Key.Delete:
                    if (MessageBox.Show("Xác nhận xóa nhóm đã chọn", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        mainWindowViewModel.Channels.Where(x => x.Data.GroupId == groupSelecteds.First().Data.GroupId).ToList().ForEach(x => mainWindowViewModel.Channels.Remove(x));
                        mainWindowViewModel.Groups.Remove(groupSelecteds.First());
                    }
                    break;
            }
        }

        private void LV_Groups_Head_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader gridViewColumnHeader &&
              (sender as ListView).ItemsSource is GroupYoutubeViewModelSaveObservableCollection groupYoutubeViewModels)
            {
                string headName = string.Empty;
                if (gridViewColumnHeader.Content is TextBlockEdit tbe) headName = tbe.Text;
                if (gridViewColumnHeader.Content is string str) headName = str;
                groupYoutubeViewModels.Sort(headName);
            }
        }
        #endregion

        #region Channel
        private async void LV_Channel_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GroupYoutubeViewModel? groupYoutubeViewModel = LV_Groups.SelectedItem as GroupYoutubeViewModel;
                MenuItem? menuItem = sender as MenuItem;
                MenuViewModel? menuViewModel = menuItem?.DataContext as MenuViewModel;
                if (menuViewModel is null)
                    return;
                var channelSelecteds = LV_Channels.SelectedItems.Cast<ChannelViewModel>().OrderBy(x => mainWindowViewModel.Channels.IndexOf(x)).ToList();
                switch (menuViewModel.Action)
                {
                    case MenuAction.Add:
                        if (groupYoutubeViewModel != null)
                        {
                            AddChannelWindow addChannelWindow = new AddChannelWindow() { Owner = this };
                            addChannelWindow.ShowDialog();
                            if (addChannelWindow.IsAdd)
                            {
                                var lines = addChannelWindow.Urls.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
                                foreach (var line in lines)
                                {
                                    if (Uri.TryCreate(line.Trim(), UriKind.Absolute, out Uri? uri))
                                    {
                                        ChannelData? channelData = ChannelData.Parse(uri);
                                        if (channelData is not null)
                                        {
                                            channelData.GroupId = groupYoutubeViewModel.Data.GroupId;
                                            channelData.Title = channelData.Query;
                                            channelData.IsLive = false;
                                            switch (channelData.QueryType)
                                            {
                                                case YoutubeQueryType.None:
                                                    continue;
                                                case YoutubeQueryType.Id:
                                                    if (mainWindowViewModel.Channels
                                                        .Any(x => x.Data.GroupId == groupYoutubeViewModel.Data.GroupId && channelData.Query.Equals(x.Data.Id)))
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        channelData.Id = channelData.Query;
                                                        channelData.Title = channelData.Query;
                                                    }
                                                    break;

                                                case YoutubeQueryType.CustomUrl:
                                                case YoutubeQueryType.Handle:
                                                case YoutubeQueryType.Username:
                                                    break;
                                            }
                                            ChannelViewModel channelViewModel = new ChannelViewModel(channelData);
                                            mainWindowViewModel.Channels.Add(channelViewModel);
                                            await mainWindowViewModel.Channels.ShowGroupAsync(groupYoutubeViewModel.Data.GroupId);
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case MenuAction.Open:
                        {
                            ChannelViewModel? channelViewModel = channelSelecteds.FirstOrDefault();
                            if (channelViewModel != null)
                            {
                                using (Process process = new Process())
                                {
                                    process.StartInfo.FileName = "https://www.youtube.com/channel/" + channelViewModel.Data.Id;
                                    process.Start();
                                }
                            }
                        }
                        break;

                    case MenuAction.Delete:
                        if (channelSelecteds.Count > 0
                          && MessageBox.Show("Bạn có chắc chắn muốn xóa các kênh đã chọn?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            channelSelecteds.ForEach(x => mainWindowViewModel.Channels.Remove(x));
                        }
                        break;

                    case MenuAction.CheckAllChannel:
                        if (string.IsNullOrEmpty(SettingData.GetRandomApi()))
                        {
                            MessageBox.Show("Chưa có ApiKey", "Lỗi");
                            return;
                        }
                        foreach (var channel in mainWindowViewModel.Channels)
                        {
                            ChannelWorks.Add(new ChannelLoadWork(channel));
                            mainWindowViewModel.ProgressMax++;
                        }
                        ChannelWorks.MaxRun = Singleton.Setting.Data.ThreadCount;
                        break;

                    case MenuAction.CheckChannelInGroup:
                        if (groupYoutubeViewModel != null)
                        {
                            if (string.IsNullOrEmpty(SettingData.GetRandomApi()))
                            {
                                MessageBox.Show("Chưa có ApiKey", "Lỗi");
                                return;
                            }
                            foreach (var channel in mainWindowViewModel.Channels.Where(x => x.Data.GroupId == groupYoutubeViewModel.Data.GroupId))
                            {
                                ChannelWorks.Add(new ChannelLoadWork(channel));
                                mainWindowViewModel.ProgressMax++;
                            }
                            ChannelWorks.MaxRun = Singleton.Setting.Data.ThreadCount;
                        }
                        break;

                    case MenuAction.CopyAddress:
                        {
                            string channels = string.Join("\r\n", channelSelecteds.Select(x => "https://www.youtube.com/channel/" + x.Data.Id));
                            Clipboard.SetText(channels);
                        }
                        break;

                    case MenuAction.MoveToBottom:
                        {
                            foreach (var channel in channelSelecteds) mainWindowViewModel.Channels.Move(mainWindowViewModel.Channels.IndexOf(channel), mainWindowViewModel.Channels.Count - 1);
                        }
                        break;

                    case MenuAction.MoveToTop:
                        {
                            int i = 0;
                            foreach (var channel in channelSelecteds) mainWindowViewModel.Channels.Move(mainWindowViewModel.Channels.IndexOf(channel), i++);
                        }
                        break;

                    case MenuAction.DeleteAllDead:
                        {
                            if (MessageBox.Show("Bạn có chắc chắn muốn xóa toàn bộ kênh chết?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                mainWindowViewModel.Channels.Where(x => !x.Data.IsLive).ToList()
                                  .ForEach(x => mainWindowViewModel.Channels.Remove(x));
                            }
                        }
                        break;
                    case MenuAction.DeleteAllDeadInGroup:
                        {
                            if (groupYoutubeViewModel != null &&
                              MessageBox.Show("Bạn có chắc chắn muốn xóa toàn bộ kênh chết trong nhóm?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                mainWindowViewModel.Channels.Where(x => !x.Data.IsLive && x.Data.GroupId == groupYoutubeViewModel.Data.GroupId).ToList()
                                  .ForEach(x => mainWindowViewModel.Channels.Remove(x));
                            }
                        }
                        break;

                    case MenuAction.CheckChannelSelected:
                        {
                            if (string.IsNullOrEmpty(SettingData.GetRandomApi()))
                            {
                                MessageBox.Show("Chưa có ApiKey", "Lỗi");
                                return;
                            }
                            foreach (var channel in channelSelecteds)
                            {
                                ChannelWorks.Add(new ChannelLoadWork(channel));
                                mainWindowViewModel.ProgressMax++;
                            }
                            ChannelWorks.MaxRun = Singleton.Setting.Data.ThreadCount;
                        }
                        break;

                    case MenuAction.ExportTxt:
                        {
                            if (channelSelecteds.Count == 0) return;
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                            saveFileDialog.FileName = $"Report {DateTime.Now:HH-mm dd-MM-yyyy}.txt";
                            saveFileDialog.Filter = "txt|*.txt|all file|*.*";
                            if (saveFileDialog.ShowDialog() == true)
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (var channel in channelSelecteds)
                                {
                                    stringBuilder.AppendLine($"{channel.Title}\thttps://www.youtube.com/channel/{channel.Data.Id}\t{channel.LastTime:HH:mm dd/MM/yyyy}\t{channel.VideoCount}\t{channel.SubscriberCount}\t{channel.ViewCount}");
                                }
                                File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString());
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", ex.GetType().FullName);
            }
        }

        private void tbe_CustomHeadData_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlockEdit textBlockEdit = sender as TextBlockEdit;
            if (e.LeftButton == MouseButtonState.Pressed && textBlockEdit != null)
            {
                ChannelViewModel channelViewModel = textBlockEdit.DataContext as ChannelViewModel;
                int tag = int.Parse(textBlockEdit.Tag as string);
                channelViewModel.CustomColumnDatas[tag].IsEditing = true;
            }
        }

        private void tb_searchChannel_TextChanged(object sender, TextChangedEventArgs e)
        {
            GroupYoutubeViewModel groupYoutubeViewModel = LV_Groups.SelectedItem as GroupYoutubeViewModel;
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                if (groupYoutubeViewModel != null)
                {
                    mainWindowViewModel.Channels.ShowGroup(groupYoutubeViewModel.Data.GroupId);
                }
                else mainWindowViewModel.Channels.Show(x => false);
            }
            else mainWindowViewModel.Channels.Show(x =>
            {
                if (string.IsNullOrEmpty(x.Title))
                    return false;

                if (mainWindowViewModel.Groups.SelectedItem is not null && mainWindowViewModel.Groups.SelectedItem.Data.GroupId != x.Data.GroupId)
                    return false;

                return mainWindowViewModel.IsFindById ?
                    x.Data.Id.ToLower().Contains(textBox.Text.ToLower()) :
                    x.Title.ToLower().Contains(textBox.Text.ToLower());
            });
        }

        private void btn_searchChannelsClear_Click(object sender, RoutedEventArgs e)
        {
            tb_searchChannel.Clear();
        }

        private void LV_Channels_KeyDown(object sender, KeyEventArgs e)
        {
            var channelSelecteds = (sender as ListView).SelectedItems.Cast<ChannelViewModel>().ToList();
            if (channelSelecteds.Count == 0) return;
            switch (e.Key)
            {
                case Key.Delete:
                    if (MessageBox.Show("Xác nhận xóa các kênh đã chọn", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        channelSelecteds.ForEach(x => mainWindowViewModel.Channels.Remove(x));
                    }
                    break;
            }
        }

        private void LV_Channels_Head_Click(object sender, RoutedEventArgs e)
        {
            GroupYoutubeViewModel groupYoutubeViewModel = LV_Groups.SelectedItem as GroupYoutubeViewModel;
            if (groupYoutubeViewModel != null && e.OriginalSource is GridViewColumnHeader gridViewColumnHeader &&
              (sender as ListView).ItemsSource is ChannelViewModelSaveObservableCollection channelViewModels)
            {
                //if (gridViewColumnHeader.Content is TextBlockEdit tbe) headName = tbe.Text;
                if (gridViewColumnHeader.Content is string str) channelViewModels.Sort(str, groupYoutubeViewModel.Data.GroupId);
                else if (gridViewColumnHeader.Content is TextBlockEdit tbe)
                {
                    int tag = int.Parse(tbe.Tag as string);
                    channelViewModels.Sort(tag, mainWindowViewModel.CustomCols[tag], groupYoutubeViewModel.Data.GroupId);
                }
            }
        }

        private void tbe_CustomHead_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlockEdit textBlockEdit = sender as TextBlockEdit;
            if (e.LeftButton == MouseButtonState.Pressed && textBlockEdit != null)
            {
                int tag = int.Parse(textBlockEdit.Tag as string);
                mainWindowViewModel.CustomCols[tag].IsEditing = true;
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem listViewItem)
            {
                ChannelViewModel channelViewModel = listViewItem.DataContext as ChannelViewModel;
                if (channelViewModel != null)
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = "https://www.youtube.com/channel/" + channelViewModel.Data.Id;
                        process.StartInfo.UseShellExecute = true;
                        process.Start();
                    }
                }
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem listViewItem)
            {
                ChannelViewModel channelViewModel = listViewItem.DataContext as ChannelViewModel;
                if (channelViewModel != null)
                {
                    LV_Groups.SelectedIndex = mainWindowViewModel.Groups.IndexOf(mainWindowViewModel.Groups.FirstOrDefault(x => x.Data.GroupId == channelViewModel.Data.GroupId));
                }
            }
        }
        #endregion


        private void btn_setting_Click(object sender, RoutedEventArgs e)
        {
            new SettingWindow() { Owner = this }.ShowDialog();
            SettingData.LoadApiKey();
        }

        private void btn_restore_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json file|*.json|All File|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    try
                    {
                        BackupData backupData = JsonConvert.DeserializeObject<BackupData>(File.ReadAllText(openFileDialog.FileName));
                        mainWindowViewModel.Channels.Clear();
                        mainWindowViewModel.Channels.Load(backupData.ChannelDatas, x => new ChannelViewModel(x));
                        mainWindowViewModel.Groups.Clear();
                        backupData.GroupYoutubeDatas.ForEach(x => mainWindowViewModel.Groups.Add(new GroupYoutubeViewModel(x)));
                        MessageBox.Show("Khôi phục thành công", "Thông báo");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy file", "Lỗi");
                }
            }
        }

        private void btn_backup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BackupData backupData = new BackupData();
                backupData.ChannelDatas.AddRange(mainWindowViewModel.Channels.ViewModels.Select(x => x.Data));
                backupData.GroupYoutubeDatas.AddRange(mainWindowViewModel.Groups.Select(x => x.Data));
                string data = JsonConvert.SerializeObject(backupData);
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "json file|*.json|All File|*.*";
                saveFileDialog.FileName = $"Backup_{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.json";
                saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false)) streamWriter.Write(data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            ChannelWorks.ShutDown();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (LV_Channels.Equals(e.Source))
            {
                var channelSelecteds = LV_Channels.SelectedItems.Cast<ChannelViewModel>().OrderBy(x => mainWindowViewModel.Channels.IndexOf(x)).ToList();
                if (channelSelecteds.Count == 0) return;
                string channels = string.Join("\r\n", channelSelecteds.Select(x => "https://www.youtube.com/channel/" + x.Data.Id));
                Clipboard.SetText(channels);
            }
        }

        private void LV_Channels_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e)
        {

        }
    }
}