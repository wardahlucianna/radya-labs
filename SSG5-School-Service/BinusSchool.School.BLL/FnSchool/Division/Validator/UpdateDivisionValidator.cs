using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.Division;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Divisions.Validator
{
    public class UpdateDivisionValidator : AbstractValidator<UpdateDivisionRequest>
    {
        public UpdateDivisionValidator()
        {
            Include(new AddDivisionValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
