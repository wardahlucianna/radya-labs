using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IFileAttendance : IFnAttendance
    {
        [Delete("/attendance/file/delete")]
        Task<ApiErrorResult> DeleteFile([Body] FileRequest body);
    }
}
