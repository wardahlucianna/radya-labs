using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using Refit;

namespace BinusSchool.Data.Api.School.FnPeriod
{
    public interface IPeriod : IFnPeriod
    {
        [Get("/school/period")]
        Task<ApiErrorResult<IEnumerable<GetPeriodResult>>> GetPeriods(GetPeriodRequest query);

        [Get("/school/period/{id}")]
        Task<ApiErrorResult<GetPeriodDetailResult>> GetPeriodDetail(string id);

        [Post("/school/period")]
        Task<ApiErrorResult> AddPeriod([Body] AddPeriodRequest body);

        [Put("/school/period")]
        Task<ApiErrorResult> UpdatePeriod([Body] UpdatePeriodRequest body);

        [Delete("/school/period")]
        Task<ApiErrorResult> DeletePeriod([Body] IEnumerable<string> ids);

        [Get("/school/period-semester")]
        Task<ApiErrorResult<IEnumerable<SelectTermResult>>> GetTerm(SelectTermRequest query);

        [Get("/school/period/get-date-with-grade")]
        Task<ApiErrorResult<GetPeriodResult>> GetPeriodDate(SelectTermRequest query);

         [Get("/school/period/current-academicyear")]
        Task<ApiErrorResult<CurrentAcademicYearResult>> GetCurrenctAcademicYear(CurrentAcademicYearRequest query);

        [Get("/school/period/get-date-with-semester")]
        Task<ApiErrorResult<GetDateBySemesterResult>> GetPeriodDateBySemester(GetDateBySemesterRequest query);

        [Get("/school/period/get-period-by-academic-year")]
        Task<ApiErrorResult<List<GetPeriodByAcademicYearResult>>> GetPeriodByAcademicYear(GetPeriodByAcademicYearRequest query);

        [Get("/school/period/current-period")]
        Task<ApiErrorResult<GetCurrentPeriodResult>> GetCurrentPeriod(GetCurrentPeriodRequest query);
    }
}
