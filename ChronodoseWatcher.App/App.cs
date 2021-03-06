using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using ChronodoseWatcher.App.Business;
using ChronodoseWatcher.App.Models.Configuration;
using ChronodoseWatcher.App.Models.Doctolib;
using ChronodoseWatcher.App.Tools;

namespace ChronodoseWatcher.App
{
    public class App
    {
        private readonly DateTime _appStartTime;

        private readonly Logger _logger;
        private readonly Config _config;
        private readonly SlackClient _slackClient;

        private readonly string _cityKey = "{city}";
        private readonly string _pageKey = "{page}";
        private readonly string _centreIdKey = "{centreId}";
        private string _doctolibSearchURL => $"https://www.doctolib.fr/vaccination-covid-19/{_cityKey}" +
                                             $"?ref_visit_motive_ids[]=6970" +
                                             $"&ref_visit_motive_ids[]=7005" +
                                             //$"&force_max_limit=2" +
                                             $"&page={_pageKey}";
        
        private string _doctolibSentryURL => $"https://www.doctolib.fr/search_results/{_centreIdKey}.json" +
                                             $"?ref_visit_motive_ids[]=6970" +
                                             $"&ref_visit_motive_ids[]=7005" +
                                             $"&speciality_id=5494" +
                                             $"&search_result_format=json" +
                                             $"&force_max_limit=2";

        private string _city;

        /// <summary>
        /// ctor
        /// </summary>
        public App(string configFilePath)
        {
            _appStartTime = DateTime.Now;
            _config = Config.LoadConfigFromFile(configFilePath);
            _logger = new Logger(_appStartTime);
            _slackClient = new SlackClient(_logger, _config);

            InitCity();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            var stop = false;
            var i = 1;
            while (!stop) // never stop :)
            {
                try
                {
                    Console.WriteLine();
                    Console.WriteLine($"################## ITERATION {i} ##################");

                    var idList = GetSearchResultIDs();
                    Console.WriteLine($"{idList.Count} centres identifiés autour de {_city}");
                    ProcessIDs(idList, i);

                    Thread.Sleep(10000); // Dodo pour 10s pour eviter de se faire ban
                    i++;
                }
                catch (Exception e)
                {
                    _logger.WriteLine(e.Message);
                    _slackClient.SendMessage(e.Message, true);
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
                _city = Console.ReadLine()?
                                .Replace(" ", "-")
                                .ToLower();

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
        /// Get the main/first result page
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        private string GetSearchResultPage(string city, int pageNumber)
        {
            try
            {
                var url = _doctolibSearchURL
                                .Replace(_cityKey, city)
                                .Replace(_pageKey, pageNumber.ToString());

                return new WebClient().DownloadString(url);
            }
            catch
            {
                return null;
            }
        }



        /// <summary>
        /// Récupére les ID des résultats de recherche de toutes les pages disponibles (limité à 9 pour l'instant)
        /// </summary>
        /// <returns></returns>
        private List<string> GetSearchResultIDs()
        {
            var ids = new List<string>();

            var firstPage = GetSearchResultPage(_city, 1);
            var pagesCount = Parser.ParsePagesCount(firstPage);

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
                    if (deserialized.Total > 0 && deserialized.Total >= _config.Slack.Threshold)
                    {
                        _slackClient.SendMessage(
                            $"*{deserialized.Total} places* à *{deserialized.Centre.LastName.Trim()}*" +
                            $"\nhttps://www.doctolib.fr{deserialized.Centre?.Link} [{id}]"
                            //+ $"\n[TEST : https://www.doctolib.fr{deserialized.Centre?.URL}]"
                            );
                    }

                    var log = $"{_logger.GetFormattedDateTime()}" +
                              $" | {iteration}" +
                              $" | {i + 1}/{ids.Count}" +
                              $" | {_city}" +
                              $" | {deserialized.Total} places au { (deserialized.Centre == null ? "Centre NULL" : deserialized.Centre.LastName.Trim())} [{id}]";

                    _logger.WriteLine(log);

                    Thread.Sleep(1000);  // Do not overload the server
                }
                catch (Exception e)
                {
                    var log = $"{_logger.GetFormattedDateTime()}" +
                              $" | {iteration}" +
                              $" | {i + 1}/{ids.Count}" +
                              $" | {_city}" +
                              $" | ERREUR [{id}] : {e.Message}";

                    _logger.WriteLine(log);
                    _slackClient.SendMessage(log, true);
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
            return divs.Select(d => d.Attributes["id"]
                                        .Value
                                        .Replace("search-result-", "")
                    ).ToList();
        }



    }
}
