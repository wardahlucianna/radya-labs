using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation.Validator
{
    internal class UpdatePersonalInvitationValidator : AbstractValidator<UpdatePersonalInvitationRequest>
    {
        public UpdatePersonalInvitationValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.IdUserTeacher).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.InvitationDate).NotEmpty();
            RuleFor(x => x.Role).NotEmpty();
            RuleFor(x => x.InvitationStartTime).NotEmpty();
            RuleFor(x => x.InvitationEndTime).NotEmpty();
        }

}
}
