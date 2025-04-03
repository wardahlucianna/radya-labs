using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IStudentCertification : IFnSchedule
    {
        [Get("/schedule/student-certification")]
        Task<ApiErrorResult<IEnumerable<GetListStudentCertificationResult>>> GetListStudentCertification(GetListStudentCertificationRequest query);
        [Get("/schedule/get-acad-year-by-student")]
        Task<ApiErrorResult<IEnumerable<GetAcadYearByStudentResult2>>> GetAcadYearByStudent(GetAcadYearByStudentRequest query);
    }
}
