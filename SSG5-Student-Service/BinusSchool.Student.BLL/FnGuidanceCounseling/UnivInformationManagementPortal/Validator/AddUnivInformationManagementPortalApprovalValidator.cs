using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementPortal.Validator
{
    public class AddUnivInformationManagementPortalApprovalValidator : AbstractValidator<AddUnivInformationManagementPortalApprovalRequest>
    {
        public AddUnivInformationManagementPortalApprovalValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.IdUnivInformationManagementPortal).NotEmpty();

            RuleFor(x => x.IdUserApproval).NotEmpty();

        }
    }
}
