using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Validator
{
    public class SaveMedicalHospitalValidator : AbstractValidator<SaveMedicalHospitalRequest>
    {
        public SaveMedicalHospitalValidator() {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.HospitalName).NotEmpty();
            RuleFor(x => x.HospitalAddress).NotEmpty();
            RuleFor(x => x.HospitalPhone).NotEmpty();
            RuleFor(x => x.HospitalEmail).NotEmpty();
        }
    }
}
