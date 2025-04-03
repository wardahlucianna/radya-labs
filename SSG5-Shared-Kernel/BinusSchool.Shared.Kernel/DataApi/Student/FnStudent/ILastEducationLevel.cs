using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;


namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface ILastEducationLevel : IFnStudent
    {
        [Get("/student/last-education-level")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetLastEducationLevel();
 
    }
}
