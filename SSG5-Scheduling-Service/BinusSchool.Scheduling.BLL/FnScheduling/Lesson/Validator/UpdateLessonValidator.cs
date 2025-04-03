using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Lesson.Validator
{
    public class UpdateLessonValidator : AbstractValidator<UpdateLessonRequest>
    {
        public UpdateLessonValidator()
        {
            RuleFor(x => x.ClassIdGenerated).NotEmpty();

            RuleFor(x => x.TotalPerWeek).GreaterThanOrEqualTo(0);

            RuleFor(x => x.IdWeekVarian).NotEmpty();

            RuleFor(x => x.Homerooms)
                .NotEmpty()
                .ForEach(homerooms => homerooms.ChildRules(homeroom =>
                {
                    homeroom.RuleFor(x => x.IdHomeroom).NotEmpty();

                    homeroom.RuleFor(x => x.Homeroom).NotEmpty();

                    homeroom.RuleFor(x => x.IdPathways)
                        .ForEach(pathways => pathways.ChildRules(pathway =>
                        {
                            pathway.RuleFor(x => x).NotEmpty();
                        }));
                }));

            RuleFor(x => x.Teachers)
                .NotEmpty()
                .ForEach(teachers => teachers.ChildRules(teacher =>
                {
                    teacher.RuleFor(x => x.IdTeacher).NotEmpty();
                }));
        }
    }
}