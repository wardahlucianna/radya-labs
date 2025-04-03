using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore.Validator
{
    public class UpdateExtracurricularScoreLegendValidator : AbstractValidator<UpdateExtracurricularScoreLegendRequest>
    {
        public UpdateExtracurricularScoreLegendValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.ScoreLegends)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {                   
                    data.RuleFor(x => x.Score).NotEmpty();
                    data.RuleFor(x => x.Description).NotEmpty();
                }));

        }
    }
}
