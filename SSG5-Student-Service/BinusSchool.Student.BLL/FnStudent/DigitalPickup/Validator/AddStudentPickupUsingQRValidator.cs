using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ScannerQRCode;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.DigitalPickup.Validator
{
    public class AddStudentPickupUsingQRValidator : AbstractValidator<AddStudentPickupUsingQRRequest>
    {
        public AddStudentPickupUsingQRValidator()
        {
            RuleFor(x => x.IdDigitalPickupQrCode).NotNull();
        }
    }
}
