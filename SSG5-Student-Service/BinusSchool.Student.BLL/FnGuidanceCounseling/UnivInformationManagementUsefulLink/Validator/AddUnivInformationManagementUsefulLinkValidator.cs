using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink.Validator
{
    public   class AddUnivInformationManagementUsefulLinkValidator : AbstractValidator<AddUnivInformationManagementUsefulLinkRequest>
    {
        public AddUnivInformationManagementUsefulLinkValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.GradeIds).NotEmpty();

            RuleFor(x => x.LinkDescription).NotEmpty();

            RuleFor(x => x.Link).NotEmpty();

            RuleFor(x => x.Link).Must(StringUtil.IsValidUrl).WithMessage("Invalid link");
        }
    }
}
