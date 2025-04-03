using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IClassOfficerSetting : IFnSchedule
    {
        [Get("/schedule/class-officer-setting")]
        Task<ApiErrorResult<IEnumerable<GetClassOfficerSettingResult>>> GetListClassDiaryTypeSetting(GetClassOfficerSettingRequest query);

        [Get("/schedule/class-officer-setting/{id}")]
        Task<ApiErrorResult<GetClassOfficerSettingDetailResult>> GetClassOfficerSettingDetail(string id);

        [Put("/schedule/class-officer-setting")]
        Task<ApiErrorResult> UpdateClassDiaryTypeSetting([Body] UpdateClassOfficerSettingRequest body);

        [Get("/schedule/class-officer-setting-download")]
        Task<HttpResponseMessage> DownloadClassOfficerSetting(GetClassOfficerSettingRequest query);

    }
}
