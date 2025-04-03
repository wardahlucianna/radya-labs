using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ITextbookPreparationUserPeriod : IFnSchool
    {
        [Get("/school/textbook-preparation/user-period")]
        Task<ApiErrorResult<IEnumerable<GetTextbookPreparationUserPeriodResult>>> GetTextbookPreparationUserPeriod(GetTextbookPreparationUserPeriodRequest param);

        [Post("/school/textbook-preparation/user-period")]
        Task<ApiErrorResult> AddTextbookPreparationUserPeriod([Body] AddTextbookPreparationUserPeriodRequest body);

        [Get("/school/textbook-preparation/user-period/{id}")]
        Task<ApiErrorResult<DetailTextbookPreparationUserPeriodResult>> DetailTextbookPreparationUserPeriod(string id);

        [Put("/school/textbook-preparation/user-period")]
        Task<ApiErrorResult> UpdateTextbookPreparationUserPeriod([Body] UpdateTextbookPreparationUserPeriodRequest body);

        [Delete("/school/textbook-preparation/user-period")]
        Task<ApiErrorResult> DeleteTextbookPreparationUserPeriod([Body] IEnumerable<string> body);

    }
}
