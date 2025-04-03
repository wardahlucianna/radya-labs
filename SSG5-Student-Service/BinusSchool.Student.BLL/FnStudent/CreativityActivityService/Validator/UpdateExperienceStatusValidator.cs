using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class UpdateExperienceStatusValidator : AbstractValidator<UpdateStatusExperienceRequest>
    {
        public UpdateExperienceStatusValidator()
        {
            RuleFor(x => x.IdExperience)
                .NotEmpty()
                .WithName("Id Experience");

            RuleFor(x => x.IdStudent)
                .NotEmpty()
                .WithName("Id Student");

            RuleFor(x => x.ExperienceStatus)
                .NotNull()
                .WithName("Experience Status");

            RuleFor(x => x.IdUser)
               .NotNull()
               .WithName("Id User");

            RuleFor(x => x.IdAcademicYear)
               .NotNull()
               .WithName("Id Academic Year");
        }
    }
}
