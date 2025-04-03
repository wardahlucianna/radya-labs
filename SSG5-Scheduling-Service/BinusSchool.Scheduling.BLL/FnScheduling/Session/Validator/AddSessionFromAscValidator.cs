using System.Collections.Generic;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Session.Validator
{
    public class AddSessionFromAscValidator : AbstractValidator<List<AddSessionFromAscRequest>>
    {
        public AddSessionFromAscValidator()
        {
            RuleFor(model => model)
               .NotEmpty()
               .ForEach(data => data.ChildRules(data =>
               {
                   data.RuleFor(x => x.IdGradePathway).NotEmpty();

                   data.RuleFor(model => model.IdDay).NotEmpty();

                   data.RuleFor(x => x.Name).NotEmpty();

                   data.RuleFor(x => x.Alias).NotEmpty();

                   data.RuleFor(x => x.DurationInMinutes).NotEmpty();

                   data.RuleFor(x => x.StartTime).NotEmpty();

                   data.RuleFor(x => x.EndTime).NotEmpty();

               }));
        }
    }
}
