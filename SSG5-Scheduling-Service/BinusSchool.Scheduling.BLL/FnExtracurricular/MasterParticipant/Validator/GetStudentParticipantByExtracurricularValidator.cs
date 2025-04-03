using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    public class GetStudentParticipantByExtracurricularValidator : AbstractValidator<GetStudentParticipantByExtracurricularRequest>
    {
        public GetStudentParticipantByExtracurricularValidator()
        {
            RuleFor(x => x.IdExtracurricular).NotEmpty();
        }
    }
}
