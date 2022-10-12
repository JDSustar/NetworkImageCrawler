using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Console = System.Console;

namespace ImageSizeCrawler
{
    public class ImageInfo
    {
        public string Filename { get; set; }
        public string FullFilePath { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string LastModified { get; set; }
        public string Notes { get; set; }
    }

    public class ImageInfoMap : ClassMap<ImageInfo>
    {
        public ImageInfoMap()
        {
            Map(x => x.Filename).Index(0).Name("Filename");
            Map(x => x.FullFilePath).Index(1).Name("FullFilepath");
            Map(x => x.Height).Index(2).Name("Height");
            Map(x => x.Width).Index(3).Name("Width");
            Map(x => x.LastModified).Index(4).Name("LastModified");
            Map(x => x.Notes).Index(5).Name("Notes");

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var ImS = new ImageSearcher();

            string path;

            if (args.Length > 0)
            {
                path = args[0];
            }
            else
            {
                Console.WriteLine("Enter the path of the folder you want to traverse (right click to paste):");
                path = Console.ReadLine();
            }

            ImS.Traverse(path);

            ImS.Save("Output/ImageSizeCrawler-output-" + DateTime.Now.Ticks + ".csv");

            Console.WriteLine("Press Enter to close this window.");
            Console.ReadLine();
        }
    }

    public class ImageSearcher
    {
        private static string[] imageFiletypes = {"jpg", "png", "tif"};

        private List<ImageInfo> _images = new List<ImageInfo>();

        public static bool IsFileAnImage(string filename)
        {
            return imageFiletypes.ToList().Contains(filename.Split(".").Last());
        }

        public ImageSearcher()
        {

        }

        public void Traverse(string path)
        {
            GetDirectoryImageInfo(path);

            foreach (var directory in Directory.GetDirectories(path))
            {
                Traverse(directory);
            }
        }

        private void GetDirectoryImageInfo(string path)
        {
            Console.WriteLine(path);
            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                Console.WriteLine(file);
                if (IsFileAnImage(file))
                {
                    try
                    {
                        // https://gist.github.com/dejanstojanovic/c5df7310174b570c16bc
                        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (var image = Image.FromStream(fileStream, false, false))
                            {
                                var height = image.Height;
                                var width = image.Width;
                                _images.Add(new ImageInfo()
                                {
                                    Filename = Path.GetFileName(file), FullFilePath = Path.GetFullPath(file),
                                    Height = height, Width = width, LastModified = File.GetLastWriteTime(file).ToString()
                                });
                            }
                        }
                    }
                    catch
                    {
                        _images.Add(new ImageInfo()
                        {
                            Filename = Path.GetFileName(file),
                            FullFilePath = Path.GetFullPath(file),
                            Height = 0,
                            Width = 0,
                            Notes = "EXCEPTION. ERROR READING FILE."
                        });
                    }
                }
            }
        }

        public void Save(string savePath)
        {
            var fullOutputFolderString = Path.GetFullPath("Output");
            Directory.CreateDirectory(fullOutputFolderString);
            using (var fileStream = new StreamWriter(savePath))
            { 
                using (var csv = new CsvWriter(fileStream, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<ImageInfo>();
                    csv.NextRecord();
                    csv.WriteRecords(_images);
                }
            }
            Console.WriteLine();
            Console.WriteLine("Output was saved here:" + fullOutputFolderString);
        }
    }
}
