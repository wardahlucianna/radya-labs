using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.ApprovalAttendanceAdministration.validator
{
    public class ApprovalAttendanceAdministrationValidator : AbstractValidator<ApprovalAttendanceAdministrationRequest>
    {
        public ApprovalAttendanceAdministrationValidator()
        {
            RuleFor(x => x.IdRole).NotEmpty().NotNull();
            RuleFor(x => x.IdSchool).NotEmpty().NotNull();
        }
    }
}
