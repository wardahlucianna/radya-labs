using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    public class AddMeritDemeritDisciplineMappingCopyValidator : AbstractValidator<AddMeritDemeritDisciplineMappingCopyRequest>
    {
        public AddMeritDemeritDisciplineMappingCopyValidator()
        {
            RuleFor(x => x.IdAcademicYearCopyTo).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.IdDisciplineMapping).NotEmpty().WithMessage("Discipline Map cannot empty");
        }
    }
}
