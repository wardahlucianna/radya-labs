using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator
{
    public class AddMedicalRecordEntryOtherPatientValidator : AbstractValidator<AddMedicalRecordEntryOtherPatientRequest>
    {
        public AddMedicalRecordEntryOtherPatientValidator()
        {
            RuleFor(a => a.IdSchool).NotEmpty();
            RuleFor(a => a.Name).NotEmpty();
            RuleFor(a => a.PhoneNumber).NotEmpty();
            RuleFor(a => a.BirthDate).NotEmpty();
        }
    }
}
