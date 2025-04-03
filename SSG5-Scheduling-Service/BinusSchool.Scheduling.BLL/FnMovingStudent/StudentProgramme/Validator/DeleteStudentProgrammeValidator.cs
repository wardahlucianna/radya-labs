using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.StudentProgramme.Validator
{
    public class DeleteStudentProgrammeValidator : AbstractValidator<DeleteStudentProgrammeRequest>
    {
        public DeleteStudentProgrammeValidator()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.homeroom).NotEmpty().WithMessage("Homeroom cannot empty");
        }
    }
}
