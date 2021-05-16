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
        private readonly Logger _logger;

        private readonly Config _config;
        private readonly string _config_file = "config.json";

        private readonly string _cityKey = "{city}";
        private readonly string _pageKey = "{page}";
        private readonly string _centreIdKey = "{centreId}";
        private string _doctolibSearchURL => $"https://www.doctolib.fr/vaccination-covid-19/{_cityKey}?ref_visit_motive_ids[]=6970&ref_visit_motive_ids[]=7005&force_max_limit=2&page={_pageKey}";
        private string _doctolibSentryURL => $"https://www.doctolib.fr/search_results/{_centreIdKey}.json?ref_visit_motive_ids%5B%5D=6970&ref_visit_motive_ids%5B%5D=7005&speciality_id=5494&search_result_format=json&force_max_limit=2";

        private string _city;

        /// <summary>
        /// ctor
        /// </summary>
        public App()
        {
            _appStartTime = DateTime.Now;
            _config = LoadConfigFromFile(_config_file);
            _logger = new Logger(_appStartTime);

            Console.WriteLine("Démarrage du bot - si vous avez paramétré les notifications, vous devriez en recevoir une dans quelques instants...");
            SendWebhook("Démarrage du bot - test de notifications !");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            InitCity();

            var stop = false;
            var i = 1;
            while (!stop) // never stop :)
            {
                try
                {
                    Console.WriteLine();
                    Console.WriteLine($"################## ITERATION {i} ##################");

                    var idList = GetSearchResultIDs();
                    Console.WriteLine($"{idList.Count} centres identifiés");
                    ProcessIDs(idList, i);

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

        private void InitCity()
        {
            _city = string.Empty;

            var cityIsOk = false;
            while (!cityIsOk)
            {
                Console.WriteLine();
                Console.Write("Merci de saisir votre ville (ex. : Strasbourg) : ");
                _city = Console.ReadLine()?.ToLower().Replace(" ", "-");

                if (GetSearchResultPage(_city, 1) == null)
                {
                    Console.WriteLine("Ville non trouvé, merci de vérifier votre saisie");
                }
                else
                {
                    cityIsOk = true;
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
        /// Get the main/first result page
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        private string GetSearchResultPage(string city, int pageNumber)
        {
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

            var n = htmlDoc.DocumentNode
                    .SelectNodes("//ul[@class='seo-magical-links']")[0]
                    .ChildNodes
                    .Count;

            //TODO Limité à 9 pages pour l'instant
            if (n > 9)
            {
                Console.WriteLine("Recherche a retourné plus que 9 pages => limitation à 9 premières pages");
                n = 9;
            }
            return n;
        }

        /// <summary>
        /// Récupére les ID des résultats de recherche de toutes les pages disponibles (limité à 9 pour l'instant)
        /// </summary>
        /// <returns></returns>
        private List<string> GetSearchResultIDs()
        {
            var ids = new List<string>();

            var firstPage = GetSearchResultPage(_city, 1);
            var pagesCount = ParsePagesCount(firstPage);

            for (var n = 1; n <= pagesCount; n++)
            {
                Console.WriteLine($"Analyse de la page {n}/{pagesCount} pour {_city}");

                ids.AddRange(
                    n == 1
                        ? ParseResultsIdsFromPage(firstPage)
                        : ParseResultsIdsFromPage(GetSearchResultPage(_city, n)));

                Thread.Sleep(500); // Do not overload the server
            }

            return ids;
        }

        /// <summary>
        /// 1 - récupére les résultats de recherche (1er page), dont le nombre de pages
        /// 2 - récupére les résultats des autres pages
        /// 3 - récupére les données pour chaque résultat via sentry de doctolib et notifie le client
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="iteration"></param>
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

                    var log = $"{_logger.GetFormattedDateTime()} | {iteration} | {i + 1}/{ids.Count} | {_city} | {deserialized.Total} places au { (deserialized.Centre == null ? "Centre NULL" : deserialized.Centre.LastName.Trim())} [{id}]";
                    _logger.WriteLine(log);

                    Thread.Sleep(1000);  // Do not overflow the server
                }
                catch (Exception e)
                {
                    var log = $"{_logger.GetFormattedDateTime()} | {iteration} | {i + 1}/{ids.Count} | {_city} | ERREUR [{id}] : {e.Message}";

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
                            new NameValueCollection { ["payload"] = JsonConvert.SerializeObject(new Payload(msg)) });
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
