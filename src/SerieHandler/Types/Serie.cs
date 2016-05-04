using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SerieHandler.Types
{
    public class Serie
    {
        public DirectoryInfo Directory { get; set; }

        public Serie(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException();

            Directory = directory;
        }

        public override string ToString()
        {
            return Directory.Name;
        }
    }

    public class Episode
    {
        public Serie Serie { get; set; }

        private FileInfo fileVideo;
        public FileInfo FileVideo
        {
            get
            {
                return fileVideo;
            }
            set
            {
                fileVideo = value;

                if (Regex.Match(value.Name.ToUpper(), @"S\d+E\d+", RegexOptions.IgnoreCase).Success)
                {
                    seasonEpisode = Regex.Match(value.Name.ToUpper(), @"S\d+E\d+").Value;
                    season = Convert.ToInt32(Regex.Match(seasonEpisode, @"S(\d+)").Groups[1].Value);
                    this.value = Convert.ToInt32(Regex.Match(seasonEpisode, @"E(\d+)").Groups[1].Value);
                }
            }
        }

        public FileInfo FileSubtitle { get; set; }

        private string seasonEpisode;
        public string SeasonEpisode
        {
            get { return seasonEpisode; }
        }

        private int season;
        public int Season
        {
            get { return season; }
        }

        private int value;
        public int Value
        {
            get { return value; }
        }

        public Episode(Serie serie, FileInfo fileVideo)
        {

            if (serie == null || fileVideo == null)
                throw new ArgumentNullException();

            FileVideo = fileVideo;
            Serie = serie;
        }

        public void RenameFiles()
        {
            RenameSubtitle();
            RenameVideo();
        }

        private void RenameSubtitle()
        {
            try { FileSubtitle.MoveTo(Serie.Directory.FullName + "\\" + ToString() + FileSubtitle.Extension); }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void RenameVideo()
        {
            try { FileVideo.MoveTo(Serie.Directory.FullName + "\\" + ToString() + FileVideo.Extension); }
            catch (Exception ex){throw ex;}
        }

        public void MoveToRoot()
        {
            try { FileVideo.MoveTo(Serie.Directory.FullName + "/" + fileVideo.Name); }
            catch (Exception ex) { throw ex; }
        }

        public override string ToString()
        {
            return Serie.Directory.Name + " " + SeasonEpisode;
        }
    }
}
