FluentBitmap
============

A simplified wrapper for `System.Drawing.Bitmap`.

`System.Drawing.Bitmap` provides a clunky API. Using the Bitmap class, you may write something similar to the following:

    unsafe
    {
        fixed (byte* ptr = _data)
        {
            using (var bitmap = new Bitmap(_width, _height, _width,
                PixelFormat.Format8bppIndexed, new IntPtr(ptr)))
            {
                var palette = bitmap.Palette;
                for (int i = 0; i < byte.MaxValue; i++)
                    palette.Entries[i] = Color.FromArgb(/* Define colors here */)
                bitmap.Palette = palette; // Don't ask me why this is necessary; it just is.

                bitmap.Save(@"C:\foo.jpg", ImageFormat.Jpeg)
            }
        }
    }

With FluentBitmap, it gets a little prettier:

	var palette = Enumerable.Range(0, byte.MaxValue)
		.Select(val => Color.FromArgb(/* Define colors here */))
		.ToArray();

	new FluentBitmap(_width, _height)
		.SetPixelFormat(PixelFormat.Format8bppIndexed)
		.SetPalette(palette)
		.SetPixelData(_data)
		.SetImageFormat(ImageFormat.Jpeg)
		.Save(@"C:\foo.jpg");

Need to save an image with lower quality, perhaps to save disk space? With the .NET Bitmap, you would need to write some code similar to this:

    var encoder = ImageCodecInfo.GetImageEncoders().Single(x => x.MimeType == "image/jpeg");
    using (var parameters = new EncoderParameters(1))
    {
        parameters.Param[0] = new EncoderParameter(Encoder.Quality, 70); // Set quality to 70
    }
    bitmap.Save(filePath, encoder, parameters);
    
With FluentBitmap, all you need to do is:

    .SetQuality(70)

Of course this is a very limited fluent interface. There are many other things you can do with the `System.Drawing.Bitmap` class that you can't do with FluentBitmap. Some day if I need more functionality, I'll add it. Until then, pull requests are welcome.