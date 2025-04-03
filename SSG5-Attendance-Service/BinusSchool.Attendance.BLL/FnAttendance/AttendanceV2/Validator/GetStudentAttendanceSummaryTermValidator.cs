using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2.Validator
{
    public class GetStudentAttendanceSummaryTermValidator : AbstractValidator<GetStudentAttendanceSummaryTermRequest>
    {
        public GetStudentAttendanceSummaryTermValidator()
        {
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.Students).NotEmpty();
        }
    }
}
