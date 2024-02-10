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
            var size = TextMeasurer.MeasureSize(text, new TextOptions(font));
            var textOptions = new RichTextOptions(font);
            textOptions.Origin = point;

            textOptions.HorizontalAlignment = alignX switch
            {
                AlignEnum.Beginning => HorizontalAlignment.Left,
                AlignEnum.Center => HorizontalAlignment.Center,
                AlignEnum.End => HorizontalAlignment.Right,
                _ => throw new ArgumentException(nameof(alignX)),
            };

            textOptions.VerticalAlignment = alignY switch
            {
                AlignEnum.Beginning => VerticalAlignment.Top,
                AlignEnum.Center => VerticalAlignment.Center,
                AlignEnum.End => VerticalAlignment.Bottom,
                _ => throw new ArgumentException(nameof(alignX)),
            };
            var drawingOptions = new DrawingOptions();
            drawingOptions.GraphicsOptions.Antialias = false;

            image.Mutate(x => x.DrawText(drawingOptions, textOptions, text, Brushes.Solid(color), null));

            return size;
        }
    }
}
