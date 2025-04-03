using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesObjective;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectivesObjective.Validator
{
    public class AddElectivesObjectiveValidator : AbstractValidator<AddElectivesObjectiveRequest>
    {
        public AddElectivesObjectiveValidator()
        {
            RuleFor(x => x.IdExtracurricular).NotEmpty();
            RuleFor(x => x.ElectivesObjectives)      
             .ForEach(data => data.ChildRules(data =>
             {
                 data.RuleFor(x => x.Description).NotEmpty();
             }));
        }
    }
}
