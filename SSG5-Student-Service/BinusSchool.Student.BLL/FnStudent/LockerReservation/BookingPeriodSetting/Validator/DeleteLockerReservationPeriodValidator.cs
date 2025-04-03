using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting.Validator
{
    public class DeleteLockerReservationPeriodValidator : AbstractValidator<DeleteLockerReservationPeriodRequest>
    {
        public DeleteLockerReservationPeriodValidator()
        {
            RuleFor(x => x.IdLockerReservationPeriod)
                .NotEmpty();
        }
    }
}
