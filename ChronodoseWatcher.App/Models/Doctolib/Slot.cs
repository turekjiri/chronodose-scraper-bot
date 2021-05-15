using System;
using Newtonsoft.Json;

namespace ChronodoseWatcher.App.Models.Doctolib
{
    public class Slot
    {
        [JsonProperty("agenda_id")]
        public int? AgendaId { get; set; } // "agenda_id": 473651

        [JsonProperty("practitioner_agenda_id")]
        public int? PractitionerAgendaId { get; set; } // "practitioner_agenda_id": null

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; } // "start_date": "2021-05-16T17:55:00.000+02:00"

        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; } // "end_date": "2021-05-16T18:00:00.000+02:00"

        [JsonProperty("steps")]
        public Step[] Steps { get; set; } // "steps": []




    }
}
