using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using ChronodoseWatcher.App.Models.Configuration;
using ChronodoseWatcher.App.Models.Doctolib;
using ChronodoseWatcher.App.Models.Slack;

namespace ChronodoseWatcher.App
{
    public class App
    {
        private readonly DateTime _appStartTime;
        private Logger _logger;

        private readonly Config _config;
        private readonly string _config_file = "config.json";

        private readonly string _cityKey = "{city}";
        private readonly string _pageKey = "{page}";
        private readonly string _centreIdKey = "{centreId}";
        private string _doctolibSearchURL => $"https://www.doctolib.fr/vaccination-covid-19/{_cityKey}?ref_visit_motive_ids[]=6970&ref_visit_motive_ids[]=7005&force_max_limit=2&page={_pageKey}";
        private string _doctolibSentryURL => $"https://www.doctolib.fr/search_results/{_centreIdKey}.json?ref_visit_motive_ids%5B%5D=6970&ref_visit_motive_ids%5B%5D=7005&speciality_id=5494&search_result_format=json&force_max_limit=2";

        /// <summary>
        /// ctor
        /// </summary>
        public App()
        {
            _appStartTime = DateTime.Now;
            _config = LoadConfigFromFile(_config_file);

            // Slack test
            Console.WriteLine("Démarrage du bot - si vous avez paramétré les notifications, vous devriez en recevoir une dans quelques secondes...");
            SendWebhook("Démarrage du bot - test de notifications !");

            Run();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Run()
        {
            var city = string.Empty;
            string firstPage = null;

            var cityIsOk = false;
            while (!cityIsOk)
            {
                city = AskForCity();
                firstPage = GetPage(city);
                if (firstPage == null)
                {
                    Console.WriteLine("Ville non trouvé, merci de vérifier votre saisie");
                }
                else
                {
                    cityIsOk = true;
                }
            }

            // Init logger
            _logger = new Logger(city, _appStartTime);

            // Run scraper
            var stop = false;
            var i = 1;
            while (!stop) // never stop :)
            {
                try
                {
                    var idList = GetIDs(city, firstPage);
                    ProcessIDs(idList, i);

                    firstPage = null; // pour vider la premiere page apres la premiere iteration
                    Thread.Sleep(10000); // Dodo pour 10s pour eviter de se faire ban
                    i++;
                }
                catch (Exception e)
                {
                    _logger.WriteLine(e.Message);
                    SendWebhook(e.Message, true);
                }
            }
        }

        /// <summary>
        /// Initier la configuration depuis fichier
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        private Config LoadConfigFromFile(string configFile)
        {
            Config config = null;

            try
            {
                if (File.Exists(_config_file))
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@"config.json"));
                }
                else
                {
                    Console.WriteLine($"ERREUR : Fichier {_config_file} n'existe pas. Pour être notifié, il faut renommer le fichier config-exemple.json en config.json et y spécifier votre paramétrage souhaité");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERREUR lors de la récupération du fichier de config : {e.Message}");
            }

            return config ??= new Config(); // si null, init default config
        }

        /// <summary>
        /// Ask user to input search param 'city' to the cmd
        /// </summary>
        /// <returns></returns>
        private string AskForCity()
        {
            Console.Write("Merci de saisir votre ville (ex. : Strasbourg) : ");
            return Console.ReadLine()?.ToLower().Replace(" ", "-");
        }

        /// <summary>
        /// Get the main/first result page
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        private string GetPage(string city, int? pageNumber = null)
        {
            pageNumber ??= 1;

            Console.WriteLine($"Chargement des données de la page {pageNumber} ({city})");

            try
            {
                var url = _doctolibSearchURL.Replace(_cityKey, city).Replace(_pageKey, pageNumber.ToString());
                return new WebClient().DownloadString(url);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get the number of search result pages
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private int ParsePagesCount(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var n = htmlDoc.DocumentNode.SelectNodes("//ul[@class='seo-magical-links']")[0].ChildNodes.Count();

            //TODO Limité à 9 pages pour l'instant
            if (n > 9)
            {
                Console.WriteLine("Recherche a retourné plus que 9 pages => limitation à 9 pages");
                n = 9;
            }
            return n;
        }

        /// <summary>
        /// Récupére les ID des résultats de recherche de toutes les pages disponibles (limité à 9 pour l'instant)
        /// </summary>
        /// <param name="city"></param>
        /// <param name="firstPage"></param>
        /// <returns></returns>
        private List<string> GetIDs(string city, string firstPage = null)
        {
            var ids = new List<string>();

            if (firstPage == null)
            {
                firstPage = GetPage(city);
            }

            for (var n = 1; n <= ParsePagesCount(firstPage); n++)
            {
                ids.AddRange(
                    n == 1
                        ? ParseResultsIdsFromPage(firstPage)
                        : ParseResultsIdsFromPage(GetPage(city, n)));

                Thread.Sleep(500); // Do not overflow the server
            }

            return ids;
        }

        /// <summary>
        /// 1 - récupére les résultats de recherche (1er page), dont le nombre de pages
        /// 2 - récupére les résultats des autres pages
        /// 3 - récupére les données pour chaque résultat via sentry de doctolib et notifie le client
        /// </summary>
        /// <param name="city"></param>
        /// <param name="iteration"></param>
        /// <param name="firstPage"></param>
        /// <returns></returns>
        private void ProcessIDs(List<string> ids, int iteration)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];

                try
                {
                    var sentryURL = _doctolibSentryURL.Replace(_centreIdKey, id);

                    var deserialized = JsonConvert.DeserializeObject<SentryResponse>(
                        new StreamReader(
                            WebRequest.Create(sentryURL)
                            .GetResponse()
                            .GetResponseStream())
                        .ReadToEnd());

                    // Premier truc à faire => notifier client
                    if (deserialized.Total > 0)
                    {
                        SendWebhook($"Centre {id} | {deserialized.Total} places | https://www.doctolib.fr{deserialized?.Centre?.Link}");
                    }

                    var log = $"{_logger.GetFormattedDateTime()} | {iteration} {i + 1}/{ids.Count} | Centre {id} : {deserialized.Total} places ({ (deserialized.Centre == null ? "Centre NULL" : deserialized.Centre.LastName)})";
                    _logger.WriteLine(log);

                    Thread.Sleep(1000);  // Do not overflow the server
                }
                catch (Exception e)
                {
                    var log = $"{_logger.GetFormattedDateTime()} | {iteration} {i + 1}/{ids.Count} | Centre {id} : {e.Message}";

                    _logger.WriteLine(log);
                    SendWebhook(log, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="html"></param>
        private List<string> ParseResultsIdsFromPage(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var divs = htmlDoc.DocumentNode.SelectNodes("//div[@class='dl-search-result']");
            return divs.Select(d => d.Attributes["id"].Value.Replace("search-result-", "")).ToList();
        }


        /// <summary>
        /// Send msg to Slack
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="isException"></param>
        private void SendWebhook(string msg, bool isException = false)
        {
            try
            {

                if (_config.Slack != null && _config.Slack.NotifySlack)
                {
                    if (!isException || (isException && _config.Slack.SendErrors))
                    {
                        new WebClient().UploadValues(_config.Slack.WebhookURL, "POST",
                            new NameValueCollection {["payload"] = JsonConvert.SerializeObject(new Payload(msg))});
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERREUR : échec d'envoi de notification : {e.Message}");
            }

        }
    }
}
