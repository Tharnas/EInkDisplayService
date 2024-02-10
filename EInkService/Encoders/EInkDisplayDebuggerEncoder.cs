using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EInkService.Encoders
{
    public class EInkDisplayDebuggerEncoder : IImageEncoder
    {
        public bool SkipMetadata { get; init; }

        public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            var (black, red) = GetBitArrays(image);

            Image<Rgba32> newImage = new Image<Rgba32>(image.Width, image.Height);
            for (int i = 0; i < black.Length; i++)
            {
                int x = i % image.Width;
                x = (x / 8 * 8) + 7 - (x % 8);
                int y = i / image.Width;
                if (red[i])
                {
                    newImage[x, y] = Rgba32.ParseHex("f00");
                }
                else if (!black[i])
                {
                    newImage[x, y] = Rgba32.ParseHex("000");
                }
                else
                {
                    newImage[x, y] = Rgba32.ParseHex("fff");
                }
            }

            newImage.Save(stream, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder());
        }

        public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            throw new NotImplementedException();
        }

        private static (BitArray black, BitArray red) GetBitArrays<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            var rootFrame = image.Frames.RootFrame;

            var black = new BitArray(image.Width * image.Height);
            var red = new BitArray(image.Width * image.Height);
            for (int y = 0; y < rootFrame.Height; y++)
            {
                for (int x = 0; x < rootFrame.Width; x++)
                {
                    var pixel = rootFrame[x, y];

                    Rgba32 color = default;
                    pixel.ToRgba32(ref color);
                    var position = 8 * ((y * image.Width + x) / 8) + 7 - (y * image.Width + x) % 8;
                    black.Set(position, !(color.R == 0 && color.G == 0 && color.B == 0));
                    red.Set(position, color.R == 255 && color.G == 0 && color.B == 0);
                    //black.Set(position, !(color.R < 10 && color.G < 10 && color.B < 10));
                    //red.Set(position, color.R > 245 && color.G < 10 && color.B < 10);
                }
            }

            return (black, red);
        }
    }
}
