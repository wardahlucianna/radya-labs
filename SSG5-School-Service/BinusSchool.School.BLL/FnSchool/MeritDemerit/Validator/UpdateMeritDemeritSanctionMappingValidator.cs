using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    public class UpdateMeritDemeritSanctionMappingValidator : AbstractValidator<UpdateMeritDemeritSanctionMappingRequest>
    {
        public UpdateMeritDemeritSanctionMappingValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.NameSanction).NotEmpty().WithMessage("Name cannot empty");
            RuleFor(x => x.Min).NotEmpty().WithMessage("Min cannot empty");
            RuleFor(x => x.Max).NotEmpty().WithMessage("Max cannot empty");
            RuleFor(x => x.Attention).NotEmpty().WithMessage("Attention by cannot empty");
        }
    }
}
