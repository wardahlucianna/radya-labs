using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.School.FnSchool.ClassRoom;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IClassroomMap : IFnSchool
    {
        [Get("/school/class_room_mapping_to_grade_streaming")]
        Task<ApiErrorResult<IEnumerable<GetMappingClassResult>>> GetMappingClasses(GetMappingClassRequest query);

        [Get("/school/class_room_mapping_to_grade_streaming/{id}")]
        Task<ApiErrorResult<GetMappingClassDetailResult>> GetMappingClassDetail(string id);

        [Post("/school/class_room_mapping_to_grade_streaming")]
        Task<ApiErrorResult> AddMappingClass([Body] AddMappingClass body);

        [Put("/school/class_room_mapping_to_grade_streaming")]
        Task<ApiErrorResult> UpdateMappingClass([Body] UpdateMappingClass body);

        [Delete("/school/class_room_mapping_to_grade_streaming")]
        Task<ApiErrorResult> DeleteMappingClass([Body] IEnumerable<string> ids);

        [Delete("/school/class_room_mapping_to_grade_streaming/delete_division")]
        Task<ApiErrorResult> DeleteDivision(string IdClassRoomDivision);

        [Delete("/school/class_room_mapping_to_grade_streaming/delete_pathway")]
        Task<ApiErrorResult> DeletePathway(string IdPathwayDetail);

        [Delete("/school/class_room_mapping_to_grade_streaming/delete_class")]
        Task<ApiErrorResult> DeleteClass(string IdGradePathwayClassroom);

        [Get("/school/classroom-map")]
        Task<ApiErrorResult<IEnumerable<GetClassroomMapResult>>> GetClassroomMappeds(GetClassroomMapRequest query);

        [Get("/school/classroom-map-by-grade")]
        Task<ApiErrorResult<IEnumerable<GetClassroomMapByGradeResult>>> GetClassroomMappedsByGrade(GetListGradePathwayClassRoomRequest query);

        [Get("/school/classroom-map-by-level")]
        Task<ApiErrorResult<IEnumerable<GetClassroomMapByGradeResult>>> GetClassroomMappedsByLevel(GetMappingClassByLevelRequest query);


        [Get("/school/classroom-map-by-grade-pathway")]
        Task<ApiErrorResult<IEnumerable<GetClassroomMapByGradeResult>>> GetClassroomMappedsByGradePathway(GetClassroomByGradePathwayRequest query);
    }
}
