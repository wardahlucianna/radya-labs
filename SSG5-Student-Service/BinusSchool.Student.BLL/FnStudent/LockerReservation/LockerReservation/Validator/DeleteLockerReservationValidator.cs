using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator
{
    public class DeleteLockerReservationValidator : AbstractValidator<DeleteLockerReservationRequest>
    {
        public DeleteLockerReservationValidator()
        {
            RuleFor(x => x.IdStudentLockerReservation)
                .NotEmpty();
        }
    }
}
