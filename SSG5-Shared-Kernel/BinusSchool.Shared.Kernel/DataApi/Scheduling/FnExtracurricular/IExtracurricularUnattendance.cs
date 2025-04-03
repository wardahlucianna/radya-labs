using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IExtracurricularUnattendance : IFnExtracurricular
    {
        [Get("/extracurricular-unattendance/get-extracurricular-unattendance")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricularUnattendanceResult>>> GetExtracurricularUnattendance(GetExtracurricularUnattendanceRequest query);

        //[Post("/extracurricular-unattendance/update-extracurricular-attendance")]-
        //Task<ApiErrorResult> UpdateExtracurricularUnattendance([Body] List<UpdateExtracurricularAttendanceRequest> body);

        [Post("/extracurricular-unattendance/add-extracurricular-unattendance")]
        Task<ApiErrorResult> AddExtracurricularUnattendance([Body] AddExtracurricularUnattendanceRequest body);

        [Delete("/extracurricular-unattendance/delete-extracurricular-unattendance")]
        Task<ApiErrorResult> DeleteExtracurricularUnattendance([Body] DeleteExtracurricularUnattendanceRequest body);
    }
}

