using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad.Validator
{
    public class AddNonTeachLoadValidator : AbstractValidator<AddNonTeachLoadRequest>
    {
        public AddNonTeachLoadValidator()
        {
            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.IdPosition).NotEmpty();

            RuleFor(x => x.Category).IsInEnum().NotEmpty();

            RuleFor(x => x.Load).GreaterThan(0);
        }
    }
}
