using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator
{
    public class UpdateLockerReservationValidator : AbstractValidator<UpdateLockerReservationRequest>
    {
        public UpdateLockerReservationValidator()
        {
            RuleFor(x => x.IdLocker).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.IdHomeroom).NotEmpty();
            RuleFor(x => x.IdReserver).NotEmpty();
            //RuleFor(x => x.IsAgree).NotEmpty();
            //RuleFor(x => x.Notes).NotEmpty();
        }
    }
}
