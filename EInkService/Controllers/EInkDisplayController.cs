using EInkService.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace EInkService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EInkDisplayController : ControllerBase
    {
        private readonly ILogger<EInkDisplayController> _logger;
        private readonly GoogleCalendarPlugin _googleCalendarPlugin;
        private readonly WeatherPlugin _weatherPlugin;

        public EInkDisplayController(ILogger<EInkDisplayController> logger, GoogleCalendarPlugin googleCalendarPlugin, WeatherPlugin weatherPlugin)
        {
            _logger = logger;
            _googleCalendarPlugin = googleCalendarPlugin;
            _weatherPlugin = weatherPlugin;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            int width = 800;
            int height = 480;

            using Image image = new Image<Rgba32>(width, height);

            var fontCollection = new FontCollection();
            fontCollection.Install("./Fonts/Plantin.ttf");
            fontCollection.Install("./Fonts/WeatherIcons.ttf");
            var fontFamily = fontCollection.Find("Plantin");
            var iconFont = fontCollection.Find("WeatherIcons");

            var theme = new Theme
            {
                Margin = 5,
                Headline = fontFamily.CreateFont(18, FontStyle.Regular),
                Subline = fontFamily.CreateFont(16, FontStyle.Italic),
                RegularText = fontFamily.CreateFont(16, FontStyle.Regular),
                ItalicText = fontFamily.CreateFont(16, FontStyle.Italic),
                SmallText = fontFamily.CreateFont(12, FontStyle.Regular),
                TemperatureText = fontFamily.CreateFont(30, FontStyle.Bold),
                WeatherIconFont = iconFont.CreateFont(60, FontStyle.Regular),
                DailyWeatherIconFont = iconFont.CreateFont(30, FontStyle.Regular),

                PrimaryColor = Color.Black,
                AccentColor = Color.Red,
            };

            // weather
            int weatherWidth = (width - theme.Margin) / 2;
            int weatherHeigth = height;
            int weatherX = 0;
            int weatherY = 0;
            using var weatherImage = new Image<Rgba32>(weatherWidth, weatherHeigth);

            await _weatherPlugin.DrawAsync(weatherImage, weatherWidth, weatherHeigth, theme);

            image.Mutate(x => x.DrawImage(weatherImage, new Point(weatherX, weatherY), 1));


            // draw vertical line
            //graphics.DrawLine(pen, new Point(width / 2, 0), new Point(width / 2, height));


            // calendar
            int calendarWidth = (width - theme.Margin) / 2;
            int calendarHeight = height;
            int calendarX = (width + theme.Margin) / 2;
            int calendarY = 0;
            using var calendarImage = new Image<Rgba32>(calendarWidth, calendarHeight);

            await _googleCalendarPlugin.DrawAsync(calendarImage, calendarWidth, calendarHeight, theme);

            image.Mutate(x => x.DrawImage(calendarImage, new Point(calendarX, calendarY), 1));

            MemoryStream ms = new MemoryStream();
            image.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "image/png");
        }
    }
}
