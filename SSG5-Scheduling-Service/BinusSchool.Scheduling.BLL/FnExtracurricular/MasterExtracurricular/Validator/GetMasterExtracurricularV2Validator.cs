using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator
{
    public class GetMasterExtracurricularV2Validator : AbstractValidator<GetMasterExtracurricularV2Request>
    {
        public GetMasterExtracurricularV2Validator()
        {
            RuleFor(a => a.IdAcademicYear)
                .NotEmpty();
        }
    }
}
