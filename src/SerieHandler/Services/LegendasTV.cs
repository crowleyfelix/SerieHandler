using HtmlAgilityPack;
using SerieHandler.Types;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SerieHandler.Services
{
    public class LegendasTV : SubtitleSite, IEpisodeSearch
    {

        public string Login { get; set; }
        public string Password { get; set; }

        public LegendasTV(string login, string senha)
        {
            Login = login;
            Password = senha;
        }

        public override FileInfo GetSubtitle(Episode episode)
        {
            FileInfo file = null;

            try
            {
                //Buscando série
                HtmlDocument html = new HtmlWeb().Load("http://legendas.tv/busca/" + episode.Serie);

                //Buscando temporada 
                var link = html.DocumentNode.SelectNodes("//div[@class='item']//a")
                    .Where(n => Regex.Match(n.InnerText, episode.Serie + @"\s*-\s*" + episode.Season + @".\s*temporada", RegexOptions.IgnoreCase).Success).First();

                string id = link.GetAttributeValue("data-filme", string.Empty);

                int page = 0;

                link = null;
                do
                {
                    html.LoadHtml(Util.RequestWeb("http://legendas.tv/legenda/busca/-/1/-/" + page + "/" + id));

                    //Se não for encontrada ir para próxima página
                    if (link == null)
                    {
                        //Buscando Episódio
                        link = SearchByEpisode(html,episode);

                        if (link == null)
                            link = SearchBySeason(html,episode);

                        if (html.DocumentNode.SelectNodes("//a").ToList().Exists(n => n.InnerText.Contains("carregar mais")))
                            page++;
                        else
                            break;
                    }

                } while (link == null);

                if (link != null)
                {
                    string urlDownload = "http://legendas.tv/downloadarquivo/" + Regex.Match(link.GetAttributeValue("href", string.Empty), "/download/(.+?/)").Groups[1].Value;

                    //Download da legenda
                    using (var client = new WebClient())
                    {
                        CookieContainer ckC = new CookieContainer();

                        //Logando no site
                        Util.RequestWeb("http://legendas.tv/login", "UTF-8", ckC, 3, "POST", string.Format("_method=POST&data%5BUser%5D%5Busername%5D={0}&data%5BUser%5D%5Bpassword%5D={0}",Login,Password));

                        #region Redirecionando Url de Download

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlDownload);

                        request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.10; rv:34.0) Gecko/20100101 Firefox/34.0";
                        request.Accept = "text/html,application/xhtml+xml,application/xml,application/json,text/javascript;q=0.9,*/*;q=0.01";
                        request.Method = "GET";
                        request.CookieContainer = ckC;
                        request.AllowAutoRedirect = true;
                        request.Referer = urlDownload;

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            urlDownload = response.ResponseUri.ToString();
                        }

                        #endregion

                        string pathDownload = episode.Serie.Directory + "/legendas/legenda" + episode.SeasonEpisode + ".rar";
                        
                        client.DownloadFile(urlDownload, pathDownload);

                        file = new FileInfo(pathDownload);

                        //Descompactando arquivo .rar
                        file = UnrarSub(file,episode);
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
                link = html.DocumentNode.SelectSingleNode("//a[contains(@href,'" + episode.SeasonEpisode + "')]");
            }
            catch
            {
            }

            return link;
        }

        public HtmlNode SearchBySeason(HtmlDocument html, Episode episode)
        {
            HtmlNode link = null;

            try
            {
                string query = episode.Serie.ToString().Replace(" ", ".") + "_" + Regex.Replace(episode.SeasonEpisode, "E.+", "_");

                link = html.DocumentNode.SelectSingleNode("//a[contains(@href,'" + query + "')]");
            }
            catch { }

            return link;
        }
        
    }
}
