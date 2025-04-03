using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.MaxTeacherLoad;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface IMaxTeacherLoad : IFnAssignment
    {
        [Get("/assignment/teacher/max-load")]
        Task<ApiErrorResult<IEnumerable<GetTeacherMaxLoadResult>>> GetMaxTeacherLoads(CollectionSchoolRequest query);

        [Get("/assignment/teacher/max-load/{id}")]
        Task<ApiErrorResult<GetTeacherMaxLoadDetailResult>> GetMaxTeacherLoadDetail(string Id);

        [Put("/assignment/teacher/max-load")]
        Task<ApiErrorResult> UpdateMaxTeacherLoad([Body] AddTeacherMaxLoadRequest body);
    }
}
