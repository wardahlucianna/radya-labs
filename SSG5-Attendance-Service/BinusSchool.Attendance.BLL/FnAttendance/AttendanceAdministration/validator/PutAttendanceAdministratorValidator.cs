using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator
{
    public class PutAttendanceAdministratorValidator : AbstractValidator<PutAttendanceAdministrationV2Request>
    {
        public PutAttendanceAdministratorValidator()
        {
            RuleFor(x => x.IdAttendanceAdministration).NotNull().WithMessage("Id Attendance can't be empty");
        }
    }
}
