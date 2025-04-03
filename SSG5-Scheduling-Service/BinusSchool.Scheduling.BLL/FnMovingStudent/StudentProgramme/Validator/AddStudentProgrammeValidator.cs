using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.StudentProgramme.Validator
{
    public class AddStudentProgrammeValidator : AbstractValidator<AddStudentProgrammeRequest>
    {
        public AddStudentProgrammeValidator()
        {
            RuleFor(x => x.idSchool).NotEmpty().WithMessage("Id school cannot empty");
            RuleFor(x => x.idstudent).NotEmpty().WithMessage("Id student cannot empty");
        }
    }
}
