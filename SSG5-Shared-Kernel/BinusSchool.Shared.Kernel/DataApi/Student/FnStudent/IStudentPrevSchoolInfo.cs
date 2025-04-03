using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentPrevSchoolInfo;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentPrevSchoolInfo : IFnStudent
    {
        [Get("/student/Student-PrevSchool-Info")]
        Task<ApiErrorResult<IEnumerable<GetStudentPrevSchoolInfoResult>>> GetStudentPrevSchoolInfos(GetStudentPrevSchoolInfoRequest query);

        [Get("/student/Student-PrevSchool-Info/{id}")]
        Task<ApiErrorResult<IEnumerable<GetStudentPrevSchoolInfoResult>>> GetStudentPrevSchoolInfoDetail(string id);

        [Put("/student/Student-PrevSchool-Info")]
        Task<ApiErrorResult> UpdateStudentPrevSchoolInfo([Body] UpdateStudentPrevSchoolInfoRequest body);
    }
}
