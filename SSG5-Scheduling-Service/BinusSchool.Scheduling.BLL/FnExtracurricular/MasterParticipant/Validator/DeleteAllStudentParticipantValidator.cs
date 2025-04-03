using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    public class DeleteAllStudentParticipantValidator : AbstractValidator<DeleteAllStudentParticipantRequest>
    {
        public DeleteAllStudentParticipantValidator()
        {
            RuleFor(x => x.IdExtracurricularList).NotEmpty();
        }
    }
}
