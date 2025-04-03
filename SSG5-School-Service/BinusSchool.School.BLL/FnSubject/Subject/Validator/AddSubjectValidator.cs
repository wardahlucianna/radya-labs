using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BinusSchool.School.FnSubject.Subject.Validator
{
    public class AddSubjectValidator : AbstractValidator<AddSubjectRequest>
    {
        public AddSubjectValidator(IServiceProvider provider)
        {
            var localizer = provider.GetService<IStringLocalizer>();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Subject Short Name");
            
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Subject Name");

            RuleFor(x => x.Grades)
                .NotEmpty()
                .Must(x => x.Select(y => y.IdGrade).Distinct().Count() == x.Count())
                .WithMessage(x => string.Format(localizer["ExNotUnique"], nameof(x.Grades)))
                .ForEach(grades => grades.ChildRules(grade =>
                {
                    grade.RuleFor(x => x.IdGrade).NotEmpty();

                    grade.When(x => x.IdPathways?.Any() ?? false, () =>
                    {
                        grade.RuleFor(x => x.IdPathways)
                            .NotEmpty()
                            .ForEach(ids => ids.ChildRules(id => id.RuleFor(x => x).NotEmpty()));
                    });

                    grade.When(x => x.IdSubjectLevels?.Any() ?? false, () =>
                    {
                        grade.RuleFor(x => x.IdSubjectLevels)
                            .ForEach(ids => ids.ChildRules(id => id.RuleFor(x => x).NotEmpty()));
                    });
                }));

            RuleFor(x => x.IdDepartment).NotEmpty();

            RuleFor(x => x.IdCurriculumType).NotEmpty();

            RuleFor(x => x.IdSubjectType).NotEmpty();

            RuleFor(x => x.MaxSession).GreaterThan(0);

            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.Sessions)
                .NotEmpty()
                .ForEach(sessions => sessions.ChildRules(session =>
                {
                    session.RuleFor(x => x.Length)
                        .NotEmpty()
                        .GreaterThan(0);

                    session.RuleFor(x => x.Count)
                        .NotEmpty()
                        .GreaterThan(0);
                }))
                .SetValidator(x => new TotalSessionValidator(x.MaxSession, localizer));
        }
    }
}
