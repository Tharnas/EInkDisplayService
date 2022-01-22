using EInkService.Helper;
using EInkService.OpenWeatherMap;
using EInkService.Options;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.Threading.Tasks;
using static EInkService.Helper.DrawingHelper;

namespace EInkService.Plugins
{
    public class WeatherPlugin : IPlugin
    {
        private readonly OpenWeatherMapService _openWeatherMapService;
        private readonly IOptions<WeatherOptions> _options;

        public WeatherPlugin(OpenWeatherMapService openWeatherMapService, IOptions<WeatherOptions> options)
        {
            _openWeatherMapService = openWeatherMapService;
            _options = options;
        }

        public async Task DrawAsync(Image image, int width, int height, Theme theme)
        {
            var result = await _openWeatherMapService.GetCurrentWeather(_options.Value.Location);

            var sunrise = result.sys.sunrise;
            var sunset = result.sys.sunset;
            //var rain3h = result.rain.rain.1h;
            //var snow3h = result.snow.1h;

            var windSpeedInKmH = result.wind.speed * 3.6;
            var windDirection = result.wind.deg;

            var weather = result.weather.First();

            var weatherTodayHeight = height / 3;
            var weatherTodayWidth = width;
            using var icon = await _openWeatherMapService.GetIcon(weather.icon);
            image.Mutate(x => x.DrawImage(icon, new Point((weatherTodayWidth / 2) - (icon.Width / 2), 0), 1));
            image.DrawString(weather.description, theme.Subline, theme.PrimaryColor, new Point(weatherTodayWidth / 2, icon.Height), AlignEnum.Center);

            image.DrawString(sunrise.ToString("t"), theme.SmallText, theme.PrimaryColor, new Point(0, 0));
            image.DrawString(sunset.ToString("t"), theme.SmallText, theme.PrimaryColor, new Point(weatherTodayWidth, 0), AlignEnum.End);


            var windSpeedSize = image.DrawString($"{windSpeedInKmH.ToString("0.0")} km/h", theme.SmallText, theme.PrimaryColor, new Point(weatherTodayWidth, weatherTodayHeight), AlignEnum.End, AlignEnum.End);
            image.DrawString($"{windDirection.ToString("0.0")}°", theme.SmallText, theme.PrimaryColor, new Point(weatherTodayWidth, (int)(weatherTodayHeight - windSpeedSize.Height)), AlignEnum.End, AlignEnum.End);
        }

    }
}
