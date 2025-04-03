using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator
{
    public class GetMasterParticipantValidatorV2 : AbstractValidator<GetMasterParticipantRequestV2>
    {
        public GetMasterParticipantValidatorV2()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            //RuleFor(x => x.Semester).NotEmpty();
        }
    }
}
