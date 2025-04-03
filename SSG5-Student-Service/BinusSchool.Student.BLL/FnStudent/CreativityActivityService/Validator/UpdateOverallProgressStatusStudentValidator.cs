using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class UpdateOverallProgressStatusStudentValidator : AbstractValidator<UpdateOverallProgressStatusStudentRequest>
    {
        public UpdateOverallProgressStatusStudentValidator()
        {
            RuleFor(x => x.IdStudent)
                .NotEmpty()
                .WithName("Id Student");

            RuleFor(x => x.StatusOverallExperienceStudent)
                .NotNull()
                .WithName("Status Overall");

            RuleFor(x => x.IdUser)
                .NotNull()
                .WithName("Id User");
        }
    }
}
