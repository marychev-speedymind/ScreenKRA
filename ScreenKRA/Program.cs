using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ImageMagick;
using System.Security.Cryptography;


namespace ScreenKRA
{

    class Screen
    {

        public string WorkDir;
        public string OutputDir;
        public string TargetImage = "mergedimage.png";

        public void InitWorkDir()
        {
            Console.WriteLine("Set directory to look for `*kra` files:");
            // string path = Path.GetFullPath(Console.ReadLine());
            string path = Path.GetFullPath(@"C:/mmc");
            if (!Directory.Exists(path))
            {
                string errorText = $"[ERROR]: {path} Dir was not found!";
                Console.WriteLine(errorText);
                throw new DirectoryNotFoundException(errorText);
            }

            this.WorkDir = path;
        }
    }

    class Program
    {
        /*
        static void Crop2(MagickImage magickImage, string path, int chunk, int spacing)
        {
            int width = magickImage.Width;
            MagickGeometry geometry = new MagickGeometry();

            geometry.Width = chunk;
            geometry.Height = magickImage.Height;
            geometry.Y = 0;

            byte index = 0;
            while (width > 0)
            {
                geometry.X += (chunk + spacing);
                index += 1;

                CreateScreen2(magickImage, geometry, path, index);
                width -= (chunk + spacing);
            }
        }
        static void CreateScreen2(MagickImage image, MagickGeometry geometry, string path2, byte index)
        {
            var screen = image.Clone(geometry);
            screen.Strip();

            screen.Write(new FileInfo(Path.Combine(@"C:\Temp", Guid.NewGuid().ToString() + ".png")), MagickFormat.Png);
            //Console.WriteLine("\t[+] Created a new screen: {0}", fullPathScreen);
        }
        */

        static void Crop(MagickImage magickImage, string path, int chunk, int spacing)
        {
            // create parent dir to list og images
            string dir = $"{chunk}x{magickImage.Height}";
            string newPath = Path.GetFullPath(path + "/" + dir);
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            Console.WriteLine($"[+] newPath directory: {newPath}");

            int width = magickImage.Width;
            MagickGeometry geometry = new MagickGeometry();

            geometry.Width = chunk;
            geometry.Height = magickImage.Height;
            geometry.Y = 0;

            byte index = 0;
            while (width > 0)
            {
                geometry.X += (chunk + spacing);
                index += 1;

                CreateScreen(magickImage, geometry, newPath, index);
                width -= (chunk + spacing);
            }
        }

        static FileInfo GetFullPathScreen(string path, MagickGeometry g, byte index)
        {
            string fileName = $"{index}.png";
            return new FileInfo($"{path}\\{fileName}");
        }

        static void CreateScreen(MagickImage image, MagickGeometry geometry, string path, byte index)
        {
            var screen = image.Clone(geometry);
            screen.Strip();

            FileInfo fullPathScreen = GetFullPathScreen(path, geometry, index);
            screen.Write(fullPathScreen, MagickFormat.Png);
            Console.WriteLine("\t[+] Created a new screen: {0}", fullPathScreen);
        }

        static void Main(string[] args)
        {

            string TargetImage = "mergedimage.png";

            Console.WriteLine("Set directory to look for `*kra` files:");
            // string workDir = Path.GetFullPath(Console.ReadLine());
            string workDir = Path.GetFullPath(@"C:/mmc");
            if (!Directory.Exists(workDir))
            {
                string errorText = $"[ERROR]: {workDir} Dir was not found!";
                Console.WriteLine(errorText);
                throw new DirectoryNotFoundException(errorText);
            }
            Console.WriteLine($"[+] Work directory: {workDir}");

            Console.WriteLine("Set directory to results:");
            // string outDir = Path.GetFullPath(Console.ReadLine());
            string outDir = Path.GetFullPath(@"C:/KRA");
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);
            Console.WriteLine($"[+] Output directory: {outDir}");

            foreach (string fullPath in Directory.EnumerateFiles(workDir, "*.kra", SearchOption.AllDirectories))
            {
                // prepare and create only folders
                /// [note] For OS: window: replace roor dirs
                string workDirWithoutDisk = workDir.Replace(Path.GetPathRoot(workDir), "");
                string fullWorkPath = Path.GetDirectoryName($"{Path.GetFullPath(fullPath).Replace(workDir, (outDir + '/' + workDirWithoutDisk))}");

                if (!Directory.Exists(fullWorkPath))
                {
                    Directory.CreateDirectory(fullWorkPath);
                    Console.WriteLine("[+] Created a new dir: {0}", fullWorkPath);
                }

                // get the screen needed
                using var file = File.OpenRead(fullPath);
                using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                var entry = zip.Entries.Where(x => x.ToString() == TargetImage).FirstOrDefault();
                using MagickImage image = new MagickImage(entry.Open());

                // rules to crop
                switch (image.Height)
                {
                    case 2208:
                    case 2688:
                        Crop(image, fullWorkPath, 1242, 52);
                        break;
                    case 2732:
                        Crop(image, fullWorkPath, 2732, 255);
                        break;
                    default:
                        Console.WriteLine($"[INFO] The {image.FileName} image doesn't contain the heights needed");
                        break;
                }
            }

            Console.WriteLine("Creating screenshots was finished!");
        }
    }
}
