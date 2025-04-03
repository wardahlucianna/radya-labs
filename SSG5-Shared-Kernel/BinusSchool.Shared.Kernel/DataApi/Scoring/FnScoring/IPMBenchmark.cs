using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.EntryScore;
using BinusSchool.Data.Model.Scoring.FnScoring.FormativeScoreEntry;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scoring.FnScoring.PMBenchmark.PMBenchmarkEntry;
using BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport;
using BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IPMBenchmark : IFnScoring
    {
        
        [Get("/pmbenchmark/pmbenchmark-entry")]
        Task<ApiErrorResult<GetPMBenchmarkEntryResult>> GetPMBenchmarkEntry(GetPMBenchmarkEntryRequest query);

        [Get("/pmbenchmark/pmbenchmark-privilage-entry")]
        Task<ApiErrorResult<IEnumerable<GetPMBenchmarkPrivilageEntryResult>>> GetPMBenchmarkPrivilageEntry(GetPMBenchmarkPrivilageEntryRequest query);

        [Put("/pmbenchmark/update-pmbenchmark-entry")]
        Task<ApiErrorResult> UpdatePMBenchmarkEntry([Body] UpdatePMBenchmarkEntryRequest query);

        [Delete("/pmbenchmark/delete-pmbenchmark-entry")]
        Task<ApiErrorResult> DeletePMBenchmarkEntry([Body] DeletePMBenchmarkEntryRequest query);



    }
}
