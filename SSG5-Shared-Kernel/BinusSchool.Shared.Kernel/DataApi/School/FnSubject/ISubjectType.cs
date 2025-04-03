using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSubject.SubjectType;
using Refit;

namespace BinusSchool.Data.Api.School.FnSubject
{
    public interface ISubjectType : IFnSubject
    {
        [Get("/school/subject-type")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetSubjectTypes(CollectionSchoolRequest query);

        [Get("/school/subject-type/{id}")]
        Task<ApiErrorResult<DetailResult2>> GetSubjectTypeDetail(string id);

        [Post("/school/subject-type")]
        Task<ApiErrorResult> AddSubjectType([Body] AddSubjectTypeRequest body);

        [Put("/school/subject-type")]
        Task<ApiErrorResult> UpdateSubjectType([Body] UpdateSubjectTypeRequest body);

        [Delete("/school/subject-type")]
        Task<ApiErrorResult> DeleteSubjectType([Body] IEnumerable<string> ids);
    }
}
