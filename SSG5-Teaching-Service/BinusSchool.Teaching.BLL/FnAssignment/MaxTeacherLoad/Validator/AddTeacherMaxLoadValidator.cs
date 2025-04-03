using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.MaxTeacherLoad;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.MaxTeacherLoad.Validator
{
    public class AddTeacherMaxLoadValidator : AbstractValidator<AddTeacherMaxLoadRequest>
    {
        public AddTeacherMaxLoadValidator()
        {
            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.MaxLoad)
                .NotEmpty()
                .GreaterThanOrEqualTo(1)
                .LessThan(int.MaxValue);
        }
    }
}
