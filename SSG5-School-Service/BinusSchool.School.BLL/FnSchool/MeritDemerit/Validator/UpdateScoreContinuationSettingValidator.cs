using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;


namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    public class UpdateScoreContinuationSettingValidator : AbstractValidator<UpdateScoreContinuationSettingRequest>
    {
        public UpdateScoreContinuationSettingValidator()
        {
            RuleFor(x => x.ScoreContinuationSettings).NotEmpty().WithMessage("Score continuation cannot empty");
        }
    }
}
