using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBitmap.Sample
{
    class Program
    {
        private const int Width = 2048;
        private const int Height = 1170;
        private const int MaxIterations = 100;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: FluentBitmap.Sample.exe output-image.jpg");
                return;
            }

            var data = getMandelbrot();
            new FluentBitmap(Width, Height)
                .SetPixelData(data)
                .Save(args[0]);
        }

        private static byte[] getMandelbrot()
        {
            // Translated from pseudocode at http://en.wikipedia.org/wiki/Mandelbrot_set#For_programmers

            var data = new byte[Height][];

            var xScale = 3.5 / Width;
            var yScale = 2.0 / Height;

            for (int y = 0; y < Height; y++)
            {
                data[y] = new byte[Width];
                for (int x = 0; x < Width; x++)
                {
                    var scaledX = x * xScale - 2.5;
                    var scaledY = y * yScale - 1;

                    var a = 0.0;
                    var b = 0.0;

                    int iteration;
                    for (iteration = 0; a * a + b * b < 4 && iteration < MaxIterations; iteration++)
                    {
                        var aTemp = a * a - b * b + scaledX;
                        b = 2 * a * b + scaledY;
                        a = aTemp;
                    }
                    data[y][x] = (byte)iteration;
                }
            }

            return data.SelectMany(x => x).ToArray();
        }
    }
}
