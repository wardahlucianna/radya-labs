using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore.Validator
{
    public class UpdateExtracurricularScoreLegendValidator2 : AbstractValidator<UpdateExtracurricularScoreLegendRequest2>
    {
        public UpdateExtracurricularScoreLegendValidator2()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.ScoreLegendCategories)
                   .NotEmpty()
                   .ForEach(data => data.ChildRules(data =>
                    {
                       
                        data.RuleFor(x => x.Description).NotEmpty();
                        data.RuleFor(x => x.ScoreLegends)
                            .NotEmpty()
                            .ForEach(data2 => data2.ChildRules(data2 =>
                            {
                                data2.RuleFor(x => x.Score).NotEmpty();
                                data2.RuleFor(x => x.Description).NotEmpty();
                            }));
                    }));

          

        }
    }
}
