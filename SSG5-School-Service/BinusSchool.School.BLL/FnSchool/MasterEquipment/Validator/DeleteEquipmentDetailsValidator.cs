using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MasterEquipment.Validator
{
    public class DeleteEquipmentDetailsValidator : AbstractValidator<DeleteEquipmentDetailsRequest>
    {
        public DeleteEquipmentDetailsValidator()
        {
            RuleFor(x => x.IdEquipment).NotEmpty();
        }
    }
}
