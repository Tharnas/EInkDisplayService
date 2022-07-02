using System;

namespace EInkService.GoogleCalendar
{
    public class CalendarEvent
    {
        public bool IsAllDay { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Title { get; set; }
    }
}
