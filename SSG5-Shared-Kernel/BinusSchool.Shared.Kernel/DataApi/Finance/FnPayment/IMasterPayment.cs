using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Finance.FnPayment.MasterPayment;
using Refit;

namespace BinusSchool.Data.Api.Finance.FnPayment
{
    public interface IMasterPayment : IFnPayment
    {
        [Get("/masterpayment/get-package-payment")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetPackagePayment(GetPackagePaymentRequest query);

        [Get("/masterpayment/get-status-workflow-payment")]
        Task<ApiErrorResult<IEnumerable<GetStatusWorkflowPaymentResult>>> GetStatusWorkflowPayment();

        [Get("/masterpayment/get-setting-for-createinvoice")]
        Task<ApiErrorResult<GetSettingForCreateInvoieResult>> GetSettingForCreateInvoie(GetSettingForCreateInvoieRequest query);

        [Get("/masterpayment/get-payment-method")]
        Task<ApiErrorResult<IEnumerable<GetPaymentMethodResult>>> GetPaymentMethod(GetPaymentMethodRequest query);

        [Get("/masterpayment/get-list-event-payment")]
        Task<ApiErrorResult<IEnumerable<GetListEventPaymentResult>>> GetListEventPayment(GetListEventPaymentRequest query);

    }
}
