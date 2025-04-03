using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Lesson.Validator
{
    public class AddLessonValidator : AbstractValidator<AddLessonRequest>
    {
        public AddLessonValidator()
        {
            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.Semester).NotEmpty().GreaterThan(0);

            RuleFor(x => x.IdGrade).NotEmpty();

            RuleFor(x => x.IdSubject).NotEmpty();

            RuleFor(x => x.ClassIdFormat).NotEmpty();

            RuleFor(x => x.ClassIdExample).NotEmpty();

            RuleFor(x => x.Lessons)
                .NotEmpty()
                .ForEach(lessons => lessons.ChildRules(lesson => 
                {
                    lesson.RuleFor(x => x.ClassIdGenerated).NotEmpty();

                    lesson.RuleFor(x => x.TotalPerWeek).GreaterThanOrEqualTo(0);

                    lesson.RuleFor(x => x.IdWeekVarian).NotEmpty();

                    lesson.RuleFor(x => x.Homerooms)
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

                    lesson.RuleFor(x => x.Teachers)
                        .NotEmpty()
                        .ForEach(teachers => teachers.ChildRules(teacher =>
                        {
                            teacher.RuleFor(x => x.IdTeacher).NotEmpty();
                        }));
                }));
        }
    }
}