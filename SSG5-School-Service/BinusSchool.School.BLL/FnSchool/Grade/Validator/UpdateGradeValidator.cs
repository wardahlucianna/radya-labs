using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Grade.Validator
{
    public class UpdateGradeValidator : AbstractValidator<UpdateGradeRequest>
    {
        public UpdateGradeValidator()
        {
            Include(new AddGradeValidator());

            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.OrderNumber)
                .GreaterThan(0)
                .WithMessage("Order number must be grater than 0");
        }
    }
}
