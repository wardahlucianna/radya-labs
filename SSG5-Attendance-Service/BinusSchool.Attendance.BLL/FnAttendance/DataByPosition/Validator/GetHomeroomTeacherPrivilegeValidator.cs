using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;

using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition.Validator
{
    public class GetHomeroomTeacherPrivilegeValidator : AbstractValidator<GetHomeroomTeacherPrivilegeRequest>
    {
        public GetHomeroomTeacherPrivilegeValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            //RuleFor(x => x.Semester).NotEmpty();
        }
    }
}
