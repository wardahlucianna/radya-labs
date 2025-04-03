using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.DigitalPickup.Validator
{
    public class AddStudentDigitalPickupQRValidator : AbstractValidator<StudentDigitalPickupQRRequest>
    {
        public AddStudentDigitalPickupQRValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotNull();
            RuleFor(x => x.IdGrade).NotNull();
            RuleFor(x => x.IdStudent).NotNull();
        }
    }
}
