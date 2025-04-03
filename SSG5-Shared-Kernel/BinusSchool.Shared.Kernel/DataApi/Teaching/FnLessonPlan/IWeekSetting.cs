using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinusSchool.Data.Api.Teaching.FnLessonPlan
{
    public interface IWeekSetting : IFnLessonPlan
    {
        [Get("/week-setting")]
        Task<ApiErrorResult<IEnumerable<GetWeekSettingResponse>>> GetWeekSetting(GetWeekSettingRequest request);
    
        [Get("/week-setting/edit")]
        Task<ApiErrorResult<GetWeekSettingEditResult>> GetWeekSettingEdit(GetWeekSettingEditRequest request);

        [Post("/week-setting")]
        Task<ApiErrorResult> AddWeekSetting([Body] AddWeekSettingRequest body);

        [Put("/week-setting")]
        Task<ApiErrorResult> SetWeekSetting([Body] SetWeekSettingRequest body);

        [Delete("/week-setting")]
        Task<ApiErrorResult> DeleteWeekSetting(DeleteWeekSettingRequest request);
    
        [Put("/week-setting/detail")]
        Task<ApiErrorResult> SaveWeekSettingDetail([Body] SaveWeekSettingDetailRequest body);
    }
}
