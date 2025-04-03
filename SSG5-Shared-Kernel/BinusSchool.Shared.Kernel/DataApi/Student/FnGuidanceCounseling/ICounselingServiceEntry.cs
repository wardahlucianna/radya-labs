using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using Refit;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface ICounselingServiceEntry : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/counseling-service-entry")]
        Task<ApiErrorResult<IEnumerable<GetCounselingServiceEntryResult>>> GetCounselingServiceEntry(GetCounselingServiceEntryRequest query);

        [Post("/guidance-counseling/counseling-service-entry")]
        Task<ApiErrorResult> AddCounselingServiceEntry([Body] AddCounselingServiceEntryRequest body);

        [Put("/guidance-counseling/counseling-service-entry")]
        Task<ApiErrorResult> UpdateCounselingServiceEntry([Body] UpdateCounselingServiceEntryRequest body);

        [Get("/guidance-counseling/counseling-service-entry/detail/{id}")]
        Task<ApiErrorResult<GetCounselingServiceEntryDetailResult>> GetCounselingServiceEntryDetailResult(string id);

        [Delete("/guidance-counseling/counseling-service-entry")]
        Task<ApiErrorResult> DeleteCounselingServiceEntry([Body] IEnumerable<string> ids);

        [Get("/guidance-counseling/counseling-service-entry/history")]
        Task<ApiErrorResult<IEnumerable<GetCounselingServiceEntryStudentHistoryResult>>> GetCounselingServiceEntryStudentHistory(GetCounselingServiceEntryStudentHistoryRequest query);

        [Post("/guidance-counseling/counseling-service-entry/download-excel")]
        Task<HttpResponseMessage> DownloadSummaryCounselingServiceEntry([Body] DownloadSummaryCounselingServiceEntryRequest body);

        [Get("/guidance-counseling/counseling-service-entry/list-grade")]
        Task<ApiErrorResult<IEnumerable<GetCounselorGradeRequest>>> GetCounselorGrade(GetCounselorGradeResult query);

        [Get("/guidance-counseling/counseling-service-entry/list-level")]
        Task<ApiErrorResult<IEnumerable<GetCounselorLevelResult>>> GetCounselorLevel(GetCounselorLevelRequest query);

        [Get("/guidance-counseling/counseling-service-entry/list-home-room")]
        Task<ApiErrorResult<IEnumerable<GetCounselorHomeRoomResult>>> GetCounselorHomeRoom(GetCounselorHomeRoomRequest query);

        [Get("/guidance-counseling/counseling-service-entry/Level-By-Counsellor")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetCounselorLevelByCounsellor(GetCounselorLevelByCounsellorRequest query);

        [Get("/guidance-counseling/counseling-service-entry/Grade-By-Counsellor")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetCounselorGradeByCounsellor(GetCounselorGradeByCounsellorRequest query);


    }
}
