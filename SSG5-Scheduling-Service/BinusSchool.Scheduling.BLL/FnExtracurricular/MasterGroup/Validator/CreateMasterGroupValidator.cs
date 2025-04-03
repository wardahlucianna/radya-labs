using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterGroup.Validator
{
    public class CreateMasterGroupValidator : AbstractValidator<CreateMasterGroupRequest>
    {
        public CreateMasterGroupValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.GroupName).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
        }
    }
}
