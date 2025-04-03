using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.TeachingAssignmentInfo;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ITeacherAssignmentInfo : IFnScoring
    {
        [Get("/Scoring/TeachingAssignmentInfo/GetClassBasedOnTeacherAssignment")]
        Task<ApiErrorResult<IEnumerable<GetClassBasedOnTeacherAssignmentResult>>> GetClassBasedOnTeacherAssignment(GetClassBasedOnTeacherAssignmentRequest query);

        [Get("/Scoring/TeachingAssignmentInfo/GetGradeBasedOnTeacher")]
        Task<ApiErrorResult<IEnumerable<GetGradeBasedOnTeacherResult>>> GetGradeBasedOnTeacher(GetGradeBasedOnTeacherRequest query);

        [Get("/Scoring/TeachingAssignmentInfo/GetClassroomBasedOnTeacherAssignment")]
        Task<ApiErrorResult<IEnumerable<GetClassroomBasedOnTeacherAssignmentResult>>> GetClassroomBasedOnTeacherAssignment(GetClassroomBasedOnTeacherAssignmentRequest query);
    }
}
