using HtmlAgilityPack;
using SerieHandler.Types;
using System;
using System.IO;
using System.Net;

namespace SerieHandler.Services
{
    public class Insubs : SubtitleSite, IEpisodeSearch
    {
        public override FileInfo GetSubtitle(Episode episode)
        {

            FileInfo file = null;

            try
            {
                CookieContainer ckC = new CookieContainer();

                //Buscando série
                HtmlDocument html = new HtmlWeb().Load("http://insubs.com/busca?q=" + episode.ToString());

                HtmlNode link = null;

                //Buscando Episódio
                link = SearchByEpisode(html,episode);

                if (link != null)
                {
                    //Acessando página da legenda
                    html = new HtmlWeb().Load(link.GetAttributeValue("href", string.Empty));

                    //Encontrando nó que contém url de Download
                    link = html.DocumentNode.SelectSingleNode("//a");

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
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\t->Legenda não encontrada");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t->Problemas:" + ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }

            return file;
        }

        public HtmlNode SearchByEpisode(HtmlDocument html, Episode episode)
        {
            HtmlNode link = null;

            try
            {
                link = html.DocumentNode.SelectSingleNode("//a");
            }
            catch { }

            return link;
        }

        public HtmlNode SearchBySeason(HtmlDocument html, Episode episode)
        {
            throw new NotImplementedException();
        }
    }
}
