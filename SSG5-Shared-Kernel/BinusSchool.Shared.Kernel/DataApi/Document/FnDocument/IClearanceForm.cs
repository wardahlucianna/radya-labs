using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.ClearanceForm;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IClearanceForm : IFnDocument
    {
        [Get("/clearance-form/get-clearanceform-period")]
        Task<ApiErrorResult<GetClearanceFormPeriodResult>> GetClearanceFormPeriod(GetClearanceFormPeriodRequest param);

        [Get("/clearance-form/get-allstudentstatus-clearanceform")]
        Task<ApiErrorResult<GetAllStudentStatusClearanceFormResult>> GetAllStudentStatusClearanceForm(GetAllStudentStatusClearanceFormRequest param);
    }
}
