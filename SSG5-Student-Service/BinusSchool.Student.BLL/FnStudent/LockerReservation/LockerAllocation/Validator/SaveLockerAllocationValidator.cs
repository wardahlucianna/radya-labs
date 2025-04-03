using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation.Validator
{
    public class SaveLockerAllocationValidator : AbstractValidator<SaveLockerAllocationRequest>
    {
        public SaveLockerAllocationValidator()
        {
            RuleFor(x => x.IdAcademicYear)
                .NotEmpty();

            RuleFor(x => x.Semester)
                .NotEmpty();

            RuleFor(x => x.IdBuilding)
                .NotEmpty();

            RuleFor(x => x.IdFloor)
                .NotEmpty();

            RuleFor(x => x.TotalLocker)
                .NotEmpty()
                .LessThanOrEqualTo(999)
                    .WithMessage("Locker quantity cannot be greater than 999.")
                .Must(totalLocker => totalLocker % 2 == 0)
                    .WithMessage("The locker total must be even.");
                
        }
    }
}
