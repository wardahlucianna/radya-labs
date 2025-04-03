using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom.Validator
{
    public class MoveHomeroomSyncValidator : AbstractValidator<MoveHomeroomSyncRequest>
    {
        public MoveHomeroomSyncValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
        }
    }
}
