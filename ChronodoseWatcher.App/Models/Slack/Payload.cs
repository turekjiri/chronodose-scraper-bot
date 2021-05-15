using Newtonsoft.Json;

namespace ChronodoseWatcher.App.Models.Slack
{
    public class Payload
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        public Payload(string text)
        {
            this.Text = text;
        }
    }
}
