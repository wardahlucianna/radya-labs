using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSubject.SubjectType;
using FluentValidation;

namespace BinusSchool.School.FnSubject.SubjectType.Validator
{
    public class AddSubjectTypeValidator : AbstractValidator<AddSubjectTypeRequest>
    {
        public AddSubjectTypeValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .MinimumLength(2)
                .WithName("Subject Type Short Name");

            RuleFor(x => x.Description)
                .NotEmpty()
                .MinimumLength(2)
                .WithName("Subject Type Name");
        }
    }
}
