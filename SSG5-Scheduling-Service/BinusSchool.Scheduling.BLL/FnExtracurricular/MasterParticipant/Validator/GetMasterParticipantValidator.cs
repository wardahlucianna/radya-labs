using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    public class GetMasterParticipantValidator : AbstractValidator<GetMasterParticipantRequest>
    {
        public GetMasterParticipantValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
        }
    }
}
