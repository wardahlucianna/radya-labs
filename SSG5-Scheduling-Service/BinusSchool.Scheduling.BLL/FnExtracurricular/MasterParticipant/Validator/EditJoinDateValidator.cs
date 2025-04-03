using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    internal class EditJoinDateValidator : AbstractValidator<EditJoinDateStudentParticipantRequest>
    {

        public EditJoinDateValidator()
        {
            RuleFor(x => x.EditRequestsList).NotNull();
        }
    }
}
