using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration.Validator
{
    public class GetRegistrationDetailValidator : AbstractValidator<GetRegistrationDetailRequest>
    {
        public GetRegistrationDetailValidator()
        {
            RuleFor(x => x.IdStudent)
               .NotEmpty()
               .ForEach(data => data.ChildRules(child =>
               {
                   child.RuleFor(x => x).NotEmpty();
               }));
        }
    }
}
