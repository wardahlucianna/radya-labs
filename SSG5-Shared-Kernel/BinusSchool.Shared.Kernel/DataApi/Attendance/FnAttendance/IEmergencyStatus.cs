using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IEmergencyStatus : IAttendance
    {
        [Get("/emergency-status")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetEmergencyStatus();
    }
}
