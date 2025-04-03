using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.StudentProgramme.Validator
{
    public class UpdateStudentProgrammeValidator : AbstractValidator<UpdateStudentProgrammeRequest>
    {
        public UpdateStudentProgrammeValidator()
        {
            RuleFor(x => x.id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.effectiveDate).NotEmpty().WithMessage("Effective date cannot empty");
        }
    }
}
