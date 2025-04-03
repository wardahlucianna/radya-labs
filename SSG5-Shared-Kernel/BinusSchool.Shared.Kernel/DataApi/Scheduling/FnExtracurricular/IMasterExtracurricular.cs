using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IMasterExtracurricular : IFnExtracurricular
    {
        [Get("/master-extracurricular/get-master-extracurricular")]
        Task<ApiErrorResult<IEnumerable<GetMasterExtracurricularResult>>> GetMasterExtracurriculars(GetMasterExtracurricularRequest body);

        [Get("/master-extracurricular/getdetail-master-extracurricular")]
        Task<ApiErrorResult<GetMasterExtracurricularDetailResult>> GetMasterExtracurricularDetail(GetMasterExtracurricularDetailRequest query);

        [Post("/master-extracurricular/add-master-extracurricular")]
        Task<ApiErrorResult> AddMasterExtracurricular([Body] AddMasterExtracurricularRequest body);

        [Put("/master-extracurricular/update-master-extracurricular")]
        Task<ApiErrorResult> UpdateMasterExtracurricular([Body] UpdateMasterExtracurricularRequest body);

        [Delete("/master-extracurricular/delete-master-extracurricular")]
        Task<ApiErrorResult> DeleteMasterExtracurricular([Body] DeleteMasterExtracurricularRequest body);

        [Post("/master-extracurricular/export-excel-master-extracurricular")]
        Task<HttpResponseMessage> ExportExcelMasterExtracurricular([Body] ExportExcelMasterExtracurricularRequest body);

        [Post("/master-extracurricular/transfer-master-extracurricular")]
        Task<ApiErrorResult> TransferMasterExtracurricular([Body] TransferMasterExtracurricularRequest body);

        [Put("/master-extracurricular/update-SES-EC-UserRole")]
        Task<ApiErrorResult> UpdateSESnECUserRole([Body] UpdateSESnECUserRoleRequest body);

        [Get("/master-extracurricular/get-electives-byLevel")]
        Task<ApiErrorResult<IEnumerable<GetElectivesByLevelResult>>> GetElectivesByLevel(GetElectivesByLevelRequest query);

        [Post("/master-extracurricular/update-electives-entry-period")]
        Task<ApiErrorResult> UpdateElectivesEntryPeriod([Body] UpdateElectivesEntryPeriodRequest body);

        [Post("/master-extracurricular/get-master-extracurricularV2")]
        Task<ApiErrorResult<IEnumerable<GetMasterExtracurricularResult>>> GetMasterExtracurricularsV2([Body] GetMasterExtracurricularV2Request body);

        [Get("/master-extracurricular/get-master-extracurricular-type")]
        Task<ApiErrorResult<IEnumerable<GetMasterExtracurricularTypeResult>>> GetMasterExtracurricularType(GetMasterExtracurricularTypeRequest body);
    }
}
