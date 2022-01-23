using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace EInkService.Plugins
{
    public class Theme
    {
        public int Margin { get; set; }

        public Font Headline { get; set; }
        public Font Subline { get; set; }
        public Font RegularText { get; set; }
        public Font ItalicText { get; set; }
        public Font SmallText { get; set; }
        public Font WeatherIconFont { get; set; }
        public Font TemperatureText { get; set; }
        public Font DailyWeatherIconFont { get; internal set; }

        public Color PrimaryColor { get; set; }
        public Color AccentColor { get; set; }
    }
}
