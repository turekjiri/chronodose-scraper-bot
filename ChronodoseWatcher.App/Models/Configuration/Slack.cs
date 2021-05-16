using Newtonsoft.Json;

namespace ChronodoseWatcher.App.Models.Configuration
{
    public class Slack
    {
        [JsonProperty("notify_slack")]
        public bool NotifySlack { get; set; }

        [JsonProperty("webhook_url")]
        public string WebhookURL { get; set; }

        [JsonProperty("send_errors")]
        public bool SendErrors { get; set; }

        [JsonProperty("minimum_free_places_to_notify")]
        public int MinimumFreePlacesToNotify { get; set; }
    }
}
