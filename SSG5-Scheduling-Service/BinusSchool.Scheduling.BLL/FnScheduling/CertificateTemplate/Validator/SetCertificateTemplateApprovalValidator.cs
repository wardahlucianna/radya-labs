using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;
using FluentValidation;
using System.Linq;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate.Validator
{
    public class SetCertificateTemplateApprovalValidator : AbstractValidator<SetCertificateTemplateApprovalRequest>
    {
        public SetCertificateTemplateApprovalValidator()
        {

            RuleFor(x => x.Id).NotEmpty();
            
            RuleFor(x => x.UserId).NotNull();

            RuleFor(x => x.IsApproved).NotNull();
        }
    }
}
