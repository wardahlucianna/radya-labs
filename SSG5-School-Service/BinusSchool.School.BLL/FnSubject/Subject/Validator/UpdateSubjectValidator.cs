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
    public class UpdateSubjectValidator : CodeWithIdVmValidator<UpdateSubjectRequest>
    {
        public UpdateSubjectValidator(IServiceProvider provider)
        {
            var localizer = provider.GetService<IStringLocalizer>();

            When(x => x.IdPathways?.Any() ?? false, () =>
            {
                RuleFor(x => x.IdPathways)
                    .NotEmpty()
                    .ForEach(ids => ids.ChildRules(id => id.RuleFor(x => x).NotEmpty()));
            });

            When(x => x.IdSubjectLevels?.Any() ?? false, () =>
            {
                RuleFor(x => x.IdSubjectLevels)
                    .NotEmpty()
                    .ForEach(ids => ids.ChildRules(id => id.RuleFor(x => x).NotEmpty()));
            });

            RuleFor(x => x.IdDepartment).NotEmpty();

            RuleFor(x => x.IdCurriculumType).NotEmpty();

            RuleFor(x => x.IdSubjectType).NotEmpty();

            RuleFor(x => x.MaxSession).GreaterThan(0);

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
                }));
            //.SetValidator(x => new TotalSessionValidator(x.MaxSession, localizer));
        }
    }
}
