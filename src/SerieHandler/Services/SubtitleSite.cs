using NUnrar.Archive;
using System.IO;
using System.Linq;
using SerieHandler.Types;
using System.Text.RegularExpressions;

namespace SerieHandler.Services
{
    public abstract class SubtitleSite
    {
        public abstract FileInfo GetSubtitle(Episode episode);
        
        protected FileInfo UnrarSub(FileInfo fileRar, Episode episode)
        {
            FileInfo subtitle = null;

            try
            {
                RarArchive rar = RarArchive.Open(fileRar);

                RarArchiveEntry subEnt = null;

                string q = episode.FileVideo.Name.Replace(episode.FileVideo.Extension, "").ToLower();
                q = Regex.Replace(q, @"\[.+?\]", "");

                //Buscando legenda pelo nome do video
                try
                { subEnt = rar.Entries.First(r => r.FilePath.ToLower().Contains(q) && r.FilePath.Contains(".srt")); }
                catch { }

                //Buscando legenda pelo episodio
                if (subEnt == null)
                {
                    q = episode.SeasonEpisode.ToLower();
                    subEnt = rar.Entries.First(r => r.FilePath.ToLower().Contains(q) && r.FilePath.Contains(".srt"));
                }

                subEnt.WriteToDirectory(episode.FileVideo.DirectoryName);

                subtitle = new FileInfo(episode.FileVideo.DirectoryName + "/" + subEnt.FilePath);
            }
            catch { }

            return subtitle;
        }
    }
}
