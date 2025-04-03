using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    internal class AddMeritDemeritDisciplineMappingValidator : AbstractValidator<AddMeritDemeritDisciplineMappingRequest>
    {
        public AddMeritDemeritDisciplineMappingValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.IdGrade).NotEmpty().WithMessage("Grade cannot empty");
            RuleFor(x => x.DisciplineName).NotEmpty().WithMessage("Name cannot empty");
        }
    }
}
