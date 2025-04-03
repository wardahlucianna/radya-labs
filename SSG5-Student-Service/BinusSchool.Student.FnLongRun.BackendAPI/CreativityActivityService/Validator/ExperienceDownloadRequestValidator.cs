using System.Linq;
using BinusSchool.Data.Model.Student.FnLongRun;
using FluentValidation;

namespace BinusSchool.Student.FnLongRun.CreativityActivityService.Validator
{
    public class ExperienceDownloadRequestValidator : AbstractValidator<ExperienceDownloadRequest>
    {
        public ExperienceDownloadRequestValidator()
        {
            RuleFor(e => e.Id).NotEmpty();

            RuleFor(e => e.IdUser).NotEmpty();

            RuleFor(e => e.IdStudent).NotEmpty();

            RuleFor(e => e.IdSchool).NotEmpty();

            RuleFor(e => e.IdAcademicYears).Must(e => e.Any());
        }
    }
}
