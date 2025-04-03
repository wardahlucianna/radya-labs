using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IMapStudentHomeroom : IFnSchedule
    {
        [Get("/schedule/map-student-homeroom/{id}")]
        Task<ApiErrorResult<IEnumerable<GetMapStudentHomeroomDetailResult>>> GetMapStudentHomeroomDetail(string id, string idGrade);

        [Post("/schedule/map-student-homeroom")]
        Task<ApiErrorResult> AddMapStudentHomeroom([Body] AddMapStudentHomeroomRequest body);

        [Delete("/schedule/map-student-homeroom")]
        Task<ApiErrorResult> DeleteMapStudentHomeroom([Body] IEnumerable<string> ids);

        [Get("/schedule/map-student-homeroom/available")]
        Task<ApiErrorResult<IEnumerable<GetMapStudentPathwayResult>>> GetAvailableMapStudentHomeroom(GetMapStudentHomeroomAvailableRequest query);

        [Get("/schedule/map-student-homeroom/copy-to-next-or-previous")]
        Task<ApiErrorResult<GetHomeroomCopyToNextOrPreviousSemesterResult>> GetHomeroomCopyToNextOrPreviousSemester(GetHomeroomCopyToNextOrPreviousSemesterRequest query);
    }
}
