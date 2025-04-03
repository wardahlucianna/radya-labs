using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator
{
    public class PostAttendanceAdministratorValidator : AbstractValidator<PostAttendanceAdministrationRequest>
    {
        public PostAttendanceAdministratorValidator()
        {
            RuleFor(x => x.IdUser).NotNull().WithMessage("User can't be empty");
            RuleFor(x => x.Students).NotNull().WithMessage("Student can't be empty");
        }
    }

    //public class PostAttendanceAdministratorStudentValidator : AbstractValidator<IEnumerable<PostAttendanceAdministrationStudentRequest>>
    //{
    //    public PostAttendanceAdministratorStudentValidator()
    //    {
    //        RuleFor(e => e.Number).Length(10).Matches("^[0-9]$");
    //    }
    //}
}
