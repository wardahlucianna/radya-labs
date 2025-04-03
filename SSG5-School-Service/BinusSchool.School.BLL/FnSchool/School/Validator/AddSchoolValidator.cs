using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.School;
using FluentValidation;

namespace BinusSchool.School.FnSchool.School.Validator
{
    public class AddSchoolValidator : AbstractValidator<AddSchoolRequest>
    {
        public AddSchoolValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
