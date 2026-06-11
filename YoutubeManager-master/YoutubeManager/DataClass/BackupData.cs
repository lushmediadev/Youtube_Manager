using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeManager.DataClass
{
    public class BackupData
    {
        public List<ChannelData> ChannelDatas { get; set; } = new List<ChannelData>();
        public List<GroupYoutubeData> GroupYoutubeDatas { get; set; } = new List<GroupYoutubeData>();
    }
}
