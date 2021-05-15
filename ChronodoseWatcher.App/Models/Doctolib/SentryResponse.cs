using Newtonsoft.Json;

namespace ChronodoseWatcher.App.Models.Doctolib
{
    class SentryResponse
    {
        //[JsonProperty("availabilities")]
        //public Availability[] Availabilities { get; set; }// "availabilities": [],

        [JsonProperty("total")]
        public int Total { get; set; } //"total": 0,

        //[JsonProperty("reason")]
        //public string Reason { get; set; } //"reason": "no_availabilities",

        //[JsonProperty("message")]
        //public string Message { get; set; } //"message": "Aucun rendez-vous de vaccination n'est disponible dans ce lieu d'ici demain soir.",

        [JsonProperty("search_result")]
        public Centre Centre { get; set; } // "search_result": {}
    }
}
