using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IAliveStatus : IFnStudent
    {
        [Get("/student/alive-status")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetAliveStatus();
           
    }
}
