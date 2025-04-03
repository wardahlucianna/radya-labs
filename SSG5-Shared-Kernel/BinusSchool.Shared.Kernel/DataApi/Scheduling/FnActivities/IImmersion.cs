using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnActivities
{
    public interface IImmersion : IFnActivities
    {
        #region Immersion Period
        [Get("/immersion/immersion-period")]
        Task<ApiErrorResult<IEnumerable<GetImmersionPeriodResult>>> GetImmersionPeriod(GetImmersionPeriodRequest body);

        [Get("/immersion/immersion-period-detail")]
        Task<ApiErrorResult<GetImmersionPeriodDetailResult>> GetImmersionPeriodDetail(GetImmersionPeriodDetailRequest body);

        [Put("/immersion/immersion-period")]
        Task<ApiErrorResult> UpdateImmersionPeriod([Body] UpdateImmersionPeriodRequest body);

        [Delete("/immersion/immersion-period")]
        Task<ApiErrorResult> DeleteImmersionPeriod([Body] DeleteImmersionPeriodRequest body);
        #endregion

        #region Master Immersion
        [Get("/immersion/master-immersion")]
        Task<ApiErrorResult<IEnumerable<GetMasterImmersionResult>>> GetMasterImmersion(GetMasterImmersionRequest body);

        [Get("/immersion/master-immersion-detail")]
        Task<ApiErrorResult<GetMasterImmersionDetailResult>> GetMasterImmersionDetail(GetMasterImmersionDetailRequest body);

        [Post("/immersion/master-immersion-excel")]
        Task<HttpResponseMessage> ExportExcelMasterImmersion([Body] ExportExcelMasterImmersionRequest body);

        [Post("/immersion/master-immersion")]
        Task<ApiErrorResult<AddMasterImmersionResult>> AddMasterImmersion([Body] AddMasterImmersionRequest body);

        [Put("/immersion/master-immersion")]
        Task<ApiErrorResult> UpdateMasterImmersion([Body] UpdateMasterImmersionRequest body);

        [Delete("/immersion/master-immersion")]
        Task<ApiErrorResult> DeleteMasterImmersion([Body] DeleteMasterImmersionRequest body);

        [Get("/immersion/immersion-doc")]
        Task<ApiErrorResult<IEnumerable<ImmersionDocumentResult_Get>>> GetImmersionDocument(ImmersionDocumentRequest_Get body);

        [Get("/immersion/immersion-doc/{id}")]
        Task<ApiErrorResult<ImmersionDocumentResult_GetDetail>> GetImmersionDocumentDetail(string id);

        //[Multipart]
        //[Post("/immersion/immersion-doc")]
        //Task<ApiErrorResult> AddImmersionDocument(string IdImmersion,
        //                                        ImmersionDocumentRequest_ImmersionDocumentType ImmersionDocumentType,
        //                                        bool NewCreatedImmersion,                                                      
        //                                        [AliasAs("file")] StreamPart file);

        [Multipart]
        [Put("/immersion/immersion-doc")]
        Task<ApiErrorResult> UpdateImmersionDocument(string IdImmersion,
                                             string ImmersionDocumentType,  // "brochure" or "poster"
                                             bool NewCreatedImmersion,

                                             [AliasAs("file")] StreamPart file);

        [Delete("/immersion/immersion-doc")]
        Task<ApiErrorResult> DeleteImmersionDocument([Body] IEnumerable<string> idImmersions);

        [Get("/immersion/get-currency")]
        Task<ApiErrorResult<IEnumerable<GetCurrencyResult>>> GetCurrency(GetCurrencyRequest body);

        [Get("/immersion/get-immersion-payment-method")]
        Task<ApiErrorResult<IEnumerable<GetImmersionPaymentMethodResult>>> GetImmersionPaymentMethod(GetImmersionPaymentMethodRequest body);
        #endregion
    }
}
