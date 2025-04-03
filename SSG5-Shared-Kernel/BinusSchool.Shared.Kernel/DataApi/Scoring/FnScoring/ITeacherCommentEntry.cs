using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ITeacherCommentEntry : IFnScoring
    {
        [Get("/Scoring/TeacherComment/GetStudentForTeacherComment")]
        Task<ApiErrorResult<IEnumerable<GetStudentForTeacherCommentResult>>> GetStudentForTeacherComment(GetStudentForTeacherCommentRequest query);

        [Get("/Scoring/TeacherComment/GetStudentScoreDetail")]
        Task<ApiErrorResult<IEnumerable<GetStudentScoreDetailResult>>> GetStudentScoreDetail(GetStudentScoreDetailRequest query);

        [Get("/Scoring/TeacherComment/GetTeacherComment")]
        Task<ApiErrorResult<IEnumerable<GetTeacherCommentResult>>> GetTeacherComment(GetTeacherCommentRequest query);

        [Post("/Scoring/TeacherComment/AddTeacherComment")]
        Task<ApiErrorResult> AddTeacherComment(List<AddTeacherCommentRequest> query);

        [Get("/Scoring/TeacherComment/GetTeacherCommentWidget")]
        Task<ApiErrorResult<GetTeacherCommentWidgetResult>> GetTeacherCommentWidgetInformation(GetTeacherCommentWidgetRequest query);

        [Get("/Scoring/TeacherComment/GetTeacherCommentPerStudent")]
        Task<ApiErrorResult<IEnumerable<GetTeacherCommentPerStudentResult>>> GetTeacherCommentPerStudent(GetTeacherCommentPerStudentRequest query);

        [Post("/Scoring/TeacherComment/GetTeacherCommentPerStudentExcel")]
        Task<HttpResponseMessage> ExportExcelTeacherCommentPerStudent([Body] ExportExcelTeacherCommentPerStudentRequest body);

        [Get("/Scoring/TeacherComment/GetPeriodEntryTeacherComment")]
        Task<ApiErrorResult<GetPeriodEntryTeacherCommentResult>> GetPeriodEntryTeacherComment(GetPeriodEntryTeacherCommentRequest query);
    }
}
