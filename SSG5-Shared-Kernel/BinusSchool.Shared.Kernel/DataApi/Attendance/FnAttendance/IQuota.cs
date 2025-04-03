using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Quota;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IQuota : IFnAttendance
    {
        [Get("/quota/detail/{idLevel}")]
        Task<ApiErrorResult<QuotaResult>> GetQuotaDetail(string idLevel);

        [Post("/quota")]
        Task<ApiErrorResult> SetQuota([Body] SetQuotaRequest body);
    }
}
