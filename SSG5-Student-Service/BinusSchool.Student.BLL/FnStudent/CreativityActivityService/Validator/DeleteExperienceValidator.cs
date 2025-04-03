using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class DeleteExperienceValidator : AbstractValidator<DeleteExperienceRequest>
    {
        public DeleteExperienceValidator()
        {
            RuleFor(x => x.IdExperience)
                .NotEmpty()
                .WithName("Id Experience");
        }
    }
}
