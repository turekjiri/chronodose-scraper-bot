using System;
using HtmlAgilityPack;

namespace ChronodoseWatcher.App.Business
{
    /// <summary>
    /// Déplacé pour tester xunit
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Get the number of search result pages
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static int ParsePagesCount(string html)
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
                Console.WriteLine("Recherche a retourné plus que 9 pages => limitation aux 9 premières pages");
                n = 9;
            }
            return n;
        }
    }
}