using System.Threading.Tasks;
using BinusSchool.Data.Models.Binusian.BinusSchool;
using BinusSchool.Data.Models.Binusian.BinusSchool.Auth;
using Refit;

namespace BinusSchool.Data.Apis.Binusian.BinusSchool
{
    public interface IAuth
    {
        [Get("/binusschool/auth/token")]
        Task<BinusianDataApiResult<GetTokenResult>> GetToken();
    }
}
