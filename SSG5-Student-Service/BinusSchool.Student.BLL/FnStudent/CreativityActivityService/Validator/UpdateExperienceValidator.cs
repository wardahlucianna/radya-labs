using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class UpdateExperienceValidator : AbstractValidator<UpdateExperienceRequest>
    {
        public UpdateExperienceValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithName("Id");

            RuleFor(x => x.IdAcademicYear)
                .NotEmpty()
                .WithName("Id Academic Year");

            RuleFor(x => x.ExperienceName)
                .NotNull()
                .WithName("Experience Name");

            RuleFor(x => x.ExperienceLocation)
                .NotNull()
                .WithName("Experience Location");

            RuleFor(x => x.SupervisorName)
                .NotNull()
                .WithName("Supervisor Name");
            
            RuleFor(x => x.SupervisorTitle)
                .NotNull()
                .WithName("Supervisor Title");

            RuleFor(x => x.Organization)
                .NotNull()
                .WithName("Organization");

            RuleFor(x => x.Description)
                .NotNull()
                .WithName("Description");

            RuleFor(x => x.ContributionOrganizer)
                .NotNull()
                .WithName("Contribution Organizer");

            RuleFor(x => x.Role)
                .NotNull()
                .WithName("Role");

            RuleFor(x => x.IdUser)
                .NotNull()
                .WithName("Id User");

        }
    }
}
