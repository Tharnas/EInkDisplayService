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

        private static void RenderTodayWeather(Image image, Theme theme, GetOneCallApiResult result)
        {
            // min / max temperature
            var minTemp = result.hourly.Min(x => x.temp);
            var maxTemp = result.hourly.Max(x => x.temp);

            // y labels
            var minTemperatureToRender = (int)Math.Ceiling(minTemp);
            var maxTemperatureToRender = (int)Math.Floor(maxTemp);

            var temperaturesToRender = Enumerable.Range(0, 5).Select((i, count) => maxTemperatureToRender - (i * (maxTemperatureToRender - minTemperatureToRender) / 4));

            // position and dimensions of graph
            var graphX = 3 + temperaturesToRender
               .Select(x => TextMeasurer.MeasureSize($"{x}°", new TextOptions(theme.RegularText)).Width)
               .Max();
            var graphWidth = image.Width - graphX - theme.Margin;
            var graphY = 20;
            var graphHeight = image.Height - graphY - 20;

            // width of one hour in pixel
            var widthPerHour = graphWidth / result.hourly.Length;

            // y axis
            //image.Mutate(x => x.DrawLine(theme.AccentColor, 2, new PointF(graphX, graphY), new PointF(graphX, graphY + graphHeight)));

            // y axis label
            foreach (var temperatureToRender in temperaturesToRender)
            {
                var y = (int)(temperatureToRender != minTemp ? graphY + graphHeight - (graphHeight * (temperatureToRender - minTemp) / (maxTemp - minTemp)) : graphY + graphHeight);
                image.DrawString($"{temperatureToRender}°", theme.RegularText, theme.PrimaryColor, new Point((int)graphX, (int)y), AlignEnum.End, AlignEnum.Center);
                //image.Mutate(x => x.DrawLine(theme.AccentColor, 2, new PointF(graphX - 3, y), new PointF(graphX + 3, y)));
                image.Mutate(x => x.DrawLine(theme.AccentColor, 1, new Point((int)graphX - 3, y), new Point((int)(graphX + graphWidth), y)));
            }

            // x axis
            //image.Mutate(x => x.DrawLine(theme.AccentColor, 2, new PointF(graphX, graphY + graphHeight), new PointF(graphX + graphWidth, graphY + graphHeight)));

            // x axis label
            for (int i = 0; i < result.hourly.Length; i++)
            {
                if (result.hourly[i].dt.Hour % 6 == 0)
                {
                    var x = (int)(i * widthPerHour + graphX);
                    image.Mutate(context => context.DrawLine(theme.AccentColor, 1, new Point(x, graphY), new Point(x, graphY + graphHeight + 3)));
                    //image.Mutate(context => context.DrawLine(theme.AccentColor, 2, new[] { new PointF(x, graphY + graphHeight - 3), new PointF(x, graphY + graphHeight + 3) }));
                    image.DrawString(result.hourly[i].dt.ToString("HH"), theme.RegularText, theme.PrimaryColor, new Point((int)x, graphY + graphHeight - 2), AlignEnum.Center, AlignEnum.Beginning);
                }
            }


            // temperature curve
            var points = result.hourly.Select((x, i) => new PointF(graphX + i * widthPerHour, x.temp != minTemp ? graphY + graphHeight - (graphHeight * (x.temp - minTemp) / (maxTemp - minTemp)) : graphY + graphHeight)).ToArray();
            image.Mutate(x => x.DrawLine(theme.PrimaryColor, 4, points));
        }

        private static void RenderDailyWeather(Image image, Theme theme, GetOneCallApiResult result)
        {
            var daysToDisplay = Math.Min(3, result.daily.Length);
            var dayWidth = image.Width / daysToDisplay;

            for (int i = 0; i < daysToDisplay; i++)
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

            var descriptionMeasurement = image.DrawString(weather.description, theme.RegularText, theme.PrimaryColor, new Point(image.Width / 5, image.Height - 5), AlignEnum.Center, AlignEnum.End);
            image.DrawString(GetIconFont(weather.icon), theme.WeatherIconFont, theme.PrimaryColor, new Point(image.Width / 5, (int)((image.Height - descriptionMeasurement.Height) / 2)), AlignEnum.Center, AlignEnum.Center);


            var currentX = image.Width;
            var currentY = image.Height;
            FontRectangle lastSize;

            lastSize = image.DrawString(" :S", theme.RegularText, theme.AccentColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentX -= (int)lastSize.Width;
            lastSize = image.DrawString(result.current.sunset.ToString("t"), theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentX -= (int)lastSize.Width;
            lastSize = image.DrawString(" to ", theme.RegularText, theme.AccentColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentX -= (int)lastSize.Width;
            lastSize = image.DrawString(result.current.sunrise.ToString("t"), theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);

            currentX = image.Width;
            currentY -= (int)lastSize.Height + 2;
            lastSize = image.DrawString(" :H", theme.RegularText, theme.AccentColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentX -= (int)lastSize.Width;
            lastSize = image.DrawString("%", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentX -= (int)lastSize.Width;
            lastSize = image.DrawString(result.current.humidity.ToString(), theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);

            currentX = image.Width;
            currentY -= (int)lastSize.Height + 2;
            lastSize = image.DrawString(" :W", theme.RegularText, theme.AccentColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentX -= (int)lastSize.Width;
            lastSize = image.DrawString($"{result.current.wind_speed * 3.6:0.0}km/h", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);

            currentX = image.Width;
            currentY -= (int)lastSize.Height + 2;
            lastSize = image.DrawString($"{result.daily.First().temp.max:0.#}° / {result.daily.First().temp.min:0.#}°", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);

            currentY -= (int)lastSize.Height + 2;
            lastSize = image.DrawString(" :UV", theme.RegularText, theme.AccentColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentX -= (int)lastSize.Width;
            lastSize = image.DrawString(result.current.uvi.ToString("0.#"), theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.End, AlignEnum.End);
            currentY -= (int)lastSize.Height;

            currentX = image.Width / 2;
            currentY = 0;
            lastSize = image.DrawString($"{result.current.feels_like:0.0}°", theme.TemperatureText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.Center, AlignEnum.Beginning);
            currentY += (int)lastSize.Height + 2;
            image.DrawString($"({result.current.temp:0.0}°)", theme.RegularText, theme.PrimaryColor, new Point(currentX, currentY), AlignEnum.Center, AlignEnum.Beginning);
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
