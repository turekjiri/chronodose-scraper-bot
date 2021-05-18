using System;
using System.IO;
using Newtonsoft.Json;

namespace ChronodoseWatcher.App.Models.Configuration
{
    public class Config
    {
        [JsonProperty("slack")]
        public Slack Slack { get; set; }

        /// <summary>
        /// Initier la configuration depuis fichier
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public static Config LoadConfigFromFile(string configFile)
        {
            Config config = null;

            try
            {
                if (File.Exists(configFile))
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile));

                    if (config == null)
                        throw new Exception();

                    Console.WriteLine("Fichier de configuration chargé");
                    Console.WriteLine($"- Notifications Slack : {config.Slack.NotifySlack}");
                    Console.WriteLine($"- Webhook URL : {config.Slack.WebhookURL}");
                    Console.WriteLine($"- Send Errors : {config.Slack.SendErrors}");
                    Console.WriteLine($"- Seuil mini de notifications : {config.Slack.Threshold} [vous serez notifié uniquement s'il y a >= de places disponibles]");
                }
                else
                {
                    Console.WriteLine($"ERREUR : Fichier '{configFile}' n'existe pas. Pour être notifié, il faut renommer le fichier config-exemple.json en config.json et y spécifier votre paramétrage");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERREUR lors de la récupération du fichier de config : {e.Message}");
            }

            return config ??= new Config(); // si null, init default config
        }
    }
}
