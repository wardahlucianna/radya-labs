using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.School.FnPeriod
{
    public interface ISemester : IFnPeriod
    {
        [Get("/school/semester")]
        Task<ApiErrorResult<IEnumerable<int>>> GetSemesters(string idGrade);
    }
}