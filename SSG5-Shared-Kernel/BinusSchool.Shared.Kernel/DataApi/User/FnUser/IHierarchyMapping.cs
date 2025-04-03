using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.User.FnUser.HierarchyMapping;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
    public interface IHierarchyMapping : IFnUser
    {
        [Get("/hierarchy-mapping")]
        Task<ApiErrorResult<IEnumerable<HierarchyMappingResult>>> GetHierarchyMappings(CollectionSchoolRequest request);

        [Get("/hierarchy-mapping/detail/{id}")]
        Task<ApiErrorResult<HierarchyMappingDetailResult>> GetHierarchyMappingDetail(string id);

        [Post("/hierarchy-mapping")]
        Task<ApiErrorResult> AddHierarchyMapping([Body] AddHierarchyMappingRequest body);

        [Put("/hierarchy-mapping")]
        Task<ApiErrorResult> UpdateHierarchyMapping([Body] UpdateHierarchyMappingRequest body);

        [Delete("/hierarchy-mapping")]
        Task<ApiErrorResult> DeleteHierarchyMapping([Body] IEnumerable<string> ids);
    }
}
