using BinusSchool.Common.Model.Enums;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Common.Extensions
{
    public static class ApprovalStatusExtension
    {
        public static string AsString(this ApprovalStatus approvalStatus, IStringLocalizer localizer)
        {
            return approvalStatus switch
            {
                ApprovalStatus.Submit => "Submit",//localizer["Submit"],
                ApprovalStatus.NeedApproval => "Need Approval",//localizer["NeedApproval"],
                ApprovalStatus.NeedRevision => "Need Revision",//localizer["NeedRevision"],
                ApprovalStatus.Approved => "Approved",//localizer["Approve"],
                ApprovalStatus.Reject => "Reject",//localizer["Reject"],
                _ => "Draft",//localizer["Draft"]
            };
        }
    }
}