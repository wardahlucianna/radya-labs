using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentSubject.Validator
{
    public class AddMoveStudentSubjectValidator : AbstractValidator<AddMoveStudentSubjectRequest>
    {
        public AddMoveStudentSubjectValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.IdSubjectOld).NotEmpty();
            //RuleFor(x => x.IdSubject).NotEmpty();
            RuleFor(x => x.EffectiveDate).NotEmpty();
        }
    }
}
