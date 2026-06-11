using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TqkLibrary.Data.Json;
using TqkLibrary.Queues.TaskQueues;
using TqkLibrary.WpfUi;
using YoutubeManager.DataClass;
using YoutubeManager.Works;

namespace YoutubeManager
{
    static class Singleton
    {
        internal static string ExeDir { get; } = Directory.GetCurrentDirectory();
        public static string ListChannelPath { get; } = ExeDir + "\\Datas\\Channels.json";


        public const int MaxCol = 4;
        public static readonly string Key = Directory.GetCurrentDirectory() + "\\key.txt";
        public static SaveJsonData<SettingData> Setting { get; } = new SaveJsonData<SettingData>(Directory.GetCurrentDirectory() + "\\Settings.json");

        internal static WorkQueue<IconLoadWork> IconLoad { get; } = new WorkQueue<IconLoadWork>()
        {
            MaxRun = 1,
        };
    }
}
