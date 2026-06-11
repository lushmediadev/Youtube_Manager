using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeManager.DataClass
{
    class SettingData
    {
        public int ChannelColSTT { get; set; } = 30;
        public int GroupColName { get; set; } = 180;
        public int ChannelColIcon { get; set; } = 40;
        public int ChannelColTitle { get; set; } = 150;
        public int ChannelColLastTime { get; set; } = 80;
        public int ChannelColVideoCount { get; set; } = 60;
        public int ChannelColSubscriberCount { get; set; } = 80;
        public int ChannelColViewCount { get; set; } = 120;
        public int ChannelColViewChangedCount { get; set; } = 120;
        public int ChannelColNote1 { get; set; } = 150;
        public bool IsShowChannelColNote2 { get; set; } = true;
        public int ChannelColNote2 { get; set; } = 150;
        public bool IsShowChannelColNote3 { get; set; } = true;
        public int ChannelColNote3 { get; set; } = 150;
        public double ColGridMain { get; set; } = 200;

        public int ThreadCount { get; set; } = 1;
        public string ApiKeys { get; set; }
        public bool IsFindById { get; set; } = false;
        public List<CustomColumn> CustomColumns { get; set; }

        static readonly List<string> Apis = new List<string>();
        static readonly object _lock = new object();
        static readonly Random random = new Random();
        public static List<string> GetAllKey() => Apis.ToList();
        public static void LoadApiKey()
        {
            lock (_lock)
            {
                Apis.Clear();
                var keys = Singleton.Setting.Data.ApiKeys?.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
                if (keys != null) Apis.AddRange(keys);
            }
        }
        public static string GetRandomApi()
        {
            lock (_lock)
            {
                if (Apis.Count == 0) return string.Empty;
                else return Apis[random.Next(Apis.Count)];
            }
        }
    }

    public class CustomColumn
    {
        public string Name { get; set; }
        public int Size { get; set; } = 0;
        public bool IsShow { get; set; } = false;
    }
}
