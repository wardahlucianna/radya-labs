using System;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface ICalendarEventToPdf : IFnConverter
    {
        [Get("/calendar-event-to-pdf")]
        Task<ConvertCalendarEventToPdfResult> ConvertCalendarEventToPdf([Query]ConvertCalendarEventToPdfRequest param);
    }
}
