using System;

namespace BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf
{
    public class ConvertCalendarEventToPdfResult
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Uri Location { get; set; }
    }
}
