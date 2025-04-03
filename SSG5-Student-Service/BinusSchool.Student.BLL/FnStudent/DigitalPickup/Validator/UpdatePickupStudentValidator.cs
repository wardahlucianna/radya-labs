using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.DigitalPickup.Validator
{
    public class UpdatePickupStudentValidator : AbstractValidator<UpdatePickupStudentRequest>
    {
        public UpdatePickupStudentValidator()
        {
            RuleFor(x => x.Status)
                .InclusiveBetween(1, 2)
                .WithMessage("Status value must be either 1 or 2");
            RuleFor(x => x.IdDigitalPickup).NotNull();
        }
    }
}
