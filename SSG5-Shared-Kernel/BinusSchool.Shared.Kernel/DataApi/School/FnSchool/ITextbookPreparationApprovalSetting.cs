using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApprovalSetting;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ITextbookPreparationApprovalSetting : IFnSchool
    {
        [Get("/school/textbook-preparation/approval-setting")]
        Task<ApiErrorResult<IEnumerable<GetTextbookPreparationApprovalSettingResult>>> GetTextbookPreparationApprovalSetting(GetTextbookPreparationApprovalSettingRequest param);

        [Post("/school/textbook-preparation/approval-setting")]
        Task<ApiErrorResult> AddTextbookPreparationApprovalSetting([Body]AddTextbookPreparationApprovalSettingRequest body);
    }
}
