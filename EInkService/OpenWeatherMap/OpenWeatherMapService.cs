using EInkService.Converter;
using EInkService.Options;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EInkService.OpenWeatherMap
{
    public class OpenWeatherMapService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<WeatherOptions> _options;

        public OpenWeatherMapService(IHttpClientFactory httpClientFactory, IOptions<WeatherOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;
        }

        public async Task<GetCurrentWeatherResult> GetCurrentWeather(string cityName)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={_options.Value.ApiKey}&lang=de";

            var message = new HttpRequestMessage(HttpMethod.Get, url);


            var httpClient = _httpClientFactory.CreateClient();
            var responseMessage = await httpClient.SendAsync(message);

            if (responseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await responseMessage.Content.ReadAsStreamAsync();
                //using var reader = new StreamReader(contentStream);
                //var text = reader.ReadToEnd();
                //Console.WriteLine(text);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new UnixTimeConverter());
                var result = await JsonSerializer.DeserializeAsync<GetCurrentWeatherResult>(contentStream, options);
                return result;
            }
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
