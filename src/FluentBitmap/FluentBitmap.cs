using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace FluentBitmap
{
    public class FluentBitmap
    {
        private static readonly Dictionary<ImageFormat, string> _mimeTypes =
            new Dictionary<ImageFormat, string>()
            {
                { ImageFormat.Bmp, "image/bmp" },
                { ImageFormat.Tiff, "image/tiff" },
                { ImageFormat.Gif, "image/gif" },
                { ImageFormat.Jpeg, "image/jpeg" },
                { ImageFormat.Png, "image/png" }
            };

        private static readonly Dictionary<PixelFormat, int> _bitDepths =
            new Dictionary<PixelFormat, int>()
            {
                { PixelFormat.Format16bppRgb555, 16 },
                { PixelFormat.Format16bppRgb565, 16 },
                { PixelFormat.Format24bppRgb, 24 },
                { PixelFormat.Format32bppRgb, 32 },
                { PixelFormat.Format1bppIndexed, 1 },
                { PixelFormat.Format4bppIndexed, 4 },
                { PixelFormat.Format8bppIndexed, 8 },
                { PixelFormat.Format16bppArgb1555, 16 },
                { PixelFormat.Format32bppPArgb, 32 },
                { PixelFormat.Format16bppGrayScale, 16 },
                { PixelFormat.Format48bppRgb, 48 },
                { PixelFormat.Format64bppPArgb, 64 },
                { PixelFormat.Canonical, 32 },
                { PixelFormat.Format32bppArgb, 32 },
                { PixelFormat.Format64bppArgb, 64}
            };

        private byte[] _data;
        private Color[] _palette;
        private ImageFormat _imageFormat = ImageFormat.Jpeg;
        private int _quality = 100;

        public FluentBitmap(int pixelWidth, int pixelHeight, PixelFormat pixelFormat)
        {
            if (pixelWidth <= 0)
                throw new ArgumentOutOfRangeException("pixelWidth", "pixelWidth must be greater than 0.");
            if (pixelHeight <= 0)
                throw new ArgumentOutOfRangeException("pixelHeight", "pixelHeight must be greater than 0.");

            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            PixelFormat = pixelFormat;
            StrideBytes = GetMinimumStride(pixelWidth, pixelFormat);
        }

        public int PixelWidth { get; private set; }

        public int PixelHeight { get; private set; }

        public int StrideBytes { get; private set; }

        public PixelFormat PixelFormat { get; private set; }

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
            if (!_bitDepths.ContainsKey(pixelFormat))
                throw new ArgumentOutOfRangeException("pixelFormat", string.Format("{0} is not supported.", pixelFormat));

            return GetMinimumStride(imagePixelWidth, _bitDepths[pixelFormat]);
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

        public FluentBitmap SetImageFormat(ImageFormat value)
        {
            _imageFormat = value;
            return this;
        }

        public FluentBitmap SetQuality(int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentException("quality must be between 0 and 100.");
            _quality = quality;
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

            var data = _data;

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    using (var bitmap = getBitmap(ptr))
                    {
                        save(bitmap, writeStream);
                    }
                }
            }
        }

        private void saveBitmap(string filePath)
        {
            if (_data == null)
                _data = new byte[StrideBytes * PixelHeight];

            var data = _data;

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    using (var bitmap = getBitmap(ptr))
                    {
                        save(bitmap, filePath);
                    }
                }
            }
        }

        private unsafe Bitmap getBitmap(byte* dataPointer)
        {
            var bitmap = new Bitmap(PixelWidth, PixelHeight, StrideBytes, PixelFormat, new IntPtr(dataPointer));
            setPalette(bitmap);

            return bitmap;
        }

        private void setPalette(Bitmap bitmap)
        {
            if (_palette != null)
            {
                var palette = bitmap.Palette;
                for (int i = 0; i < _palette.Length; i++)
                    palette.Entries[i] = _palette[i];
                bitmap.Palette = palette;
            }
        }

        private void save(Bitmap bitmap, Stream stream)
        {
            var encoder = getEncoder();
            if (encoder == null)
            {
                bitmap.Save(stream, _imageFormat);
                return;
            }

            using (var encoderParams = getEncoderParameters())
            {
                bitmap.Save(stream, encoder, encoderParams);
            }
        }

        private void save(Bitmap bitmap, string filePath)
        {
            var encoder = getEncoder();
            if (encoder == null)
            {
                bitmap.Save(filePath, _imageFormat);
                return;
            }

            using (var encoderParams = getEncoderParameters())
            {
                bitmap.Save(filePath, encoder, encoderParams);
            }
        }

        private ImageCodecInfo getEncoder()
        {
            if (!_mimeTypes.ContainsKey(_imageFormat))
                return null;

            var mimeType = _mimeTypes[_imageFormat];
            var encoders = ImageCodecInfo.GetImageEncoders();
            return encoders.SingleOrDefault(x => x.MimeType == mimeType);
        }

        private unsafe EncoderParameters getEncoderParameters()
        {
            var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, _quality);
            return parameters;
        }
    }
}
