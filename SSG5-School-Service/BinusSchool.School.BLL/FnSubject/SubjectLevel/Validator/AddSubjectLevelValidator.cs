using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSubject.SubjectLevel;
using FluentValidation;

namespace BinusSchool.School.FnSubject.SubjectLevel.Validator
{
    public class AddSubjectLevelValidator : AbstractValidator<AddSubjectLevelRequest>
    {
        public AddSubjectLevelValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Subject Level Short Name");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Subject Level Name");
        }
    }
}
