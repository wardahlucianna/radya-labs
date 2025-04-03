using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScoreComponent.Validator
{
    public class AddExtracurricularScoreComponentValidator2 : AbstractValidator<AddExtracurricularScoreComponentRequest2>
    {
        public AddExtracurricularScoreComponentValidator2()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.ScoreComponentCategories)
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(x => x.Description).NotEmpty();
                    data.RuleFor(y => y.ScoreComponents)
                    .NotEmpty()
                    .ForEach(data1 => data1.ChildRules(data1 =>
                    {
                        data1.RuleFor(x => x.Description).NotEmpty();
                        data1.RuleFor(x => x.OrderNumber).NotEmpty();
                    }));
                }));          
        }
    }
}
