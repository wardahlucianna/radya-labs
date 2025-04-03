using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Session.Validator
{
    public class AddSessionValidator : AbstractValidator<AddSessionRequest>
    {
        public AddSessionValidator()
        {
            RuleFor(model => model.IdGradePathway)
                .NotEmpty()
                .ForEach(gradePathways => gradePathways.ChildRules(gradePathway =>
                {
                    gradePathway.RuleFor(x => x).NotEmpty();
                }));

            RuleFor(model => model.IdDay)
                .NotEmpty()
                .ForEach(days => days.ChildRules(day =>
                {
                    day.RuleFor(x => x).NotEmpty();
                }));

            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Alias).NotEmpty();
            
            RuleFor(x => x.DurationInMinutes).NotEmpty();

            RuleFor(x => x.StartTime).NotEmpty();
            
            RuleFor(x => x.EndTime).NotEmpty();
        }
    }
}
