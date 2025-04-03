using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad.Validator
{
    public class UpdateNonTeachLoadValidator : AbstractValidator<UpdateNonTeachLoadRequest>
    {
        public UpdateNonTeachLoadValidator()
        {
            Include(new AddNonTeachLoadValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
