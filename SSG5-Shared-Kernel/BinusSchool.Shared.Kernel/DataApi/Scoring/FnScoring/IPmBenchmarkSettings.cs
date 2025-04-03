using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.PmBenchmarkSettings;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IPmBenchmarkSettings : IFnScoring
    {
        [Get("/pmbenchmarksettings/get-pmbenchmark-period-settings")]
        Task<ApiErrorResult<GetPmBenchmarkPeriodSettingsResult>> GetPmBenchmarkPeriodSettings(GetPmBenchmarkPeriodSettingsRequest query);

        [Delete("/pmbenchmarksettings/delete-pmbenchmark-period-settings")]
        Task<ApiErrorResult> DeletePmBenchmarkPeriodSettings([Body] DeletePmBenchmarkPeriodSettingsRequest query);

        [Post("/pmbenchmarksettings/save-pmbenchmark-period-settings")]
        Task<ApiErrorResult> SavePmBenchmarkPeriodSettings(SavePmBenchmarkPeriodSettingsRequest query);

        [Get("/pmbenchmarksettings/get-pmbenchmark-mapping-settings")]
        Task<ApiErrorResult<GetPmBenchmarkMappingSettingsResult>> GetPmBenchmarkMappingSettings(GetPmBenchmarkMappingSettingsRequest query);

        [Get("/pmbenchmarksettings/get-list-filter-pmbenchmark-mapping-settings")]
        Task<ApiErrorResult<GetListFilterPmBenchmarkMappingSettingsResult>> GetListFilterPmBenchmarkMappingSettings(GetListFilterPmBenchmarkMappingSettingsRequest query);

        [Delete("/pmbenchmarksettings/delete-pmbenchmark-mapping-settings")]
        Task<ApiErrorResult> DeletePmBenchmarkMappingSettings([Body] DeletePmBenchmarkMappingSettingsRequest query);

        [Post("/pmbenchmarksettings/save-pmbenchmark-mapping-settings")]
        Task<ApiErrorResult> SavePmBenchmarkMappingSettings(SavePmBenchmarkMappingSettingsRequest query);

    }
}
