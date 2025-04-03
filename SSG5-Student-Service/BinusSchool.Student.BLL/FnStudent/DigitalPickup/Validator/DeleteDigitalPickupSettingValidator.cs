using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.DigitalPickup.Validator
{
    public class DeleteDigitalPickupSettingValidator : AbstractValidator<DeleteDigitalPickupSettingRequest>
    {
        public DeleteDigitalPickupSettingValidator()
        {
            RuleFor(x => x.IdGrade).NotNull();
        }
    }
}
