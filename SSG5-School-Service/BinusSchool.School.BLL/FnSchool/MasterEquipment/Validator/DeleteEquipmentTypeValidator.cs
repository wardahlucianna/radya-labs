using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MasterEquipment.Validator
{
    public class DeleteEquipmentTypeValidator : AbstractValidator<DeleteEquipmentTypeRequest>
    {
        public DeleteEquipmentTypeValidator()
        {
            RuleFor(x => x.IdEquipmentType).NotEmpty();
        }
    }
}
