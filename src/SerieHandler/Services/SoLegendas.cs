using HtmlAgilityPack;
using SerieHandler.Types;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SerieHandler.Services
{
    public class SoLegendas : SubtitleSite, IEpisodeSearch
    {
        public override FileInfo GetSubtitle(Episode episode)
        {

            FileInfo file = null;

            try
            {

                HtmlDocument html;
                HtmlNode link = null;

                //Buscando série
                for (int i = 0; i < 2; i++)
                {
                    switch (i)
                    {
                        //Buscando por episódio
                        case 0:
                            html = new HtmlWeb().Load("http://solegendas.com.br/?s=" + episode);
                            link = SearchByEpisode(html,episode);

                            if (link != null)
                                i = 2;

                            break;

                        //Buscando por temporada
                        case 1:
                            html = new HtmlWeb().Load("http://solegendas.com.br/?s=" + episode.Serie + " " + episode.Season + " temporada");
                            link = SearchBySeason(html,episode);
                            break;
                    }

                }
                
                if (link != null)
                {
                    //Acessando página da legenda
                    html = new HtmlWeb().Load(link.GetAttributeValue("href", string.Empty));

                    link = html.DocumentNode.SelectNodes("//a")
                    .Where(n => n.InnerText == "Download").First();

                    if (link != null)
                    {
                        string urlDownload = link.GetAttributeValue("href", string.Empty);

                        //Download da legenda
                        using (var client = new WebClient())
                        {
                            string pathDownload = episode.Serie.Directory + "/legendas/legenda" + episode.SeasonEpisode + ".rar";

                            client.DownloadFile(urlDownload, pathDownload);
                            file = new FileInfo(pathDownload);

                            //Descompactando arquivo .rar
                            file = UnrarSub(file, episode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return file;
        }

        public HtmlNode SearchByEpisode(HtmlDocument html, Episode episode)
        {
            HtmlNode link = null;

            try
            {
                link = html.DocumentNode.SelectNodes("//div[@class='item-details']//a[@itemprop='url']")
                .Where(
                        n => n.InnerText.ToLower().Contains(episode.Serie.ToString().ToLower()) &&
                        n.InnerText.ToLower().Contains(episode.SeasonEpisode.ToLower())
                      ).First();
            }
            catch { }

            return link;
        }

        public HtmlNode SearchBySeason(HtmlDocument html, Episode episode)
        {
            HtmlNode link = null;

            try
            {
                link = html.DocumentNode.SelectNodes("//div[@class='item-details']//a[@itemprop='url']")
                .Where(
                        n => Regex.Match(n.InnerText.ToLower(), episode.Serie.ToString().ToLower() + " " + episode.Season + ". temporada").Success
                      ).First();
            }
            catch { }

            return link;
        }
    }
}
