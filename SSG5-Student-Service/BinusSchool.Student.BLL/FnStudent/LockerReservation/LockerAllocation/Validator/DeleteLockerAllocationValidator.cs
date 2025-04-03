using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation.Validator
{
    public class DeleteLockerAllocationValidator : AbstractValidator<DeleteLockerAllocationRequest>
    {
        public DeleteLockerAllocationValidator()
        {
            RuleFor(x => x.IdAcademicYear)
                .NotEmpty();

            RuleFor(x => x.Semester)
                .NotEmpty();

            RuleFor(x => x.IdBuilding)
                .NotEmpty();

            RuleFor(x => x.IdFloor)
                .NotEmpty();
        }
    }
}
