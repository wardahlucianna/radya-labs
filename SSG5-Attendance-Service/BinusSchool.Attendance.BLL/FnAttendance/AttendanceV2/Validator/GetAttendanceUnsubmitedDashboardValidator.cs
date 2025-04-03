using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2.Validator
{
    public class GetAttendanceUnsubmitedDashboardValidator : AbstractValidator<GetAttendanceUnsubmitedDashboardRequest>
    {
        public GetAttendanceUnsubmitedDashboardValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.SelectedPosition).NotEmpty();
        }
    }
}
