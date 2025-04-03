using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterGroup.Validator
{
    public class DeleteMasterGroupValidator : AbstractValidator<DeleteMasterGroupRequest>
    {
        public DeleteMasterGroupValidator()
        {
            RuleFor(x => x.GroupId).NotEmpty();
        }
    }
}
