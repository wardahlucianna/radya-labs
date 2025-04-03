using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentPhoto : IFnStudent
    {
        [Get("/student-photo/GetListStudentPhotoData")]
        Task<ApiErrorResult<IEnumerable<GetStudentPhotoListResult>>> GetListStudentPhotoData(GetStudentPhotoListRequest query);

        [Post("/student-photo/SaveStudentPhotoData")]
        Task<ApiErrorResult> SaveStudentPhotoData([Body] SaveStudentPhotoDataRequest query);

        [Post("/student-photo/CopyStudentPhotoData")]
        Task<ApiErrorResult> CopyStudentPhotoData([Body] CopyStudentPhotoRequest query);

        [Delete("/student-photo/DeleteStudentPhotoRequest")]
        Task<ApiErrorResult> DeleteStudentPhotoData([Body] DeleteStudentPhotoRequest query);
    }
}
