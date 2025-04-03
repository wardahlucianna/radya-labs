using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Common.Comparers;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.LHAndCA;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.LHAndCA.Validator
{
    public class AddAssignLHAndCAValidator : AbstractValidator<AddAssignLHAndCARequest>
    {
        public AddAssignLHAndCAValidator(IServiceProvider provider)
        {
            //var localizer = provider.GetService<IStringLocalizer>();

            RuleFor(x => x.IdGrade).NotEmpty();

            RuleFor(x => x.IdUserLevelHead).NotEmpty();

            RuleFor(x => x.Data)
                .NotEmpty();
                //.Equal(x => x.IdGrade, new JsonDataAssignmentComparer(localizer, "Grade"))
                //.WithMessage(x => string.Format(localizer["ExNotSame"], "Grade", nameof(x.IdGrade)));

            When(x => x.ClassAdvisors?.Any() ?? false, () =>
            {
                RuleFor(x => x.ClassAdvisors)
                    .Must(x => x.Select(y => y.IdClassroom).Distinct().Count() == x.Count())
                    //.WithMessage(x => string.Format(localizer["ExNotUnique"], nameof(x.ClassAdvisors)))
                    .ForEach(cas => cas.ChildRules(ca =>
                    {
                        ca.RuleFor(x => x.IdClassroom).NotEmpty();

                        ca.RuleFor(x => x.IdUserClassAdvisor).NotEmpty();

                        ca.RuleFor(x => x.Data)
                            .NotEmpty();
                            //.Equal(x => x.IdClassroom, new JsonDataAssignmentComparer(localizer, "Classroom"))
                            //.WithMessage(x => string.Format(localizer["ExNotSame"], "Classroom", nameof(x.IdClassroom)));
                    }));
            });
        }
    }
}
