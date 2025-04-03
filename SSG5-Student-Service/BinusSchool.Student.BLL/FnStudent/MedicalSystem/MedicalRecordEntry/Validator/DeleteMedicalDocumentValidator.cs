using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator
{
    public class DeleteMedicalDocumentValidator : AbstractValidator<DeleteMedicalDocumentRequest>
    {
        public DeleteMedicalDocumentValidator()
        {
            RuleFor(x => x.IdDocument).NotEmpty();
        }
    }
}
