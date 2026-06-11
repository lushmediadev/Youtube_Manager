using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Queues.TaskQueues;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3.Data;
using YoutubeManager.UI.ViewModels;
using YoutubeManager.DataClass;
using YoutubeExplode.Channels;
using YoutubeExplode;
using YoutubeManager.Enums;

namespace YoutubeManager.Works
{
    class ChannelLoadWork : BaseWork
    {
        static readonly YoutubeClient youtubeClient = new YoutubeClient();
        public readonly ChannelViewModel channelViewModel;
        public ChannelListResponse? ChannelListResponse { get; private set; }
        public bool IsRequestLimitExceeded { get; private set; } = false;
        public ChannelLoadWork(ChannelViewModel channelViewModel)
        {
            this.channelViewModel = channelViewModel;
        }

        public override async Task DoWorkAsync()
        {
            try
            {
                using YouTubeService youTubeService = new YouTubeService(new BaseClientService.Initializer() { ApiKey = SettingData.GetRandomApi() });
                var req = youTubeService.Channels.List(
                    new Repeatable<string>(
                        new List<string>() {
                            "brandingSettings",
                            "contentDetails",
                            "contentOwnerDetails",
                            "snippet",
                            "id",
                            "localizations",
                            "statistics",
                            "status",
                            "topicDetails"
                        }
                    )
                );

                if (!string.IsNullOrEmpty(channelViewModel.Data.Id)) req.Id = channelViewModel.Data.Id;
                else
                {
                    switch (channelViewModel.Data.QueryType)
                    {
                        case YoutubeQueryType.Id:
                            req.Id = channelViewModel.Data.Query;
                            break;

                        case YoutubeQueryType.Username:
                            req.ForUsername = channelViewModel.Data.Query;
                            break;

                        case YoutubeQueryType.Handle:
                            req.ForHandle = channelViewModel.Data.Query;
                            break;

                        case YoutubeQueryType.CustomUrl:
                            {
                                var result = await youtubeClient.Channels.GetBySlugAsync(channelViewModel.Data.Query);
                                req.Id = result.Id.ToString();
                                break;
                            }

                        default: return;
                    }
                }
                ChannelListResponse = await req.ExecuteAsync();
            }
            catch (YoutubeExplode.Exceptions.RequestLimitExceededException)
            {
                IsRequestLimitExceeded = true;
            }
            catch (Exception)
            {

            }
        }
    }
}
