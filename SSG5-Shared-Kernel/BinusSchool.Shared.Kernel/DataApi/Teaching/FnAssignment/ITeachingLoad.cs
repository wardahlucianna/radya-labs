using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeachingLoad;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITeachingLoad : IFnAssignment
    {
        [Get("/assignment/teaching-load")]
        Task<ApiErrorResult<IEnumerable<GetTeacherLoadResult>>> GetTeacherLoads(GetTeacherLoadRequest query);
    }
}
