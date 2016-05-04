using HtmlAgilityPack;
using SerieHandler.Types;

namespace SerieHandler.Services
{
    interface IEpisodeSearch
    {
        HtmlNode SearchBySeason(HtmlDocument html, Episode episode);

        HtmlNode SearchByEpisode(HtmlDocument html, Episode episode);
    }
}
