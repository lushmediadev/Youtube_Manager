using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Queues.TaskQueues;

namespace YoutubeManager.Works
{
    class CheckWork : BaseWork
    {
        public bool IsSuccess { get; set; } = false;
        public readonly string ApiKey;
        public CheckWork(string ApiKey)
        {
            this.ApiKey = ApiKey;
        }

        private static readonly Random rd = new Random();
        static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[rd.Next(s.Length)]).ToArray());
        }
        static string RandomString(int min, int max) => RandomString(rd.Next(min, max + 1));

        public async override Task DoWorkAsync()
        {
            try
            {
                using YouTubeService youTubeService = new YouTubeService(new BaseClientService.Initializer() { ApiKey = ApiKey });
                var req = youTubeService.Search.List(new Repeatable<string>(new List<string>() { "snippet" }));
                req.Q = RandomString(3, 7);
                await req.ExecuteAsync();
                IsSuccess = true;
            }
            catch (Exception)
            {

            }
        }
    }
}
