using System.Linq;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class CasExperienceDownloadRequestValidator : AbstractValidator<CasExperienceDownloadRequest>
    {
        public CasExperienceDownloadRequestValidator()
        {
            RuleFor(e => e.IdUser).NotEmpty().WithName("Id User");

            RuleFor(e => e.IdStudent).NotEmpty().WithName("Id Student");

            RuleFor(e => e.IdSchool).NotEmpty().WithName("Id School");

            RuleFor(e => e.Role).NotEmpty().WithName("Role");

            RuleFor(e => e.IdAcademicYears).Must(e => e.Any()).WithName("Id Academic years");
        }
    }
}
