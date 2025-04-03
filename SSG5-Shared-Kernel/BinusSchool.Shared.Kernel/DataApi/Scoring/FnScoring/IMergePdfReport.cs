using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scoring.FnScoring.MergePdfReport;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
   
    public interface IMergePdfReport : IFnScoring
    {    
        [Post("/merge-pdf/Report")]
        Task<HttpResponseMessage> MergeToPDFReport([Body] MergePdfReportRequest body);
    }
}
