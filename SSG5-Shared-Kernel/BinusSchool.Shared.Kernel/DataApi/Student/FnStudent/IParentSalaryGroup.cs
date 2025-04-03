using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IParentSalaryGroup : IFnStudent
    {
        [Get("/student/parent-salary-group")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetParentSalaryGroup();
 
        
    }
}
