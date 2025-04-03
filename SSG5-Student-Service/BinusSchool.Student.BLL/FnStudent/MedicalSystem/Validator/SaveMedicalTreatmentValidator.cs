using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Validator
{
    public class SaveMedicalTreatmentValidator : AbstractValidator<SaveMedicalTreatmentRequest>
    {
        public SaveMedicalTreatmentValidator()
        {
            RuleFor(x => x.MedicalTreatmentName).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
