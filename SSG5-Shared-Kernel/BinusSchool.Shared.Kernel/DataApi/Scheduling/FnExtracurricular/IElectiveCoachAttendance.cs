using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IElectiveCoachAttendance : IFnExtracurricular
    {
        [Get("/extracurricular/coach-att/get-coach-attendace")]
        Task<ApiErrorResult<IEnumerable<GetElectiveCoachAttendanceResult>>> GetElectiveCoachAttendance(GetElectiveCoachAttendanceRequest body);

        [Post("/extracurricular/coach-att/add-coach-attendace")]
        Task<ApiErrorResult> AddElectiveCoachAttendance([Body] AddElectiveCoachAttendanceRequest body);
    }
}
