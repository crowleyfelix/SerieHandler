using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace SerieHandler
{
    class Program
    {
        static void Main(string[] args)
        {            
            Console.Title = "Serie Handler";
            
            string rootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + @"\Series\";
            
            if (args.Length==2)
            {
                if (args[0].Contains(rootDirectory))
                    new Handler(args[0],args[1]).Start();
            }
            else
            {

                List<string> seriesDirectory = new List<string>();
                try
                {
                    seriesDirectory = Directory.GetDirectories(rootDirectory).ToList();
                }
                catch (DirectoryNotFoundException notFound)
                {
                    if (notFound != null)
                        Directory.CreateDirectory(rootDirectory);
                }

                if (seriesDirectory.Count == 0)
                    Console.WriteLine("Nenhuma série encontrada na pasta " + rootDirectory);
                else
                {

                    foreach (var serieDirectory in seriesDirectory)
                    {
                        new Handler(serieDirectory).Start();
                    }
                }
            }

            System.Threading.Thread.Sleep(5000);

        }

        private static void Output(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
