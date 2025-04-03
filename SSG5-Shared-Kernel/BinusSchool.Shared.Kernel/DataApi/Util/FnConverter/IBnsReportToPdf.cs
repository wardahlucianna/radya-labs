using System;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.BnsReportToPdf;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface IBnsReportToPdf : IFnConverter
    {
        [Post("/bnsReport-to-pdf")]
        Task<ConvertBnsReportToPdfResult> ConvertBnsReportToPdf([Body] ConvertBnsReportToPdfRequest body);
    }
}
