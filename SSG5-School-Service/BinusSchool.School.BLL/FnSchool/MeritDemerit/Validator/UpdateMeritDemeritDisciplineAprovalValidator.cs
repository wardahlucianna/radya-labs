using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    public class UpdateMeritDemeritDisciplineAprovalValidator : AbstractValidator<UpdateMeritDemeritDisciplineAprovalRequest>
    {
        public UpdateMeritDemeritDisciplineAprovalValidator()
        {
            RuleFor(x => x.IdAcademic).NotEmpty().WithMessage("School code cannot empty");
            RuleFor(x => x.MeritDemeritApprovalSetting).NotEmpty().WithMessage("Merit and demerit discipline Approval cannot empty");
        }


    }
}
