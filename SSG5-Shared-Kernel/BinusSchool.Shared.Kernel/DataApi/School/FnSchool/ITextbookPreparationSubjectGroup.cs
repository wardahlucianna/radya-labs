using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ITextbookPreparationSubjectGroup : IFnSchool
    {
        [Get("/school/textbook-preparation/subject-group")]
        Task<ApiErrorResult<IEnumerable<GetTextbookPreparationSubjectGroupResult>>> GetTextbookPreparationSubjectGroup(GetTextbookPreparationSubjectGroupRequest param);

        [Post("/school/textbook-preparation/subject-group-subject")]
        Task<ApiErrorResult<IEnumerable<GetTextbookPreparationSubjectResult>>> GetSubject([Body] GetTextbookPreparationSubjectRequest body);

        [Post("/school/textbook-preparation/subject-group")]
        Task<ApiErrorResult> AddTextbookPreparationSubjectGroup([Body] AddTextbookPreparationSubjectGroupRequest body);

        [Get("/school/textbook-preparation/subject-group/{id}")]
        Task<ApiErrorResult<DetailTextbookPreparationSubjectGroupResult>> DetailTextbookPreparationSubjectGroup(string id);

        [Put("/school/textbook-preparation/subject-group")]
        Task<ApiErrorResult> UpdateTextbookPreparationSubjectGroup([Body] UpdateTextbookPreparationSubjectGroupRequest body);

        [Delete("/school/textbook-preparation/subject-group")]
        Task<ApiErrorResult> DeleteTextbookPreparationSubjectGroup([Body] IEnumerable<string> body);
    }
}
