using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.DigitalPickup.Validator
{
    public class GetDigitalPickupSettingValidator : AbstractValidator<GetDigitalPickupSettingRequest>
    {
        public GetDigitalPickupSettingValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotNull();
        }
    }
}
