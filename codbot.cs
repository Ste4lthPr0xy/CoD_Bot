using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.CvEnum;
using Microsoft.VisualBasic.Devices;
using FiestaAnalytics.src.keyboard;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace CoD_Bot
{
    public class codbot
    {
        static void Main(string[] args)
        {
            Image<Bgr, byte>? marchImage = null;

            List<Image<Bgr, byte>> imagesList = GetAllEmguImagesFromResources(out marchImage);



            Thread botThread = new Thread(() => doMatching(imagesList, marchImage));
            botThread.Start();
        }

        public static void doMatching(List<Emgu.CV.Image<Bgr, byte>> matchimages, Image<Bgr, byte>? marchImage)
        {
            // Bildschirmgröße bestimmenc
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // Pixelgenauigkeit für Übereinstimmung festlegen
            double matchingThreshold = 0.9;

            // Endlosschleife, um nach dem Bild zu suchen und zu klicken
            while (true)
            {
                Thread.Sleep(3000);
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.C)
                {
                    Console.WriteLine("Stopping the Bot");
                    break;
                }
                // Bildschirm als Bitmap aufnehmen
                Bitmap screenCapture = new Bitmap(screenWidth, screenHeight);
                Graphics g = Graphics.FromImage(screenCapture);
                g.CopyFromScreen(0, 0, 0, 0, screenCapture.Size);

                // Bitmap in Emgu CV-Image konvertieren
                Image<Bgr, byte> screenImage = screenCapture.ToImage<Bgr, byte>();
                //Image<Bgr, byte> screenImage = new Image<Bgr, byte>(screenCapture);

                int matches = 0;
                // Durch die Testbilder iterieren und nach Übereinstimmungen suchen
                foreach (Image<Bgr, byte> testImage in matchimages)
                {
                    // Template Matching durchführen
                    using (Image<Gray, float> result = screenImage.MatchTemplate(testImage, TemplateMatchingType.CcoeffNormed))
                    {
                        // Position der besten Übereinstimmung finden
                        double[] minVal = new double[1];
                        double[] maxVal = new double[1];
                        Point[] minLoc = new Point[1];
                        Point[] maxLoc = new Point[1];
                        result.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);

                        // Wenn eine Übereinstimmung gefunden wurde, klicken wir auf die Mitte des gefundenen Bereichs
                        if (maxVal[0] >= matchingThreshold)
                        {
                            matches++;
                            int clickX = maxLoc[0].X + testImage.Width / 2;
                            int clickY = maxLoc[0].Y + testImage.Height / 2;

                            MouseOperations.SetCursorPosition(clickX, clickY);
                            MouseOperations.SetCursorPosition(clickX, clickY);
                            Thread.Sleep(100);
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
                            Thread.Sleep(100);
                            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);

                            if (testImage == marchImage)
                            {
                                Thread.Sleep(500);
                                SendKeys.SendWait(" ");
                            }
                        }
                    }

                }
                if (matches == 0)
                {
                    SendKeys.SendWait(" ");
                }
            }
        }
        public static List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>> GetAllEmguImagesFromResources(out Image<Bgr, byte>? marchImage)
        {
            marchImage = null;
            List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>> imagesList = new List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resourceName in resourceNames)
            {
                if (resourceName.StartsWith("CoD_Bot.Images.Windowed.") && IsImage(resourceName))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        Bitmap bitmap = new Bitmap(stream);
                        Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> emguImage = bitmap.ToImage<Bgr, byte>();
                        imagesList.Add(emguImage);
                        if (resourceName.Contains("March"))
                        {
                            marchImage = emguImage;
                        }
                    }
                }
            }

            return imagesList;
        }

        private static bool IsImage(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            return extension.Equals(".bmp", StringComparison.InvariantCultureIgnoreCase)
                || extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                || extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                || extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase)
                || extension.Equals(".gif", StringComparison.InvariantCultureIgnoreCase);
        }

        public static class MouseOperations
        {
            // Importieren von externen Windows-APIs für Mausbewegungen und -klicks
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern bool SetCursorPos(int x, int y);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

            // Flags für Mausbewegungen und -klicks
            [Flags]
            public enum MouseEventFlags : uint
            {
                Absolute = 0x8000,
                HWheel = 0x01000,
                Move = 0x0001,
                MoveNoCoalesce = 0x2000,
                LeftDown = 0x0002,
                LeftUp = 0x0004,
                RightDown = 0x0008,
                RightUp = 0x0010,
                MiddleDown = 0x0020,
                MiddleUp = 0x0040,
                XDown = 0x0080,
                XUp = 0x0100,
                Wheel = 0x0800,
                VirtualDesk = 0x4000,
            }


            // Methode zum Bewegen des Mauszeigers an eine bestimmte Position
            public static void SetCursorPosition(int x, int y)
            {
                SetCursorPos(x, y);
            }

            // Methode zum Simulieren eines Mausklicks
            public static void MouseEvent(MouseEventFlags flags)
            {
                mouse_event((uint)flags, 0, 0, 0, 0);
            }
        }
    }
}
