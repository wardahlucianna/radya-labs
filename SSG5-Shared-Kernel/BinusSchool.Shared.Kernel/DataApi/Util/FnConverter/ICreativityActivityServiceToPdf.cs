using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf;
using BinusSchool.Data.Model.Util.FnConverter.CreativityActivityService;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface ICreativityActivityServiceToPdf : IFnConverter
    {
        [Get("/creativity-activity-service-to-pdf/timeline")]
        Task<ConvertExperienceToPdfResult> TimelineToPdf([Query] TimelineToPdfRequest param);

        [Get("/creativity-activity-service-to-pdf/experience")]
        Task<IEnumerable<ConvertExperienceToPdfResult>> ExperienceToPdf([Query] ExperienceToPdfRequest param);
    }
}
