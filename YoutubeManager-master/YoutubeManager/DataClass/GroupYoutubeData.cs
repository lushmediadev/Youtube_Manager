using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TqkLibrary.WpfUi.Interfaces;
using TqkLibrary.WpfUi.ObservableCollections;

namespace YoutubeManager.DataClass
{
    public class GroupYoutubeData : IItemData<Guid>
    {
        [JsonProperty("Guid")]
        public Guid GroupId { get; set; }
        public string Name { get; set; }
    }
}
