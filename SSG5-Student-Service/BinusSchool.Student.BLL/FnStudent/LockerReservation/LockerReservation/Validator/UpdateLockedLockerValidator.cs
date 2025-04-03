using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator
{
    public class UpdateLockedLockerValidator : AbstractValidator<UpdateLockedLockerRequest>
    {
        public UpdateLockedLockerValidator()
        {
            RuleFor(x => x.IdLocker).NotEmpty();
            RuleFor(x => x.LockedLocker).NotNull();
        }
    }
}
