using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.BlendedLearningProgram.Validator
{
    public class SaveRespondAnswerBLPValidator : AbstractValidator<SaveRespondAnswerBLPRequest>
    {
        public SaveRespondAnswerBLPValidator()
        {
            RuleFor(x => x.IdSurveyCategory).NotEmpty();
            
            RuleFor(x => x.ListQuestionAnswer)
                .NotEmpty()
                .ForEach(servey => servey.ChildRules(servey =>
                {
                    servey.RuleFor(x => x.IsUpdate).NotNull();
                    servey.RuleFor(x => x.IdStudent).NotEmpty();
                    servey.RuleFor(x => x.IdSurveyAnswerMapping).NotEmpty();
                    servey.RuleFor(x => x.IdSurveyQuestionMapping).NotEmpty();
                }));
        }
    }
}
