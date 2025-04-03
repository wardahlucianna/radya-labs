using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSubject.SubjectLevel;
using Refit;

namespace BinusSchool.Data.Api.School.FnSubject
{
    public interface ISubjectLevel : IFnSubject
    {
        [Get("/school/subject-level")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetSubjectLevels(CollectionSchoolRequest query);

        [Get("/school/subject-level/{id}")]
        Task<ApiErrorResult<DetailResult2>> GetSubjectLevelDetail(string id);

        [Post("/school/subject-level")]
        Task<ApiErrorResult> AddSubjectLevel([Body] AddSubjectLevelRequest body);

        [Put("/school/subject-level")]
        Task<ApiErrorResult> UpdateSubjectLevel([Body] UpdateSubjectLevelRequest body);

        [Delete("/school/subject-level")]
        Task<ApiErrorResult> DeleteSubjectLevel([Body] IEnumerable<string> ids);
    }
}
