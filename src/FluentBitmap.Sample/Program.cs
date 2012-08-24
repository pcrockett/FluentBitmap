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
        private static readonly int Stride = FluentBitmap.GetMinimumStride(Width, BytesPerPixel);

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: FluentBitmap.Sample.exe output-image.png");
                return;
            }

            var mandelbrot = getMandelbrot();

            var filePath = args[0];
            new FluentBitmap(Width, Height)
                .SetImageFormat(ImageFormat.Jpeg)
                .SetPixelFormat(PixFormat)
                .SetStride(Stride)
                .SetPixelData(mandelbrot)
                .SetQuality(Quality)
                .Save(filePath);

            Process.Start(filePath);
        }

        private static byte[] getMandelbrot()
        {
            // Translated and tweaked from pseudocode at http://en.wikipedia.org/wiki/Mandelbrot_set#For_programmers

            const double XMax = 1.0 / Zoom;
            const double XMin = -2.5 / Zoom;
            const double YMax = 1.0 / Zoom;
            const double YMin = -1.0 / Zoom;

            var data = new byte[Height * Stride];

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
                    Array.Copy(pixelBytes, 0, data, calculatePixelIndex(x, y), BytesPerPixel);
                }
            }

            return data;
        }

        private static int calculatePixelIndex(int x, int y)
        {
            return (y * Stride) + (x * BytesPerPixel);
        }
    }
}
