﻿using EInkService.GoogleCalendar;
using EInkService.Helper;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;
using System.Text;
using System.Threading.Tasks;
using static EInkService.Helper.DrawingHelper;

namespace EInkService.Plugins
{
    public class GoogleCalendarPlugin : IPlugin
    {
        private readonly GoogleCalendarService _googleCalendarService;

        public GoogleCalendarPlugin(GoogleCalendarService googleCalendarService)
        {
            _googleCalendarService = googleCalendarService;
        }

        public async Task DrawAsync(Image image, int width, int height, Theme theme)
        {
            var events = await _googleCalendarService.GetCalendarEntries();
            
            image.DrawString(DateTime.Now.ToString("D"), theme.Headline, theme.PrimaryColor, new Point(width / 2, theme.Margin), AlignEnum.Center);

            for (int i = 0; i < events.Count; i++)
            {
                var startPoint = new Point(theme.Margin, (i + 1) * 60);

                if (startPoint.Y >= height)
                {
                    break;
                }

                FontRectangle titleSize;
                if (string.IsNullOrEmpty(events[i].Title))
                {
                    titleSize = image.DrawString("<kein title>", theme.RegularText, theme.PrimaryColor, startPoint);
                }
                else
                {
                    titleSize = image.DrawString(events[i].Title, theme.RegularText, theme.PrimaryColor, startPoint);
                }

                var text = GenerateDateText(events[i]);
                image.DrawString(text, theme.RegularText, theme.PrimaryColor, new Point(startPoint.X, startPoint.Y + (int)titleSize.Height + theme.Margin));
            }
        }

        private static string GenerateDateText(CalendarEvent e)
        {
            var dateText = new StringBuilder();
            DayText(e.Start, dateText);

            dateText.Append(" von ");

            dateText.Append(e.Start.ToString("t"));

            dateText.Append(" bis ");

            if (e.Start.Date != e.End.Date)
            {
                DayText(e.End, dateText);
            }

            dateText.Append(" ");
            dateText.Append(e.End.ToString("t"));

            return dateText.ToString();
        }

        private static void DayText(DateTime dateTime, StringBuilder dateRange)
        {
            if (dateTime.Date == DateTime.Now.Date)
            {
                dateRange.Append("Heute");
            }
            else if (dateTime.Date == DateTime.Now.Date.AddDays(1))
            {
                dateRange.Append("Morgen");
            }
            else
            {
                dateRange.Append(dateTime.Date.ToString("d"));
            }
        }
    }
}
