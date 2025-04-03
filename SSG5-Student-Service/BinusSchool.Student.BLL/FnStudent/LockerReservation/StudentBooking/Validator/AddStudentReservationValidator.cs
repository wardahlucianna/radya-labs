using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.StudentBooking;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.StudentBooking.Validator
{  
    public class AddStudentReservationValidator : AbstractValidator<AddStudentReservationRequest>
    {
        public AddStudentReservationValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.IdHomeroom).NotEmpty();
            RuleFor(x => x.IdlockerLocation).NotEmpty();
            RuleFor(x => x.IdlockerPosition).NotEmpty();
           
        }
    }
}
