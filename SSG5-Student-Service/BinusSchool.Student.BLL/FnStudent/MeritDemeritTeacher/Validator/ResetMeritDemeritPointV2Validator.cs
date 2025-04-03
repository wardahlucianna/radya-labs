using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class ResetMeritDemeritPointV2Validator : AbstractValidator<ResetMeritDemeritPointV2Request>
    {
        public ResetMeritDemeritPointV2Validator()
        {
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("School cannot empty");
        }
    }
}
