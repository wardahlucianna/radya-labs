using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment.Validator
{
    public class UpdateStudentEnrollmentValidator : AbstractValidator<UpdateStudentEnrollmentRequest>
    {
        public UpdateStudentEnrollmentValidator(IServiceProvider provider)
        {
            var localizer = provider.GetService<IStringLocalizer>();

            RuleFor(x => x.IdHomeroom).NotEmpty();

            RuleFor(x => x.Enrolls)
                .ForEach(enrolls => enrolls.ChildRules(enroll =>
                {
                    enroll.RuleFor(x => x.IdStudent).NotEmpty();

                    enroll.RuleFor(x => x.Lessons)
                        .Must(x => x?.Select(y => y.IdLesson).Distinct().Count() == x?.Count())
                        .WithMessage(x => string.Format(localizer["ExNotUnique"], nameof(x.Lessons)))
                        .ForEach(lessons => lessons.ChildRules(lesson =>
                        {
                            lesson.RuleFor(x => x.IdLesson).NotEmpty();
                        }));
                }));
        }
    }
}
