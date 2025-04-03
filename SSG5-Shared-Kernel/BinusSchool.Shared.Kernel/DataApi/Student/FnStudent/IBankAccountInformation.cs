using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.BankAccountInformation;
using Refit;


namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IBankAccountInformation : IFnStudent
    {
        [Get("/student/bank-account-information")]
        Task<ApiErrorResult<IEnumerable<GetBankAccountInformationResult>>> GetBankAccountInformations(GetBankAccountInformationRequest query);

        [Put("/student/bank-account-information")]
        Task<ApiErrorResult> UpdateBankAccountInformation([Body] UpdateBankAccountInformationRequest body);
    }
}
