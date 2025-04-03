using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using Refit;

namespace BinusSchool.Data.Api.School.FnSubject
{
    public interface ISubject : IFnSubject
    {
        [Get("/school/subject")]
        Task<ApiErrorResult<IEnumerable<GetSubjectResult>>> GetSubjects(GetSubjectRequest query);

        [Get("/school/subject/{id}")]
        Task<ApiErrorResult<GetSubjectDetailResult>> GetSubjectDetail(string id);

        [Post("/school/subject-by-code-school")]
        Task<ApiErrorResult<List<GetSubjectPathwayForAscTimetableResult>>> GetSubjectByCodeAndSchool([Body] GetSubjectPathwayForAscTimetableRequest request);

        [Post("/school/subject")]
        Task<ApiErrorResult> AddSubject([Body] AddSubjectRequest body);

        [Put("/school/subject")]
        Task<ApiErrorResult> UpdateSubject([Body] UpdateSubjectRequest body);

        [Delete("/school/subject")]
        Task<ApiErrorResult> DeleteSubject([Body] IEnumerable<string> ids);

        [Post("/school/subject/copy")]
        Task<ApiErrorResult> CopySubject([Body] CopySubjectRequest body);
    }
}
