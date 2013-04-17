using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

namespace FluentBitmap.Wic
{
    public class FluentBitmap
    {
        private byte[] _data;
        private Color[] _palette;
        private BitmapEncoder _encoder = new JpegBitmapEncoder();

        public FluentBitmap(int pixelWidth, int pixelHeight, PixelFormat pixelFormat)
        {
            if (pixelFormat == null)
                throw new ArgumentNullException("pixelFormat", "pixelFormat is null.");
            if (pixelWidth <= 0)
                throw new ArgumentOutOfRangeException("pixelWidth", "pixelWidth must be greater than 0.");
            if (pixelHeight <= 0)
                throw new ArgumentOutOfRangeException("pixelHeight", "pixelHeight must be greater than 0.");

            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            PixelFormat = pixelFormat;
            StrideBytes = GetMinimumStride(pixelWidth, pixelFormat);
            PixelsPerInch = 96;
        }

        public int PixelWidth { get; private set; }

        public int PixelHeight { get; private set; }

        public int StrideBytes { get; private set; }

        public PixelFormat PixelFormat { get; private set; }

        public int PixelsPerInch { get; private set; }

        public static int GetMinimumStride(int imagePixelWidth, int bitsPerPixel)
        {
            const int strideIntervalBits = 32;

            var maxStrideBitsPossible = imagePixelWidth * bitsPerPixel + (strideIntervalBits - 1);
            var numIntervals = maxStrideBitsPossible / strideIntervalBits;
            var totalStrideBits = strideIntervalBits * numIntervals;

            return totalStrideBits / 8;
        }

        public static int GetMinimumStride(int imagePixelWidth, PixelFormat pixelFormat)
        {
            return GetMinimumStride(imagePixelWidth, pixelFormat.BitsPerPixel);
        }

        public FluentBitmap SetPalette(Color[] value)
        {
            _palette = value;
            return this;
        }

        public FluentBitmap SetPixelData(byte[] imageBytes)
        {
            if (imageBytes != null && imageBytes.Length != StrideBytes * PixelHeight)
                throw new ArgumentException("imageBytes.Length must be equal to StrideBytes * PixelHeight", "imageBytes");

            _data = imageBytes;
            return this;
        }

        public FluentBitmap SetEncoder(BitmapEncoder encoder)
        {
            if (encoder == null)
                throw new ArgumentNullException("encoder", "encoder is null.");

            _encoder = encoder;
            return this;
        }

        public FluentBitmap SetPixelsPerInch(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException("value must be greater than 0.");

            PixelsPerInch = value;
            return this;
        }

        public void Save(string filePath)
        {
            saveBitmap(filePath);
        }

        public void Save(Stream writeStream)
        {
            saveBitmap(writeStream);
        }

        private void saveBitmap(Stream writeStream)
        {
            if (_data == null)
                _data = new byte[StrideBytes * PixelHeight];

            var palette = getPalette();
            var image = BitmapSource.Create(PixelWidth, PixelHeight,
                PixelsPerInch, PixelsPerInch, PixelFormat, palette, _data, StrideBytes);

            _encoder.Frames.Clear();
            _encoder.Frames.Add(BitmapFrame.Create(image));
            _encoder.Save(writeStream);
        }

        private void saveBitmap(string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                saveBitmap(stream);
                stream.Flush();
            }
        }

        private BitmapPalette getPalette()
        {
            if (_palette == null || _palette.Length == 0)
                return null;
            else
                return new BitmapPalette(_palette);
        }
    }
}
