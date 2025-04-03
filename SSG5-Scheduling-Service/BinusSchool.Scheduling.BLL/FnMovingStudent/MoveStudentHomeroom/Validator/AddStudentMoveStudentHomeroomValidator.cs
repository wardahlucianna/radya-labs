using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom.Validator
{
    public class AddStudentMoveStudentHomeroomValidator : AbstractValidator<AddStudentMoveStudentHomeroomRequest>
    {
        public AddStudentMoveStudentHomeroomValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.IdHomeroom).NotEmpty();
            RuleFor(x => x.IdHomeroomOld).NotEmpty();
            RuleFor(x => x.EffectiveDate).NotEmpty();
        }
    }
}
