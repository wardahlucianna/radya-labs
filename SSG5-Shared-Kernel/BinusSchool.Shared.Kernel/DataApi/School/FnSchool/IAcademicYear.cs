using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IAcademicYear : IFnSchool
    {
        [Get("/school/academicyears")]
        Task<ApiErrorResult<IEnumerable<GetListAnswerSetResult>>> GetAcademicyears(CollectionSchoolRequest query);

        [Get("/school/academicyears/{id}")]
        Task<ApiErrorResult<DetailResult2>> GetAcademicyearDetail(string id);

        [Post("/school/academicyears")]
        Task<ApiErrorResult> AddAcademicyear([Body] AddAcademicYearRequest body);

        [Put("/school/academicyears")]
        Task<ApiErrorResult> UpdateAcademicyear([Body] UpdateAcademicYearRequest body);

        [Delete("/school/academicyears")]
        Task<ApiErrorResult> DeleteAcademicyear([Body] IEnumerable<string> ids);

        [Get("/school/academicyears-by-range")]
        Task<ApiErrorResult<IEnumerable<GetAcadyearByRangeResult>>> GetAcademicyearsByRange(GetAcadyearByRangeRequest query);

        [Get("/school/academicyears-range/{id}")]
        Task<ApiErrorResult<GetAcadyearByRangeResult>> GetAcademicyearRange([AliasAs("id")] string idAcadyear);
    }
}
