using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStayingWith : IFnStudent
    {
        [Get("/student/staying-with")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetStayingWith();
 
    }
}
