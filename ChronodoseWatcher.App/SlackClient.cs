using System;
using System.Collections.Specialized;
using System.Net;
using ChronodoseWatcher.App.Models.Configuration;
using ChronodoseWatcher.App.Models.Slack;
using Newtonsoft.Json;

namespace ChronodoseWatcher.App
{
    public class SlackClient
    {
        private readonly Logger _logger;
        private readonly Config _config;

        public SlackClient(Logger logger, Config config)
        {
            _logger = logger;
            _config = config;

            SendWelcome();
        }

        private void SendWelcome()
        {
            SendMessage("*Bienvenue !*" +
                        "\nSi vous avez reçu ce message, c'est que vos notifications sont correctement paramétrées");
        }

        /// <summary>
        /// Send msg to Slack
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="isException"></param>
        public void SendMessage(string msg, bool isException = false)
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
                _logger.WriteLine(e.Message.Contains("404")
                    ? $"ERREUR : Slack a répondu 404/NotFound, veuillez vérifier l'URL de votre Webhook"
                    : $"ERREUR : échec d'envoi de notification Slack : {e.Message}");
            }
        }
    }
}