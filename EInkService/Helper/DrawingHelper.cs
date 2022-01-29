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

            var drawingOptions = new DrawingOptions();
            drawingOptions.GraphicsOptions.Antialias = false;

            switch (alignX)
            {
                case AlignEnum.Beginning:
                    drawingOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case AlignEnum.Center:
                    drawingOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case AlignEnum.End:
                    drawingOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                default:
                    throw new ArgumentException(nameof(alignX));
            }

            switch (alignY)
            {
                case AlignEnum.Beginning:
                    drawingOptions.TextOptions.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case AlignEnum.Center:
                    drawingOptions.TextOptions.VerticalAlignment = VerticalAlignment.Center;
                    break;
                case AlignEnum.End:
                    drawingOptions.TextOptions.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                default:
                    throw new ArgumentException(nameof(alignX));
            }


            image.Mutate(x => x.DrawText(drawingOptions, text, font, color, point));

            return size;
        }
    }
}
