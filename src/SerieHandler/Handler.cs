using SerieHandler.Services;
using SerieHandler.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SerieHandler
{
    public class Handler
    {

        private Serie serie;
        private string fileName;

        public Handler(string directoryPath, string fileName = "")
        {
            serie = new Serie(new DirectoryInfo(directoryPath));

            this.fileName = fileName;

            if (!Directory.Exists(serie.Directory.FullName + "/legendas"))
                Directory.CreateDirectory(serie.Directory.FullName + "/legendas");
        }

        public void Start()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Série: " + serie + "\n");
            Console.ForegroundColor = ConsoleColor.White;

            List<FileInfo> fileVideos = GetFileVideos();
            List<FileInfo> fileSubtitles = GetFileSubtitles();

            foreach (FileInfo fileVideo in fileVideos)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Episódio " + fileVideo.Name);
                Console.ForegroundColor = ConsoleColor.White;

                Episode ep = new Episode(serie, fileVideo);

                try
                {
                    if (!serie.Directory.FullName.Contains(ep.FileVideo.DirectoryName))
                    {
                        //Movendo para pasta raiz
                        string directory = ep.FileVideo.DirectoryName;

                        ep.MoveToRoot();

                        //Caso houver valor em fileName, congelar os processos de torrent
                        if (!string.IsNullOrEmpty(fileName))
                        {

                            List<Process> procs = new List<Process>();

                           TorrentClients.List().ForEach(p=>procs.AddRange(Process.GetProcessesByName(p)));

                            foreach (Process proc in procs)
                                ProcessHandler.FreezeThreads(proc);

                            try
                            {
                                Directory.Delete(directory, true);
                            }
                            finally
                            {
                                foreach (Process proc in procs)
                                {
                                    ProcessHandler.UnfreezeThreads(proc);
                                    proc.Dispose();
                                }
                            }

                        }
                        else
                            Directory.Delete(directory, true);
                    }

                    if (!string.IsNullOrEmpty(ep.SeasonEpisode))
                    {
                        ep.FileSubtitle = null;

                        try
                        { ep.FileSubtitle = fileSubtitles.First(l => l.Name.Contains(ep.SeasonEpisode)); }
                        catch { }

                        if (ep.FileSubtitle == null)
                        {
                            //Procurando legenda em múltiplas fontes de download
                            List<SubtitleSite> sites = new List<SubtitleSite>();

                            //Adicionar classe estática com uma lista de sites de legenda
                            sites.Add(new LegendasTV("crowleyfelix", "jasonator"));
                            sites.Add(new SoLegendas());

                            foreach (SubtitleSite site in sites)
                            {
                                ep.FileSubtitle = site.GetSubtitle(ep);

                                if (ep.FileSubtitle != null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine(" Ok");
                                    Console.ForegroundColor = ConsoleColor.White;

                                    ep.RenameFiles();

                                    break;
                                }

                            }
                            if (ep.FileSubtitle == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\t->Legenda não encontrada");
                                Console.ForegroundColor = ConsoleColor.White;
                            }

                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(" Ok");
                            Console.ForegroundColor = ConsoleColor.White;

                            ep.RenameFiles();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\t->Não possui indicação de temporada e episódio");
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\t->{0}", ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }

            }

            Console.Write("\n\n");

        }

        public List<FileInfo> GetFileVideos()
        {
            List<FileInfo> fileVideos;

            if (!string.IsNullOrEmpty(fileName))
                fileVideos = serie.Directory.GetFiles(fileName, SearchOption.AllDirectories).ToList();
            else
            {
                fileVideos = serie.Directory.GetFiles("*.mkv", SearchOption.AllDirectories)
                    .Concat(serie.Directory.GetFiles("*.avi", SearchOption.AllDirectories))
                    .Concat(serie.Directory.GetFiles("*.mp4", SearchOption.AllDirectories))
                    .Concat(serie.Directory.GetFiles("*.rmvb", SearchOption.AllDirectories))
                    .OrderBy(f => f.Name)
                    .ToList();
            }

            fileVideos.RemoveAll(v => v.Name.Contains(".sample"));

            return fileVideos;

        }

        public List<FileInfo> GetFileSubtitles()
        {
            return serie.Directory.GetFiles("*.srt").ToList();
        }
    }

}
