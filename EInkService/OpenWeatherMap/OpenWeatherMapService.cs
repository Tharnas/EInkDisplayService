using EInkService.Converter;
using EInkService.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EInkService.OpenWeatherMap
{
    public class OpenWeatherMapService
    {
        private readonly ILogger<OpenWeatherMapService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<WeatherOptions> _options;

        public OpenWeatherMapService(ILogger<OpenWeatherMapService> logger, IHttpClientFactory httpClientFactory, IOptions<WeatherOptions> options)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _options = options;
        }

        public async Task<GetOneCallApiResult> GetCurrentWeather(double lat, double lon)
        {
            _logger.LogInformation ($"Trying to get weather information from lat: {lat} and lon: {lon}");

            var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&appid={_options.Value.ApiKey}&units=metric&lang=de";

            var message = new HttpRequestMessage(HttpMethod.Get, url);


            var httpClient = _httpClientFactory.CreateClient();
            var responseMessage = await httpClient.SendAsync(message);

            if (responseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await responseMessage.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions();
                options.Converters.Add(new UnixTimeConverter());
                var result = await JsonSerializer.DeserializeAsync<GetOneCallApiResult>(contentStream, options);
                return result;
            }

            using var errorContentStream = await responseMessage.Content.ReadAsStreamAsync();
            var reader = new StreamReader(errorContentStream);
            _logger.LogError(reader.ReadToEnd());

            return null;
        }

        public async Task<Image> GetIcon(string iconName)
        {
            var url = $"https://openweathermap.org/img/w/{iconName}.png";

            var message = new HttpRequestMessage(HttpMethod.Get, url);

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                using var content = await response.Content.ReadAsStreamAsync();
                return Image.Load(content);
            }

            return null;
        }
    }
}
