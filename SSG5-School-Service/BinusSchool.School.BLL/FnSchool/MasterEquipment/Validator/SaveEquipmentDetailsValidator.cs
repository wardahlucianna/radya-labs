using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MasterEquipment.Validator
{
    public class SaveEquipmentDetailsValidator : AbstractValidator<SaveEquipmentDetailsRequest>
    {
        public SaveEquipmentDetailsValidator()
        {
            RuleFor(x => x.IdEquipmentType).NotEmpty();
            RuleFor(x => x.EquipmentName).NotEmpty();
            RuleFor(x => x.TotalStockQty).NotNull();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
