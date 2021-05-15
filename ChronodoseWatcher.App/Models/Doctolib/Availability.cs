using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChronodoseWatcher.App.Models.Doctolib
{
    public class Availability
    {
        [JsonProperty("date")]
        public DateTime date { get; set; }

        [JsonProperty("slots")]
        public List<Slot> Slots { get; set; }
    }
}
