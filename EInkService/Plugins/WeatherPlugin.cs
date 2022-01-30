using EInkService.Helper;
using EInkService.OpenWeatherMap;
using EInkService.Options;
using Microsoft.Extensions.Options;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
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
            var result = await _openWeatherMapService.GetCurrentWeather(_options.Value.Latitude, _options.Value.Longitude);

            using var currentWeatherImage = new Image<Rgba32>(width, height / 3);
            RenderCurrentWeather(currentWeatherImage, theme, result);
            image.Mutate(x => x.DrawImage(currentWeatherImage, new Point(0, 0), 1));


            using var weatherTodayImage = new Image<Rgba32>(width, height / 3);
            RenderTodayWeather(weatherTodayImage, theme, result);
            image.Mutate(x => x.DrawImage(weatherTodayImage, new Point(0, height / 3), 1));


            using var dailyWeatherImage = new Image<Rgba32>(width, height / 3);
            RenderDailyWeather(dailyWeatherImage, theme, result);
            image.Mutate(x => x.DrawImage(dailyWeatherImage, new Point(0, height * 2 / 3), 1));
        }

        private void RenderTodayWeather(Image image, Theme theme, GetOneCallApiResult result)
        {
            var minTemp = result.hourly.Min(x => x.temp);
            var minTextSize = TextMeasurer.Measure($"{minTemp:0.0}°", new RendererOptions(theme.SmallText));
            var maxTemp = result.hourly.Max(x => x.temp);
            var maxTextSize = TextMeasurer.Measure($"{maxTemp:0.0}°", new RendererOptions(theme.SmallText));


            var graphX = Math.Max(minTextSize.Width, maxTextSize.Width) + 3;
            var graphWidth = image.Width - graphX - theme.Margin;
            var graphY = 20;
            var graphHeight = image.Height - graphY - 20;


            image.Mutate(x => x.DrawLines(theme.PrimaryColor, 1, new PointF(graphX, graphY), new PointF(graphX, graphY + graphHeight)));
            if (minTemp < 0 && maxTemp > 0)
            {
                var zeroY = graphY + graphHeight - (graphHeight * (0 - minTemp) / (maxTemp - minTemp));
                image.Mutate(x => x.DrawLines(theme.PrimaryColor, 1, new PointF(graphX, zeroY), new PointF(graphX + graphWidth, zeroY)));
            }
            else
            {
                image.Mutate(x => x.DrawLines(theme.PrimaryColor, 1, new PointF(graphX, graphY + graphHeight), new PointF(graphX + graphWidth, graphY + graphHeight)));
            }

            image.DrawString($"{maxTemp:0.0}°", theme.SmallText, theme.PrimaryColor, new Point((int)graphX, graphY), AlignEnum.End, AlignEnum.Center);
            image.DrawString($"{minTemp:0.0}°", theme.SmallText, theme.PrimaryColor, new Point((int)graphX, graphY + graphHeight), AlignEnum.End, AlignEnum.Center);

            var widthPerHour = graphWidth / result.hourly.Length;

            var points = result.hourly.Select((x, i) => new PointF(graphX + i * widthPerHour, x.temp != minTemp ? graphY + graphHeight - (graphHeight * (x.temp - minTemp) / (maxTemp - minTemp)) : graphY + graphHeight)).ToArray();
            image.Mutate(x => x.DrawLines(theme.PrimaryColor, 2, points));

            //image.Mutate(x => x.DrawLines(theme.PrimaryColor, 2, new PointF(0, 0), new PointF(image.Width, 0), new PointF(image.Width, image.Height), new PointF(0, image.Height), new PointF(0, 0)));
        }

        private static void RenderDailyWeather(Image image, Theme theme, GetOneCallApiResult result)
        {
            var dayWidth = image.Width / result.daily.Length;

            for (int i = 0; i < result.daily.Length; i++)
            {
                var currentDailyTop = image.Height;
                var dailyDaySize = image.DrawString(result.daily[i].dt.ToString("ddd"), theme.RegularText, theme.PrimaryColor, new Point(i * dayWidth + dayWidth / 2, currentDailyTop), AlignEnum.Center, AlignEnum.End);
                currentDailyTop -= (int)dailyDaySize.Height + theme.Margin;

                var dailyIconSize = image.DrawString(GetIconFont(result.daily[i].weather.First().icon), theme.DailyWeatherIconFont, theme.PrimaryColor, new Point(i * dayWidth + dayWidth / 2, currentDailyTop), AlignEnum.Center, AlignEnum.End);
                currentDailyTop -= (int)(dailyIconSize.Height + theme.Margin);

                var dailyMinTemperatureSize = image.DrawString($"{result.daily[i].temp.min:0.0}°", theme.RegularText, theme.PrimaryColor, new Point(i * dayWidth + dayWidth / 2, currentDailyTop), AlignEnum.Center, AlignEnum.End);
                currentDailyTop -= (int)dailyMinTemperatureSize.Height + 2;

                image.DrawString($"{result.daily[i].temp.max:0.0}°", theme.RegularText, theme.PrimaryColor, new Point(i * dayWidth + dayWidth / 2, currentDailyTop), AlignEnum.Center, AlignEnum.End);
            }
        }

        private static void RenderCurrentWeather(Image image, Theme theme, GetOneCallApiResult result)
        {
            var weather = result.current.weather.First();

            var descriptionMeasurement = image.DrawString(weather.description, theme.Subline, theme.PrimaryColor, new Point(image.Width / 5, image.Height), AlignEnum.Center, AlignEnum.End);
            image.DrawString(GetIconFont(weather.icon), theme.WeatherIconFont, theme.PrimaryColor, new Point(image.Width / 5, (int)((image.Height - descriptionMeasurement.Height) / 2)), AlignEnum.Center, AlignEnum.Center);


            var currentX = image.Width;
            var currentY = image.Height;
            FontRectangle lastSize;
            lastSize = image.DrawString($"{result.current.sunrise:t} to {result.current.sunset:t} :Sun", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentY -= (int)lastSize.Height + 2;
            lastSize = image.DrawString($"{result.current.humidity}% :Humidity", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentY -= (int)lastSize.Height + 2;
            lastSize = image.DrawString($"{result.current.wind_speed * 3.6:0.0}km/h: Wind", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentY -= (int)lastSize.Height + 2;
            lastSize = image.DrawString($"{result.daily.First().temp.max:0.#}° / {result.daily.First().temp.min:0.#}°", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentY -= (int)lastSize.Height + 2;
            lastSize = image.DrawString($"{result.current.uvi:0.#}: UV", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentY -= (int)lastSize.Height;

            currentX = image.Width / 2;
            currentY = currentY / 2;
            lastSize = image.DrawString($"{result.current.feels_like:0.0}°", theme.TemperatureText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.Center, AlignEnum.Center);
            currentY += (int)(lastSize.Height / 2) + 2;
            image.DrawString($"({result.current.temp:0.0}°)", theme.SmallText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.Center, AlignEnum.Beginning);
        }

        private static string GetIconFont(string icon)
        {
            return icon switch
            {
                "01d" => "\uea02",
                "01n" => "\uea01",
                "02d" => "\uea03",
                "02n" => "\uea04",
                "03d" => "\uea05",
                "03n" => "\uea06",
                "04d" => "\uea07",
                "04n" => "\uea08",
                "09d" => "\uea09",
                "09n" => "\uea0a",
                "10d" => "\uea0b",
                "10n" => "\uea0c",
                "11d" => "\uea0d",
                "11n" => "\uea0e",
                "1232n" => "\uea0f",
                "13d" => "\uea10",
                "13n" => "\uea11",
                "50d" => "\uea12",
                "50n" => "\uea13",
                _ => icon,
            };
        }
    }
}
