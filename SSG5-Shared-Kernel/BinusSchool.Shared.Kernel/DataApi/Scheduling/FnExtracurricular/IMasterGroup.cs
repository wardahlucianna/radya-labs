using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IMasterGroup : IFnExtracurricular
    {
        [Post("/master-group/get-master-group")]
        Task<ApiErrorResult<IEnumerable<GetMasterGroupResult>>> GetMasterGroup([Body] GetMasterGroupRequest body);

        [Post("/master-group/create-master-group")]
        Task<ApiErrorResult> CreateMasterGroup([Body] CreateMasterGroupRequest body);

        [Put("/master-group/update-master-group")]
        Task<ApiErrorResult> UpdateMasterGroup([Body] UpdateMasterGroupRequest body);

        [Delete("/master-group/delete-master-group")]
        Task<ApiErrorResult> DeleteMasterGroup([Body] DeleteMasterGroupRequest body);

        [Post("/master-group/master-group-excel")]
        Task<HttpResponseMessage> ExportExcelMasterGroup([Body] ExportExcelMasterGroupRequest body);
    }
}
