﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendanceDate;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IExtracurricularAttendanceDate : IFnExtracurricular
    {
        [Get("/extracurricular-attendance-date/get-extracurricular-attendance-bydate")]
        Task<ApiErrorResult<IEnumerable<GetElectiveAttendanceByDateResult>>> GetElectiveAttendanceByDate(GetElectiveAttendanceByDateRequest body);

    }
}
