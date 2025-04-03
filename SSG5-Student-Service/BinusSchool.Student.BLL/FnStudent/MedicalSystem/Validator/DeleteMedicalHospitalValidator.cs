using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Validator
{
    public class DeleteMedicalHospitalValidator : AbstractValidator<DeleteMedicalHospitalRequest>
    {
        public DeleteMedicalHospitalValidator()
        {
            RuleFor(x => x.IdHospital).NotEmpty();
        }
    }
}
