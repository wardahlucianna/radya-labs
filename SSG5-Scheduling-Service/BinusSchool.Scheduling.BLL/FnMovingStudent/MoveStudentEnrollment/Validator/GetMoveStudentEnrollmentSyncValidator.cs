using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment.Validator
{
    public class GetMoveStudentEnrollmentSyncValidator : AbstractValidator<MoveStudentEnrollmentSyncRequest>
    {
        public GetMoveStudentEnrollmentSyncValidator()
        {
            RuleFor(x => x.idSchool).NotEmpty().WithMessage("Id school old cannot empty");
        }
    }
}
