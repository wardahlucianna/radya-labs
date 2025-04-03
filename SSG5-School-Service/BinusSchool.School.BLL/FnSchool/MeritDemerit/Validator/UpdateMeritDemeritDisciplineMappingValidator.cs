using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    public class UpdateMeritDemeritDisciplineMappingValidator : AbstractValidator<UpdateMeritDemeritDisciplineMappingRequest>
    {
        public UpdateMeritDemeritDisciplineMappingValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.IdGrade).NotEmpty().WithMessage("Grade cannot empty");
            RuleFor(x => x.DisciplineName).NotEmpty().WithMessage("Name cannot empty");
        }
    }
}
