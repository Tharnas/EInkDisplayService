using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;

namespace EInkService.Helper
{
    public static partial class DrawingHelper
    {
        public static FontRectangle DrawString(this Image image, string text, Font font, Color color, Point point, AlignEnum alignX = AlignEnum.Beginning, AlignEnum alignY = AlignEnum.Beginning)
        {
            var size = TextMeasurer.Measure(text, new RendererOptions(font));

            switch (alignX)
            {
                case AlignEnum.Beginning:
                    break;
                case AlignEnum.Center:
                    point.X -= (int)(size.Width / 2);
                    break;
                case AlignEnum.End:
                    point.X -= (int)size.Width;
                    break;
                default:
                    throw new ArgumentException(nameof(alignX));
            }

            switch (alignY)
            {
                case AlignEnum.Beginning:
                    break;
                case AlignEnum.Center:
                    point.Y -= (int)(size.Height / 2);
                    break;
                case AlignEnum.End:
                    point.Y -= (int)size.Height;
                    break;
                default:
                    throw new ArgumentException(nameof(alignX));
            }

            image.Mutate(x => x.DrawText(text, font, color, point));

            return size;
        }
    }
}
