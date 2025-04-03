using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSubject.CurriculumType;
using Refit;

namespace BinusSchool.Data.Api.School.FnSubject
{
    public interface ICurriculumType : IFnSubject
    {
        [Get("/school/curriculum-type")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetCurriculumTypes(CollectionSchoolRequest query);

        [Get("/school/curriculum-type/{id}")]
        Task<ApiErrorResult<DetailResult2>> GetCurriculumTypeDetail(string id);

        [Post("/school/curriculum-type")]
        Task<ApiErrorResult> AddCurriculumType([Body] AddCurriculumTypeRequest body);

        [Put("/school/curriculum-type")]
        Task<ApiErrorResult> UpdateCurriculumType([Body] UpdateCurriculumTypeRequest body);

        [Delete("/school/curriculum-type")]
        Task<ApiErrorResult> DeleteCurriculumType([Body] IEnumerable<string> ids);
    }
}
