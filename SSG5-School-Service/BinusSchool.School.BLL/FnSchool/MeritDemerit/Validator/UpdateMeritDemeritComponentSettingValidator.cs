using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    public class UpdateMeritDemeritComponentSettingValidator :  AbstractValidator<UpdateMeritDemeritComponentSettingRequest>
    {
        public UpdateMeritDemeritComponentSettingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("School code cannot empty");
            RuleFor(x => x.MeritDemeritComponentSetting).NotEmpty().WithMessage("Merit and demerit component setting cannot empty");
        }
    }
}
