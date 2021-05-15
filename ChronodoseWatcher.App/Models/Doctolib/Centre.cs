using Newtonsoft.Json;

namespace ChronodoseWatcher.App.Models.Doctolib
{
    public class Centre
    {
        [JsonProperty("id")]
        public int ID { get; set; } // "id": 6752794,

        //[JsonProperty("is_directory")]
        //public bool IsDirectory { get; set; } // "is_directory": false,

        [JsonProperty("address")]
        public string Address { get; set; } // "address": "98 Rue de Hochfelden",

        [JsonProperty("city")]
        public string City { get; set; } // "city": "Strasbourg",

        [JsonProperty("zipcode")]
        public string ZipCode { get; set; } // "zipcode": "67200",

        [JsonProperty("link")]
        public string Link { get; set; } // "link": "/centre-de-vaccinations-internationales/strasbourg/centre-de-vaccination-sos-medecin-67",

        [JsonProperty("first_name")]
        public string FirstName { get; set; } //"first_name": null,

        [JsonProperty("last_name")]
        public string LastName { get; set; } //"last_name": "Centre de vaccination - SOS médecin 67 (réservé aux professionnels de santé)",

        //[JsonProperty("visit_motive_id")]
        //public int VisitMotiveID { get; set; } //"visit_motive_id": 2706787,

        //[JsonProperty("visit_motive_name")]
        //public string VisitMotiveName { get; set; } //"visit_motive_name": "1re injection vaccin COVID-19 (Moderna)",

        [JsonProperty("url")]
        public string URL { get; set; } //"url": "/centre-de-vaccinations-internationales/strasbourg/centre-de-vaccination-sos-medecin-67?highlight%5Bspeciality_ids%5D%5B%5D=5494"

        //"agenda_ids": [413188],
        //"landline_number": null,
        //"booking_temporary_disabled": false,
        //"resetVisitMotive": false,
        //"toFinalizeStep": false,
        //"toFinalizeStepWithoutState": false,
        //public string CloudinaryPublicId { get; set; } //"cloudinary_public_id": "default_organization_avatar",
        //"profile_id": 251760,
        //"exact_match": null,
        //"priority_speciality": false,
        //"name_with_title": "Centre de vaccination - SOS médecin 67 (réservé aux professionnels de santé)",
        //"speciality": null,
        //"organization_status": "Centre de vaccinations internationales",
        //"top_specialities": ["1 salle de vaccination"],
        //"position": {"lat": 48.60227,"lng": 7.7226753},
        //"place_id": null,
        //"telehealth": false,
    }
}
