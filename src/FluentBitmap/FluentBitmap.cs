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

        private readonly int _width;
        private readonly int _height;
        private int _stride;
        private byte[] _data;
        private PixelFormat _pixelFormat = PixelFormat.Format8bppIndexed;
        private Color[] _palette;
        private ImageFormat _imageFormat = ImageFormat.Jpeg;
        private int _quality = 100;

        public FluentBitmap(int pixelWidth, int pixelHeight)
        {
            if (pixelWidth <= 0)
                throw new ArgumentOutOfRangeException("pixelWidth", "pixelWidth must be greater than 0.");
            if (pixelHeight <= 0)
                throw new ArgumentOutOfRangeException("pixelHeight", "pixelHeight must be greater than 0.");
            _width = pixelWidth;
            _height = pixelHeight;
            _stride = _width;
            _data = new byte[_width * _height];
        }

        public static int GetMinimumStride(int imageWidth, int bytesPerPixel)
        {
            return 4 * ((imageWidth * bytesPerPixel + 3) / 4);
        }

        public FluentBitmap SetStride(int value)
        {
            _stride = value;
            return this;
        }

        public FluentBitmap SetPixelFormat(PixelFormat value)
        {
            _pixelFormat = value;
            return this;
        }

        public FluentBitmap SetPalette(Color[] value)
        {
            _palette = value;
            return this;
        }

        public FluentBitmap SetPixelData(byte[] value)
        {
            _data = value;
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
            if (_stride % 4 != 0)
                throw new InvalidOperationException("Stride must be a multiple of 4. By default it is the same as the image width. Use SetStride() to change stride.");

            var bitmap = new Bitmap(_width, _height, _stride, _pixelFormat, new IntPtr(dataPointer));
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
