using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Pathway.Validator
{
    public class CopyPathwayValidator : AbstractValidator<CopyPathwayRequest>
    {
        public CopyPathwayValidator()
        {
            RuleFor(x => x.IdAcadyearFrom).NotEmpty();
            RuleFor(x => x.IdAcadyearTo).NotEmpty();
        }
    }
}
