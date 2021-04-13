using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ImageMagick;


namespace ScreenKRA
{
    class Program
    {
        static void Crop(MagickImage magickImage, string path, int chunk, int spacing)
        {
            int width = magickImage.Width;
            MagickGeometry geometry = new MagickGeometry();

            geometry.Width = chunk;
            geometry.Height = magickImage.Height;
            geometry.Y = 0;

            while (width > 0)
            {
                geometry.X += (chunk + spacing);
                CreateScreen(magickImage, geometry, path);
                width -= (chunk + spacing);
            }
        }

        static FileInfo GetFullPathScreen(string path, MagickGeometry g)
        {
            string chinkImagePath = $"{Path.GetDirectoryName(path)}";
            string chinkImageName = $"{Path.GetFileNameWithoutExtension(path)}({g.Width} X {g.Height})--[{g.X}, {g.Y}].png";
            return new FileInfo($"{chinkImagePath}\\{chinkImageName}");
        }

        static void CreateScreen(MagickImage image, MagickGeometry geometry, string path)
        {
            var screen = image.Clone(geometry);
            screen.Write(GetFullPathScreen(path, geometry), MagickFormat.Png);
            Console.WriteLine("\t[+] Created a new screen: {0}", GetFullPathScreen(path, geometry));
        }

        static void Main(string[] args)
        {
            string TargetImage = "mergedimage.png";

            Console.WriteLine("Set directory to look for `*kra` files:");
            string workDir = Path.GetFullPath(Console.ReadLine());
            // string workDir = Path.GetFullPath(@"C:/mmc");

            Console.WriteLine("Set directory to results:");
            string outDir = Path.GetFullPath(Console.ReadLine());
            // string outDir = Path.GetFullPath(@"C:/mmc/out");

            if (!Directory.Exists(workDir))
            {
                string errorText = $"[ERROR]: {workDir} Dir was not found!";
                Console.WriteLine(errorText);
                throw new DirectoryNotFoundException(errorText);
            }

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);


            foreach (string fullPath in Directory.EnumerateFiles(workDir, "*.kra", SearchOption.AllDirectories))
            {
                string fullWorkPath = $"{Path.GetFullPath(fullPath).Replace(workDir, outDir)}";
                if (!Directory.Exists(Path.GetDirectoryName(fullWorkPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullWorkPath));
                    Console.WriteLine("[+] Created a new dir: {0}", Path.GetDirectoryName(fullWorkPath));
                }

                using var file = File.OpenRead(fullPath);
                using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                var entry = zip.Entries.Where(x => x.ToString() == TargetImage).FirstOrDefault();
                using MagickImage image = new MagickImage(entry.Open());

                switch (image.Height)
                {
                    case 2208:
                        Crop(image, fullWorkPath, 1242, 52);
                        break;
                    case 2688:
                        Crop(image, fullWorkPath, 1242, 52);
                        break;
                    case 2732:
                        Crop(image, fullWorkPath, 2732, 255);
                        break;
                    default:
                        Crop(image, fullWorkPath, image.Width, 0);
                        break;
                }
            }

            Console.WriteLine("Creating screenshots was finished!");
        }
    }
}
