using System;
using System.IO;
using ImageMagick;


namespace ScreenKRA
{
    class Program
    {
        static void Main(string[] args)
        {
            Screen screen = new Screen();
            
            screen.InitWorkDir();
            screen.InitOutputDir();

            foreach (string fullPath in Directory.EnumerateFiles(screen.WorkDir, "*.kra", SearchOption.AllDirectories))
            {
                screen.InitAndCreateFullOutputPath(fullPath);
                
                using MagickImage image = screen.GetTagretImage(fullPath);
                switch (image.Height)
                {
                    case 2208:
                    case 2688:
                        screen.Crop(image, Path.GetFileNameWithoutExtension(fullPath), 1242, 52);
                        break;
                    case 2732:
                        screen.Crop(image, Path.GetFileNameWithoutExtension(fullPath), 2732, 255);
                        break;
                    default:
                        screen.CreateNativeScreen(image, Path.GetFileNameWithoutExtension(fullPath));
                        break;
                }
            }

            END();
        }

        static void END()
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Creating screenshots was finished!");
            Console.WriteLine("----------------------------------");
        }
    }
}
