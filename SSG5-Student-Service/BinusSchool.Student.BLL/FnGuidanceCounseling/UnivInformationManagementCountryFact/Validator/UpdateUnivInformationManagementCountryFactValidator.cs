using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact.Validator
{
    public class UpdateUnivInformationManagementCountryFactValidator : AbstractValidator<UpdateUnivInformationManagementCountryFactRequest>
    {
        public UpdateUnivInformationManagementCountryFactValidator()
        {

            RuleFor(x => x.LevelIds).NotEmpty();

            RuleFor(x => x.CountryName).NotEmpty();

            When(x => !string.IsNullOrEmpty(x.CountryWebsite), () => {
                RuleFor(x => x.CountryWebsite).Must(StringUtil.IsValidUrl).WithMessage("Invalid website");
            });
        }

    }
}
