using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TqkLibrary.Queues.TaskQueues;
using TqkLibrary.WpfUi;
using YoutubeManager.UI.ViewModels;

namespace YoutubeManager.Works
{
    class IconLoadWork : BaseWork
    {
        public bool ForceDownload { get; set; } = false;
        public readonly ChannelViewModel channelViewModel;
        public IconLoadWork(ChannelViewModel channelViewModel)
        {
            this.channelViewModel = channelViewModel;
        }

        static readonly HttpClient httpClient = new HttpClient();
        public async override Task DoWorkAsync()
        {
            if (ForceDownload || !File.Exists(channelViewModel.Data.ImageLocal))
            {
                string filePath = Directory.GetCurrentDirectory() + $"\\Datas\\Images\\{channelViewModel.Data.Id}.png";
                try
                {
                    if (!string.IsNullOrEmpty(channelViewModel.Data.ImageUrl))
                    {
                        using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, channelViewModel.Data.ImageUrl);
                        using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead);
                        httpResponseMessage.EnsureSuccessStatusCode();
                        using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                        Stream content = await httpResponseMessage.Content.ReadAsStreamAsync();
                        await content.CopyToAsync(fileStream);
                    }
                }
                catch (Exception)
                {

                }
                if (File.Exists(filePath)) channelViewModel.Data.ImageLocal = filePath;
            }

            if (File.Exists(channelViewModel.Data.ImageLocal)) await Task.Delay(50);
        }
    }
}
