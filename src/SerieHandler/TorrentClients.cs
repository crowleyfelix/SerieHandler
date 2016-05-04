using System.Collections.Generic;
using System.Linq;

namespace SerieHandler
{
    public static class TorrentClients
    {
        private static readonly string[] torrentList = {
            "BitTorrent",
            "uTorrent",
            "BitComet"
        };

        public static List<string> List()
        {
            return torrentList.ToList();
        }
    }
}
