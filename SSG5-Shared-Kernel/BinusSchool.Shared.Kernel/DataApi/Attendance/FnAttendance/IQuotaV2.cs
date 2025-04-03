using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.QuotaV2;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IQuotaV2 : IFnAttendance
    {
        [Get("/quotaV2/detail/{idLevel}")]
        Task<ApiErrorResult<QuotaV2Result>> GetQuotaV2Detail(string idLevel);

        [Post("/quotaV2")]
        Task<ApiErrorResult> SetQuota([Body] SetQuotaV2Request body);
    }
}
