using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator
{
    public class GetListLockerReservationValidator : AbstractValidator<GetListLockerReservationRequest>
    {
        public GetListLockerReservationValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotNull();
        }
    }
}
