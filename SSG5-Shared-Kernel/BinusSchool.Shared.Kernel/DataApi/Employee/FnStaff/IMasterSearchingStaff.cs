using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Employee.MasterSearching;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IMasterSearchingStaff : IFnStaff
    {
        [Get("/staff/MasterSearching/GetFieldDataList")]
        Task<ApiErrorResult<IEnumerable<GetFieldDataListforMasterSearchingStaffResult>>> GetStaffFieldData();

        [Post("/staff/MasterSearching/GetMasterSearchingData")]
        Task<ApiErrorResult<IEnumerable<GetMasterSearchingforStaffResult>>> GetMasterSearchingDataForStaff([Body] GetMasterSearchingDataforStaffRequest param);

        [Post("/staff/MasterSearching-export")]
        Task<HttpResponseMessage> MasterSearchingforStaffExport([Body] ExportToExcelMasterSearchingStaffDataRequest body);
    }
}
