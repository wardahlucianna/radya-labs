using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MasterEquipment.Validator
{
    public class SaveEquipmentTypeValidator : AbstractValidator<SaveEquipmentTypeRequest>
    {
        public SaveEquipmentTypeValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.EquipmentTypeName).NotEmpty();
            RuleFor(x => x.ReservationOwner).NotEmpty();
        }
    }
}
