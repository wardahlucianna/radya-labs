using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation.Validator
{
    public class CopyLockerAllocationFromLastSemesterValidator : AbstractValidator<CopyLockerAllocationFromLastSemesterRequest>
    {
        public CopyLockerAllocationFromLastSemesterValidator()
        {
            RuleFor(x => x.IdAcademicYear)
                .NotEmpty();

            RuleFor(x => x.Semester)
                .NotEmpty();
        }
    }
}
