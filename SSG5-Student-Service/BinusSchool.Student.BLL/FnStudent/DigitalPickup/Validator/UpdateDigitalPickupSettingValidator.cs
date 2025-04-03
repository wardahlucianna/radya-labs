using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting;
using FluentValidation;


namespace BinusSchool.Student.FnStudent.DigitalPickup.Validator
{
    public class UpdateDigitalPickupSettingValidator : AbstractValidator<UpdateDigitalPickupSettingRequest>
    {
        public UpdateDigitalPickupSettingValidator()
        {
            RuleFor(x => x.IdGrade).NotNull();
            RuleFor(x => x.EndScanTime).NotNull();
            RuleFor(x => x.StartScanTime).NotNull();
        }
    }
}
