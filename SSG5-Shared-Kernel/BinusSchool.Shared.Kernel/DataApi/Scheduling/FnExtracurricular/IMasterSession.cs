using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricularSession;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IMasterSession : IFnExtracurricular
    {
        [Post("/extracurricular/master-session/CheckAvailableSession")]
        Task<ApiErrorResult<CheckAvailableSessionForExtracurricularResult>> CheckAvailableSession([Body] CheckAvailableSessionForExtracurricularRequest body);
    }
}
