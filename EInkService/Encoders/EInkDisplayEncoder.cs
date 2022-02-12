using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EInkService.Encoders
{
    public class EInkDisplayEncoder : IImageEncoder
    {
        public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            var (black, red) = GetBitArrays(image);

            var buffer = new byte[image.Width * image.Height / 8];
            black.CopyTo(buffer, 0);
            stream.Write(buffer, 0, buffer.Length);
            red.CopyTo(buffer, 0);
            stream.Write(buffer, 0, buffer.Length);
        }

        public async Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            var (black, red) = GetBitArrays(image);

            var buffer = new byte[image.Width * image.Height / 8];
            black.CopyTo(buffer, 0);
            await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            red.CopyTo(buffer, 0);
            await stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
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
                    black.Set(position, !(color.R < 10 && color.G < 10 && color.B < 10));
                    red.Set(position, color.R > 245 && color.G < 10 && color.B < 10);
                }
            }

            return (black, red);
        }
    }
}
