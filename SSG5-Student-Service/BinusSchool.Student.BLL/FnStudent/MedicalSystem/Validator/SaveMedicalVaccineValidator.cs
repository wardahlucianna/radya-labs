using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Validator
{
    public class SaveMedicalVaccineValidator : AbstractValidator<SaveMedicalVaccineRequest>
    {
        public SaveMedicalVaccineValidator()
        {
            RuleFor(x => x.MedicalVaccineName).NotEmpty();
            RuleFor(x => x.IdDosageType).NotEmpty();
            RuleFor(x => x.DosageAmount).GreaterThan(0);
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
