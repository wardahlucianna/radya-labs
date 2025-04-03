using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting.Validator
{
    public class SaveLockerBookingPeriodGradeValidator : AbstractValidator<SaveLockerBookingPeriodGradeRequest>
    {
        public SaveLockerBookingPeriodGradeValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.Grades).NotEmpty();
        }
    }
}
