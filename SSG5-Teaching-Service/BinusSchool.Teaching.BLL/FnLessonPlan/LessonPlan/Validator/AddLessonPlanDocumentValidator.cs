using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using FluentValidation;
using System.Linq;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator
{
    public class AddLessonPlanDocumentValidator : AbstractValidator<AddLessonPlanDocumentRequest>
    {
        public AddLessonPlanDocumentValidator()
        {
            RuleFor(x => x.PathFile)
                .NotNull()
                .WithMessage("Path File is required");
            RuleFor(x => x.Filename)
                .NotNull()
                .WithMessage("File name is required");
            RuleFor(x => x.IdAcademicYear)
                .NotNull()
                .WithMessage("Academic is required");
            RuleFor(x => x.IdGrade)
                .NotNull()
                .WithMessage("Grade is required");
            RuleFor(x => x.IdPeriod)
                .NotNull()
                .WithMessage("Period is required");
            RuleFor(x => x.IsAllSubject)
                .NotNull()
                .WithMessage("Subject is required");
            RuleFor(x => x.WeekNumber)
                .NotNull()
                .WithMessage("Week is required");
            RuleFor(x => x.IdUser)
               .NotNull()
               .WithMessage("User is required");
            // RuleFor(x => x.IdLessonPlan)
            //     .NotNull()
            //     .WithMessage("Id Lesson Plan is required");
        }
    }
}
