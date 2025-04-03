using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator
{
    public class SaveGeneralPhysicalMeasurementValidator : AbstractValidator<SaveGeneralPhysicalMeasurementRequest>
    {
        public SaveGeneralPhysicalMeasurementValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Mode).NotEmpty();
            RuleFor(x => x.BodyTemperature).NotEmpty();
            RuleFor(x => x.MeasurementDate).NotEmpty();
            RuleFor(x => x.MeasurementPIC).NotEmpty();
        }
    }
}
