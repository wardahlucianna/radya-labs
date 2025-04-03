using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.DailyAttendanceRecap;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.DailyAttendanceRecap.Validator
{
    public class GenerateExcelDailyAttendanceRecapValidator : AbstractValidator<GenerateExcelDailyAttendanceRecapRequest>
    {
        public GenerateExcelDailyAttendanceRecapValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
        }
    }
}
