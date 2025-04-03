using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator
{
    public class SaveLockerReservationValidator : AbstractValidator<SaveLockerReservationRequest>
    {
        public SaveLockerReservationValidator()
        {
            RuleFor(x => x.IdLocker).NotEmpty()
                .WithMessage("Locker cannot be empty");
            RuleFor(x => x.IdStudent).NotEmpty()
                .WithMessage("StudentID cannot be empty");
            RuleFor(x => x.IdGrade).NotEmpty()
                .WithMessage("Grade cannot be empty");
            RuleFor(x => x.IdHomeroom).NotEmpty()
                .WithMessage("Homeroom cannot be empty");
            RuleFor(x => x.IdReserver).NotEmpty();
            RuleFor(x => x.IsAgree).NotNull();
            //RuleFor(x => x.Notes).NotEmpty();
        }
    }
}
