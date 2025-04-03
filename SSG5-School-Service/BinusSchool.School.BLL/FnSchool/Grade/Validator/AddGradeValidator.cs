using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Grade.Validator
{
    public class AddGradeValidator : AbstractValidator<AddGradeRequest>
    {
        public AddGradeValidator()
        {
            RuleFor(x => x.IdLevel).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Grade Short Name");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Grade Name");

            RuleFor(x => x.OrderNumber)
                .GreaterThan(0)
                .WithMessage("Order number must be grater than 0");
        }
    }
}
