using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace FluentBitmap
{
    public class FluentBitmap
    {
        private readonly int _width;
        private readonly int _height;
        private int _stride;
        private byte[] _data;
        private PixelFormat _pixelFormat = PixelFormat.Format8bppIndexed;
        private Color[] _palette;
        private ImageFormat _imageFormat = ImageFormat.Jpeg;

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

        public void Save(string filePath)
        {
            saveBitmap(x => x.Save(filePath, _imageFormat));
        }

        public void Save(Stream writeStream)
        {
            saveBitmap(x => x.Save(writeStream, _imageFormat));
        }

        private void saveBitmap(Action<Bitmap> saveAction)
        {
            if (_stride % 4 != 0)
                throw new InvalidOperationException("Stride must be a multiple of 4. By default it is the same as the image width. Use SetStride() to change stride.");

            var data = _data;

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    using (var bitmap = new Bitmap(_width, _height, _stride,
                        _pixelFormat, new IntPtr(ptr)))
                    {
                        if (_palette != null)
                        {
                            var palette = bitmap.Palette;
                            for (int i = 0; i < _palette.Length; i++)
                                palette.Entries[i] = _palette[i];
                            bitmap.Palette = palette;
                        }

                        saveAction(bitmap);
                    }
                }
            }
        }
    }
}
