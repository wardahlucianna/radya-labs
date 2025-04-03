using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScoreComponent.Validator
{
    public class AddExtracurricularScoreComponentValidator : AbstractValidator<AddExtracurricularScoreComponentRequest>
    {
        public AddExtracurricularScoreComponentValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.ScoreComponents)
             .NotEmpty()
             .ForEach(data => data.ChildRules(data =>
             {               
                 data.RuleFor(x => x.Description).NotEmpty();
             }));
        }
    }
}
