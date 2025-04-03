using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;
using FluentValidation;
using System.Linq;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate.Validator
{
    public class AddCertificateTemplateValidator : AbstractValidator<AddCertificateTempRequest>
    {
        public AddCertificateTemplateValidator()
        {

            RuleFor(x => x.IdAcademicYear).NotEmpty();
            
            RuleFor(x => x.IsUseBinusLogo).NotNull();

            RuleFor(x => x.Name).NotNull();

            RuleFor(x => x.Title).NotNull();

            RuleFor(x => x.SubTitle).NotNull();

            RuleFor(x => x.Description).NotNull();

            RuleFor(x => x.Background).NotNull();

            RuleFor(x => x.Signature1).NotNull();

            RuleFor(x => x.Signature1As).NotNull();

        }
    }
}
