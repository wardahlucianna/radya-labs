using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;
using FluentValidation;
using System.Linq;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate.Validator
{
    public class DeleteCertificateTemplateValidator : AbstractValidator<DeleteCertificateTemplateRequest>
    {
        public DeleteCertificateTemplateValidator()
        {

            RuleFor(x => x.IdCertificateTemplate).NotNull();
            
            RuleFor(x => x.UserId).NotNull();
        }
    }
}
