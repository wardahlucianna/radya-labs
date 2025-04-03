using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;


namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    public class AddMeritDemeritSanctionMappingCopyValidator : AbstractValidator<AddMeritDemeritSanctionMappingCopyRequest>
    {
        public AddMeritDemeritSanctionMappingCopyValidator()
        {
            RuleFor(x => x.IdAcademicYearCopyTo).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.IdSunctionMapping).NotEmpty().WithMessage("Sunction Map cannot empty");
        }
    }
}
