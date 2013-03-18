using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace FluentBitmap.Sample
{
    class Program
    {
        private const int Width = 1025;
        private const int Height = (int)(Width / 1.75);
        private const int MaxIterations = 1000;
        private const double Zoom = 1.0;
        private const double XOffset = 0.0;
        private const double YOffset = 0.0;
        private const int BytesPerPixel = 3;
        private const PixelFormat PixFormat = PixelFormat.Format24bppRgb;
        private const int Quality = 100;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: FluentBitmap.Sample.exe output-image.png");
                return;
            }

            var filePath = args[0];
            var bitmap = new FluentBitmap(Width, Height, PixFormat)
                .SetImageFormat(ImageFormat.Png)
                .SetQuality(Quality);

            var mandelbrotData = getMandelbrot(bitmap.StrideBytes);

            bitmap.SetPixelData(mandelbrotData)
                .Save(filePath);

            Process.Start(filePath);
        }

        private static byte[] getMandelbrot(int stride)
        {
            // Translated and tweaked from pseudocode at http://en.wikipedia.org/wiki/Mandelbrot_set#For_programmers

            const double XMax = 1.0 / Zoom;
            const double XMin = -2.5 / Zoom;
            const double YMax = 1.0 / Zoom;
            const double YMin = -1.0 / Zoom;

            var data = new byte[Height * stride];

            var xScale = (XMax - XMin) / Width;
            var yScale = (YMax - YMin) / Height;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var scaledX = x * xScale + XMin + XOffset;
                    var scaledY = y * yScale + YMin - YOffset;

                    var a = 0.0;
                    var b = 0.0;

                    uint iteration;
                    for (iteration = 0; a * a + b * b < 4 && iteration < MaxIterations; iteration++)
                    {
                        var aTemp = a * a - b * b + scaledX;
                        b = 2 * a * b + scaledY;
                        a = aTemp;
                    }

                    var pixelBytes = BitConverter.GetBytes(iteration);
                    var imagePixelIndex = calculatePixelIndex(x, y, stride, BytesPerPixel);
                    Array.Copy(pixelBytes, 0, data, imagePixelIndex, BytesPerPixel);
                }
            }

            return data;
        }

        private static int calculatePixelIndex(int x, int y, int stride, int bytesPerPixel)
        {
            return (y * stride) + (x * bytesPerPixel);
        }
    }
}
