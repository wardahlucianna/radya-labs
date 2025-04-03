using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricularSession;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricularSession.Validator
{
    public class CheckAvailableSessionForExtracurricularValidator : AbstractValidator<CheckAvailableSessionForExtracurricularRequest>
    {
        public CheckAvailableSessionForExtracurricularValidator()
        {
            //RuleFor(x => x.IdSchool).NotEmpty();

            //RuleFor(x => x.Code)
            //    .NotEmpty()
            //    .WithName("Division Short Name");

            //RuleFor(x => x.Description)
            //    .NotEmpty()
            //    .WithName("Division Name");
        }
    }
}
