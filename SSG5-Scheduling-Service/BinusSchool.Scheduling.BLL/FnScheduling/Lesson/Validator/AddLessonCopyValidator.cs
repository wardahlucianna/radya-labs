using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Lesson.Validator
{
    public class AddLessonCopyValidator : AbstractValidator<AddLessonCopyRequest>
    {
        public AddLessonCopyValidator()
        {
            RuleFor(x => x.IdAcadyearCopyTo).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.SemesterCopyTo).NotEmpty().WithMessage("Semester cannot empty");
            RuleFor(x => x.IdLesson).NotEmpty().WithMessage("Id Lesson cannot empty");
        }
    }
}
