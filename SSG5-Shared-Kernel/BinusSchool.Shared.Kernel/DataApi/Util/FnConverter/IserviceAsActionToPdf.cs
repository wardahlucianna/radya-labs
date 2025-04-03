using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.ServiceAsActionToPdf;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface IServiceAsActionToPdf : IFnConverter
    {
        [Post("/service-as-action-to-pdf")]
        Task<HttpResponseMessage> ConvertServiceAsActionToPdf([Body] ConvertServiceAsActionToPdfRequest body);
    }
}
