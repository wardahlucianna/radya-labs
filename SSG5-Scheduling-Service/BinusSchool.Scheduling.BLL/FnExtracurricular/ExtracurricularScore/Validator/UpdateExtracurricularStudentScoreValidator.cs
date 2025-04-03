using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore.Validator
{
    public class UpdateExtracurricularStudentScoreValidator : AbstractValidator<UpdateExtracurricularStudentScoreRequest>
    {
        public UpdateExtracurricularStudentScoreValidator()
        {
            RuleFor(x => x.IdExtracurricular).NotEmpty();

            RuleFor(x => x.StudentScores)
                .NotEmpty()
                .ForEach(data => data.ChildRules(dc =>
                {
                    //dc.When(x => x.IdExtracurricularScoreEntry == null, () => {
                    //    dc.RuleFor(x => x.IdExtracurricularScoreLegend).NotEmpty();
                    //});                                            
                    dc.RuleFor(x => x.IdExtracurricularScoreComponent).NotEmpty();
                    dc.RuleFor(x => x.IdStudent).NotEmpty();
                }));
                   
        }
    }
}
