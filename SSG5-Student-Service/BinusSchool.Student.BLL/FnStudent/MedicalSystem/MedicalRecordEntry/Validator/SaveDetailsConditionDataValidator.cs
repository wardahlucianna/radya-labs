using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry;
using FluentValidation;
using Microsoft.Azure.Cosmos.Linq;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator
{
    public class SaveDetailsConditionDataValidator : AbstractValidator<SaveDetailsConditionDataRequest>
    {
        public SaveDetailsConditionDataValidator()
        {
            RuleFor(x => x.CheckInDate).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
