using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.ClassRoom;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IClassRoom : IFnSchool
    {
        [Get("/school/class_room")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetClassRooms(CollectionSchoolRequest query);

        [Get("/school/class_room/{id}")]
        Task<ApiErrorResult<DetailResult2>> GetClassRoomDetail(string id);

        [Post("/school/class_room")]
        Task<ApiErrorResult> AddClassRoom([Body] AddClassRoomRequest body);

        [Put("/school/class_room")]
        Task<ApiErrorResult> UpdateClassRoom([Body] UpdateClassRoomRequest body);

        [Delete("/school/class_room")]
        Task<ApiErrorResult> DeleteClassRoom([Body] IEnumerable<string> ids);
    }
}
