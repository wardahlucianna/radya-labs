using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.Level;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Level.Validator
{
    public class UpdateLevelValidator : AbstractValidator<UpdateLevelRequest>
    {
        public UpdateLevelValidator()
        {
            Include(new AddLevelValidator());
            
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.OrderNumber)
                .GreaterThan(0)
                .WithMessage("Order number must be grater than 0");
        }
    }
}
