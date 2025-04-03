using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSubject.CurriculumType;
using FluentValidation;

namespace BinusSchool.School.FnSubject.CurriculumType.Validator
{
    public class AddCurriculumTypeValidator : AbstractValidator<AddCurriculumTypeRequest>
    {
        public AddCurriculumTypeValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .MinimumLength(2)
                .WithName("Curriculum Type Short Name");

            RuleFor(x => x.Description)
                .NotEmpty()
                .MinimumLength(2)
                .WithName("Curriculum Type Name");
        }
    }
}
