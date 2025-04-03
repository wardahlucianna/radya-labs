using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator
{
    public class SetStatusApprovalAttendanceAdministrationValidator : AbstractValidator<SetStatusApprovalAttendanceAdministrationRequest>
    {
        public SetStatusApprovalAttendanceAdministrationValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id can't be empty");
            RuleFor(x => x.IsApproved).NotNull().WithMessage("Status Approve can't be empty");
        }
    }

}
