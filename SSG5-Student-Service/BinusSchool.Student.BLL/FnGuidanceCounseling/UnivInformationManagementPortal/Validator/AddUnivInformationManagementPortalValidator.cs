using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementPortal.Validator
{
    public class AddUnivInformationManagementPortalValidator : AbstractValidator<AddUnivInformationManagementPortalRequest>
    {
        public AddUnivInformationManagementPortalValidator()
        {
            RuleFor(x => x.UnivercityName).NotEmpty();

            RuleFor(x => x.UnivercityWebsite).NotEmpty();

            When(x => !string.IsNullOrEmpty(x.UnivercityWebsite), () => {
                RuleFor(x => x.UnivercityWebsite).Must(StringUtil.IsValidUrl).WithMessage("Invalid website");
            });

        }
    }
}
