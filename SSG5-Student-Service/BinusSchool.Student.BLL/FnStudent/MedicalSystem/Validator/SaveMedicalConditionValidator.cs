using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Validator
{
    public class SaveMedicalConditionValidator : AbstractValidator<SaveMedicalConditionRequest>
    {
        public SaveMedicalConditionValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.MedicalConditionName).NotEmpty();
            RuleFor(x => x.IdMedicalItem).NotEmpty();
            RuleFor(x => x.IdMedicalTreatment).NotEmpty();
        }
    }
}
