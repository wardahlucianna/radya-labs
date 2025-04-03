using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator
{
    public class GetAttendanceAdministratorSummaryValidator : AbstractValidator<GetAttendanceAdministrationSummaryRequest>
    {
        public GetAttendanceAdministratorSummaryValidator()
        {
            RuleFor(x => x.IdStudent).NotNull();
            RuleFor(x => x.IdAttendance).NotNull();
            RuleFor(x => x.StartDate).NotNull();
            RuleFor(x => x.EndDate).NotNull();
            RuleFor(x => x.StartPeriod).NotNull();
            RuleFor(x => x.StartPeriod).NotNull();

        }
    }
}
