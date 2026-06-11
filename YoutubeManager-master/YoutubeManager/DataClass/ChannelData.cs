using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using TqkLibrary.WpfUi.Interfaces;
using TqkLibrary.WpfUi.ObservableCollections;
using YoutubeManager.Enums;

namespace YoutubeManager.DataClass
{
    public class ChannelData : IItemData<Guid>
    {
        //static readonly Regex regex_Id = new Regex("(?<=\\/channel\\/).*?$");//(?!channel\\/)[A-z0-9_-]{24}$
        //static readonly Regex regex_userName = new Regex("(?<=\\/user\\/).*?$");
        //static readonly Regex regex_name = new Regex("(?<=\\/c\\/).*?$");

        static readonly Regex regex_ytbUrl = new Regex("youtube\\..+?\\/(channel\\/|@|c\\/|user\\/)(.*?)(?:\\?|&|\\/|$)");

        //https://www.youtube.com/@<id>
        //https://www.youtube.com/c/<Name>
        //https://www.youtube.com/channel/<id>
        //https://www.youtube.com/user/<id>

        public static ChannelData? Parse(Uri uri)
        {
            Match match = regex_ytbUrl.Match(uri.OriginalString);
            if (match.Success)
            {
                string type = match.Groups[1].Value.TrimEnd('/').ToLower();
                YoutubeQueryType? youtubeQueryType = type switch
                {
                    "@" => YoutubeQueryType.Handle,
                    "c" => YoutubeQueryType.CustomUrl,
                    "channel" => YoutubeQueryType.Id,
                    "user" => YoutubeQueryType.Username,
                    _ => null,
                };
                string Query = match.Groups[2].Value;
                if (youtubeQueryType == YoutubeQueryType.CustomUrl)
                {
                    Query = HttpUtility.UrlDecode(Query);
                }
                if (youtubeQueryType.HasValue)
                {
                    return new ChannelData()
                    {
                        Query = Query,
                        QueryType = youtubeQueryType.Value,
                    };
                }
            }
            return null;
        }
        public required string Query { get; set; }
        public required YoutubeQueryType QueryType { get; set; } = YoutubeQueryType.None;



        public Guid GroupId { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageLocal { get; set; }

        public string? Id { get; set; }
        public string? Tag { get; set; }
        public string? Title { get; set; }
        public DateTime? LastTime { get; set; }
        public ulong? VideoCount { get; set; }
        public ulong? SubscriberCount { get; set; }
        public ulong? ViewCount { get; set; }
        public ulong? ViewCountChanged { get; set; }
        public TimeSpan? DayDiffUpdate { get; set; }
        public bool IsRequestLimitExceeded { get; set; } = false;
        public bool IsLive { get; set; } = false;

        public List<string> CustomColDatas { get; set; } = new();


        public override string ToString() => $"GroupId: {GroupId}, Id: {Id}, Title: {Title}";
    }
}
