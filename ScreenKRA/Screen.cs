using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ImageMagick;


namespace ScreenKRA
{
    public class Screen
    {
        public string WorkDir;
        public string OutputDir;
        public string FullOutputPathToDir;
        public static readonly string TargetImage = "mergedimage.png";

        public void InitAndCreateFullOutputPath(string fullPath)
        {
            /// [note] For OS: window: replace roor dirs
            string workDirWithoutDisk = WorkDir.Replace(Path.GetPathRoot(WorkDir), "");
            string pathToDir = Path.GetDirectoryName($"{Path.GetFullPath(fullPath).Replace(WorkDir, (OutputDir + '/' + workDirWithoutDisk))}");
            
            this.FullOutputPathToDir = pathToDir;
            CreateDir(this.FullOutputPathToDir);
        }

        public MagickImage GetTagretImage(string fullPath)
        {
            using var file = File.OpenRead(fullPath);
            using var zip = new ZipArchive(file, ZipArchiveMode.Read);
            var entry = zip.Entries.Where(x => x.ToString() == TargetImage).FirstOrDefault();
            return new MagickImage(entry.Open());
        }

        public static void CreateDir(string pathToDir)
        {
            if (!Directory.Exists(pathToDir))
            {
                Directory.CreateDirectory(pathToDir);
                Console.WriteLine($"[+] Created the directory: {pathToDir}");
            }
        }

        // public 

        public void CreateNativeScreen(MagickImage image, string fileName)
        {
            MagickGeometry geometry = new MagickGeometry(width: image.Width, height: image.Height);
            CreateScreen(image, geometry, FullOutputPathToDir, fileName);
            Console.WriteLine($"[INFO] The {image.FileName} image doesn't contain the heights needed");
        }

        public void Crop(MagickImage magickImage, string fileName, int chunk, int spacing)
        {
            // create parent dir to list of images
            string dir = $"{fileName}({chunk}x{magickImage.Height})";
            string fullPathToDir = Path.GetFullPath(FullOutputPathToDir + "/" + dir);
            CreateDir(fullPathToDir);

            MagickGeometry geom = new MagickGeometry(width: chunk, height: magickImage.Height);

            short itemWidth = (short)(chunk + spacing);
            int width = magickImage.Width;
            
            byte num = 0;
            while (width > 0)
            {
                CreateScreen(magickImage, geom, fullPathToDir, num.ToString());

                geom.X += itemWidth;
                num += 1;
                width -= itemWidth;
            }

            if (Math.Abs(width) == Math.Abs(spacing))
            {
                Console.WriteLine($"\r\nRemainder: {width} / Image width: {magickImage.Width}\r\n");
                Console.WriteLine("Crop finished successfuly!");
            } else
            {
                Console.WriteLine($"[WARNING] Crop: Troubles with an image's width: {magickImage.Width}!");
            }

        }

        public void InitWorkDir()
        {
            Console.WriteLine("Set directory to look for `*kra` files:");
            string path = Path.GetFullPath(Console.ReadLine());
            if (!Directory.Exists(path))
            {
                string errorText = $"[ERROR] Dir was not found! {path} ";
                Console.WriteLine(errorText);
                throw new DirectoryNotFoundException(errorText);
            }

            this.WorkDir = path;
            Console.WriteLine($"[+] Work directory: {path}");
        }

        public void InitOutputDir()
        {
            Console.WriteLine("Set directory to results:");
            string outDir = Path.GetFullPath(Console.ReadLine());
            CreateDir(outDir);
            this.OutputDir = outDir;
        }

        // private

        private static FileInfo GetFullPathScreen(string pathToDir, string fileName) => new FileInfo($"{pathToDir}\\{fileName}.png");

        private static void CreateScreen(MagickImage image, MagickGeometry geometry, string pathToDir, string fileName)
        {
            var screen = image.Clone(geometry);
            screen.Strip();

            FileInfo fullPathScreen = GetFullPathScreen(pathToDir, fileName);
            screen.Write(fullPathScreen, MagickFormat.Png);
            Console.WriteLine("\t[+] Created a new screen: {0}", fullPathScreen);
        }
    }


}
