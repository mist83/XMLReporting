using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;

namespace ConsoleApplication1
{
    public class ConsoleDrawing
    {
        public static void DrawRedditAlien(string textToUse = "REDDIT*", int? width = null)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            var bitmap = (Bitmap)GetImageFromUrl(@"http://upload.wikimedia.org/wikipedia/fr/f/fc/Reddit-alien.png");

            var rectangle = GetAlphaBoundingRect(bitmap);
            var bm = new Bitmap(rectangle.Width, rectangle.Height);

            // Copy the bits from the old image to the new one
            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    var color = bitmap.GetPixel(x + rectangle.Left, y + rectangle.Top);
                    bm.SetPixel(x, y, color);
                }
            }

            bitmap = bm;

            if (!width.HasValue)
                width = Console.BufferWidth;

            double resizeFactor = (double)width / (double)bitmap.Width;
            Size newSize = new Size((int)((double)bitmap.Width * resizeFactor) - 1, (int)((double)bitmap.Width * resizeFactor * .8) - 1);
            bitmap = new Bitmap(bitmap, newSize);

            int characterIndex = 0;

            var colors = new HashSet<Color>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    ConsoleColor? overrideColor = null;

                    // Eyes
                    if (color.ToString() == "Color [A=255, R=255, G=86, B=0]")
                        overrideColor = ConsoleColor.Red;

                    // Outline
                    if (color.A != 0 && color.R == 0 && color.G == 0 && color.B == 0)
                        overrideColor = ConsoleColor.DarkGray;

                    // Track colors
                    if (color.A != 0 && (color.R != 0 || color.G != 0 || color.B != 0))
                        colors.Add(color);

                    // Show the color as white or one of the predefined colors
                    if (overrideColor.HasValue || (color.R != 0 && color.G != 0 && color.B != 0))
                    {
                        ConsoleColor defaultColor = Console.ForegroundColor;

                        if (overrideColor.HasValue)
                            Console.ForegroundColor = overrideColor.Value;

                        Console.Write(textToUse[characterIndex % textToUse.Length]);
                        characterIndex++;
                        Console.ForegroundColor = defaultColor;
                    }
                    else
                        Console.Write(' ');
                }
                Console.WriteLine();
            }
        }

        public static Image GetImageFromUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = httpWebReponse.GetResponseStream())
                {
                    return Image.FromStream(stream);
                }
            }
        }

        private static Rectangle GetAlphaBoundingRect(Bitmap bitmap)
        {
            int left = int.MaxValue;
            int top = int.MaxValue;
            int width = int.MinValue;
            int height = int.MinValue;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.A != 0)
                    {
                        if (x < left)
                            left = x;

                        if (y < top)
                            top = y;

                        if (x > width)
                            width = x;

                        if (y > height)
                            height = y;
                    }
                }
            }

            return new Rectangle(left, top, width - left, height - top);
        }
    }
}
