using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent;
using BinusSchool.Data.Model.Student.FnStudent.StudentDocument;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentEmail : IFnStudent
    {
        [Post("/student/Email/GetStudentEmailRecommendation")]
        Task<ApiErrorResult> GetStudentEmailRecommendation([Body] GetStudentEmailRecomendationRequest body);

        [Post("/student/Email/CheckEmailAvailability")]
        Task<ApiErrorResult> CheckEmailAvailability([Body] CheckEmailAvailabilityRequest body);

        [Post("/student/Email/CreateStudentEmail")]
        Task<ApiErrorResult> CreateStudentEmail([Body] CreateStudentEmailRequest body);
        
        [Post("/student/Email/GetStudentEmailList")]
        Task<ApiErrorResult<IEnumerable<GetStudentEmailListResult>>> GetStudentEmailList([Body] GetStudentEmailListRequest body);
    }
}
