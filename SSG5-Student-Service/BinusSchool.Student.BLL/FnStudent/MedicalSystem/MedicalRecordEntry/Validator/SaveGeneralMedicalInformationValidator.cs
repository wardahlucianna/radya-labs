using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator
{
    public class SaveGeneralMedicalInformationValidator : AbstractValidator<SaveGeneralMedicalInformationRequest>
    {
        public SaveGeneralMedicalInformationValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Mode).NotEmpty();
        }
    }
}
