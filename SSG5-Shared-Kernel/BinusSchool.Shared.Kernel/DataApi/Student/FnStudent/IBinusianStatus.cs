using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IBinusianStatus : IFnStudent
    {
        [Get("/student/binusian-status")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetBinusianStatus();

        [Get("/student/binusian-status/{id}")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetBinusianStatusDetail(string id);

    }
}
