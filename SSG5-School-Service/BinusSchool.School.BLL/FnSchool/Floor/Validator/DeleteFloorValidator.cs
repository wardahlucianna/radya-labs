using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Floor.Validator
{
    internal class DeleteFloorValidator : AbstractValidator<DeleteFloorRequest>
    {
        public DeleteFloorValidator()
        {
            RuleFor(x => x.IdFloor).NotEmpty();
        }
    }
}
