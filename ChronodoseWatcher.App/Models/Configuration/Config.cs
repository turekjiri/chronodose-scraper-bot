using Newtonsoft.Json;

namespace ChronodoseWatcher.App.Models.Configuration
{
    public class Config
    {
        [JsonProperty("slack")]
        public Slack Slack { get; set; }
    }
}
