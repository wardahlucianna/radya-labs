using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2.Validator
{
    public class CancelAttendanceValidator : AbstractValidator<CancelAttendanceRequest>
    {
        public CancelAttendanceValidator()
        {
            RuleFor(x => x.IdAttendanceAdministration).NotNull();
            RuleFor(x => x.IdStudent).NotNull();
        }
    }
}
