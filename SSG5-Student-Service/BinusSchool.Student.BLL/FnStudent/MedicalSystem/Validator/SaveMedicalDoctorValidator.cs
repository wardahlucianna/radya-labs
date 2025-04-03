using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Validator
{
    public class SaveMedicalDoctorValidator : AbstractValidator<SaveMedicalDoctorRequest>
    {
        public SaveMedicalDoctorValidator()
        {
            RuleFor(x => x.DoctorName).NotEmpty().WithMessage("Doctor Name is required");
            RuleFor(x => x.DoctorAddress).NotEmpty().WithMessage("Doctor Address is required");
            RuleFor(x => x.DoctorPhoneNumber).NotEmpty().WithMessage("Doctor Phone Number is required");
            RuleFor(x => x.DoctorEmail).NotEmpty().WithMessage("Doctor Email is required");
            RuleFor(x => x.IdMedicalHospital).NotEmpty().WithMessage("Hospital is required");
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("School is required");
        }
    }
}
