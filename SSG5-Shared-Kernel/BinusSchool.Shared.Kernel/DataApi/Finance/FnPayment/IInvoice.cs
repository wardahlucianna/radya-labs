using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Finance.FnPayment.Invoice;
using Refit;

namespace BinusSchool.Data.Api.Finance.FnPayment
{
    public interface IInvoice : IFnPayment
    {
        [Get("/invoice/get-summary-student-invoice")]
        Task<ApiErrorResult<IEnumerable<GetSummaryInvoiceResult>>> GetSummaryInvoice(GetSummaryInvoiceRequest query);

        [Post("/invoice/add-master-invoice-payment")]
        Task<ApiErrorResult<AddMasterInvoiceResult>> AddMasterInvoice([Body] AddMasterInvoiceRequest body);

        [Put("/invoice/update-master-invoice-payment")]
        Task<ApiErrorResult> UpdateMasterInvoice([Body] UpdateMasterInvoiceRequest body);

        [Delete("/invoice/delete-master-invoice-payment")]
        Task<ApiErrorResult> DeleteMasterInvoice([Body] DeleteMasterInvoiceRequest body);


        [Get("/invoice/get-master-invoice-detail")]
        Task<ApiErrorResult<GetMasterInvoiceDetailResult>> GetMasterInvoiceDetail(GetMasterInvoiceDetailRequest query);

        [Post("/invoice/export-excel-summary-student-invoice")]
        Task<HttpResponseMessage> ExportExcelSummaryInvoice([Body] GetSummaryInvoiceRequest body);

        [Post("/invoice/export-excel-invoice-detail")]
        Task<HttpResponseMessage> ExportExcelMasterInvoiceDetail([Body] GetMasterInvoiceDetailRequest body);
    }
}
