using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.MasterWorkhabit;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.MasterWorkhabit.Validator
{
    public class AddWorkhabitValidator : AbstractValidator<AddWorkhabitRequest>
    {
        public AddWorkhabitValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Workhabit Short Name");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Workhabit Name");
        }
    }
}
