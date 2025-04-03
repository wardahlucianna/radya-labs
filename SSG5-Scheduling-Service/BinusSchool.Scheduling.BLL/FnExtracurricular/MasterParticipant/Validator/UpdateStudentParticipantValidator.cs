using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    public class UpdateStudentParticipantValidator : AbstractValidator<UpdateStudentParticipantRequest>
    {
        public UpdateStudentParticipantValidator()
        {
            RuleFor(x => x.IdHomeroom).NotEmpty();
            RuleFor(x => x.IdExtracurricular).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}
