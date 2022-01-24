﻿using EInkService.Helper;
using EInkService.OpenWeatherMap;
using EInkService.Options;
using Microsoft.Extensions.Options;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
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


            using var dailyWeatherImage = new Image<Rgba32>(width, height / 3);
            RenderDailyWeather(dailyWeatherImage, theme, result);
            image.Mutate(x => x.DrawImage(dailyWeatherImage, new Point(0, height * 2 / 3), 1));
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

            var temperatureText = $"{result.current.temp:0.0}°";
            var temperatureMesurment = TextMeasurer.Measure(temperatureText, new RendererOptions(theme.TemperatureText));
            var iconMesurment = TextMeasurer.Measure(GetIconFont(weather.icon), new RendererOptions(theme.WeatherIconFont));

            var iconAndTemperatureWidth = temperatureMesurment.Width + iconMesurment.Width + theme.Margin;
            var iconAndTemperatureHeight = Math.Max(temperatureMesurment.Height, iconMesurment.Height);

            image.DrawString(temperatureText, theme.TemperatureText, theme.PrimaryColor, new Point((int)((image.Width / 2) - (iconAndTemperatureWidth / 2)), theme.Margin));
            image.DrawString($"{result.current.feels_like:0.0}°", theme.Subline, theme.PrimaryColor, new Point((int)((image.Width / 2) - (iconAndTemperatureWidth / 2) + temperatureMesurment.Width), (int)iconAndTemperatureHeight), AlignEnum.End, AlignEnum.End);
            image.DrawString(GetIconFont(weather.icon), theme.WeatherIconFont, theme.PrimaryColor, new Point((int)((image.Width / 2) - (iconAndTemperatureWidth / 2) + temperatureMesurment.Width + theme.Margin), 0));
            image.DrawString(weather.description, theme.Subline, theme.PrimaryColor, new Point(image.Width / 2, (int)iconAndTemperatureHeight + theme.Margin), AlignEnum.Center);

            image.DrawString(result.current.sunrise.ToString("t"), theme.SmallText, theme.PrimaryColor, new Point(theme.Margin, theme.Margin));
            image.DrawString(result.current.sunset.ToString("t"), theme.SmallText, theme.PrimaryColor, new Point(image.Width, theme.Margin), AlignEnum.End);

            var windSpeedInKmH = result.current.wind_speed * 3.6;
            var windDirection = result.current.wind_deg;
            var windSpeedSize = image.DrawString($"{windSpeedInKmH.ToString("0.0")} km/h", theme.SmallText, theme.PrimaryColor, new Point(image.Width, image.Height), AlignEnum.End, AlignEnum.End);
            image.DrawString($"{windDirection.ToString("0.0")}°", theme.SmallText, theme.PrimaryColor, new Point(image.Width, (int)(image.Height - windSpeedSize.Height)), AlignEnum.End, AlignEnum.End);
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
