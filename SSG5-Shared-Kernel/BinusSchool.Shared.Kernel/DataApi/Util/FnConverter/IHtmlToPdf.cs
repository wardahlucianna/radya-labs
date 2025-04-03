using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.HtmlToPdf;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface IHtmlToPdf : IFnConverter
    {
        [Post("/html-to-pdf")]
        Task<HttpResponseMessage> ConvertHtmlToPdf([Body] ConvertHtmlToPdfRequest body);
    }
}
