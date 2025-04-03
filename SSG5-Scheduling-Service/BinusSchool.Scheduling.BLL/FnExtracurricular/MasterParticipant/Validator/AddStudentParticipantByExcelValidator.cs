using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    public class AddStudentParticipantByExcelValidator : AbstractValidator<AddStudentParticipantByExcelRequest>
    {
        public AddStudentParticipantByExcelValidator()
        {
            RuleFor(x => x.IdExtracurricular).NotEmpty();
            RuleFor(x => x.SendEmail).NotNull();
        }
    }
}
