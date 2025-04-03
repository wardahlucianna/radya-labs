using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Validator
{
    public class SaveMedicalItemValidator : AbstractValidator<SaveMedicalItemRequest>
    {
        public SaveMedicalItemValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.MedicalItemName).NotEmpty();
            RuleFor(x => x.IdMedicalItemType).NotEmpty();
            RuleFor(x => x.IsCommonDrug).NotNull();
            RuleFor(x => x.IdDosageType).NotEmpty();
        }
    }
}
