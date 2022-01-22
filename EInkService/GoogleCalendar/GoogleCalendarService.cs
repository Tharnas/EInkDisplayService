using EInkService.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace EInkService.GoogleCalendar
{
    public class GoogleCalendarService
    {
        private readonly IOptions<GoogleOptions> _options;

        public GoogleCalendarService(IOptions<GoogleOptions> options)
        {
            _options = options;
        }

        public async Task<List<CalendarEvent>> GetCalendarEntries()
        {
            var credential =
                new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(_options.Value.ClientEmail)
                {
                    Scopes = new string[] { CalendarService.Scope.Calendar }
                }.FromPrivateKey(_options.Value.PrivateKey));
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
            });

            //var calendar = await service.Calendars.Get(_options.Value.CalendarId).ExecuteAsync();

            // Define parameters of request.
            EventsResource.ListRequest listRequest = service.Events.List(_options.Value.CalendarId);
            listRequest.TimeMin = DateTime.Now;
            listRequest.ShowDeleted = false;
            listRequest.SingleEvents = true;
            listRequest.MaxResults = 10;
            listRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = await listRequest.ExecuteAsync();
            var result = new List<CalendarEvent>();
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    result.Add(new CalendarEvent
                    {
                        Start = eventItem.Start.DateTime.HasValue ? eventItem.Start.DateTime.Value : DateTime.ParseExact(eventItem.Start.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        End = eventItem.End.DateTime.HasValue ? eventItem.End.DateTime.Value : DateTime.ParseExact(eventItem.End.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddHours(23).AddMinutes(59),
                        Title = eventItem.Summary
                    });
                }
            }

            return result;
        }
    }
}
