using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using FluentValidation;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion.Validator
{
    public class DeleteMasterImmersionValidator : AbstractValidator<DeleteMasterImmersionRequest>
    {
        public DeleteMasterImmersionValidator()
        {
            RuleFor(x => x.IdImmersions).NotEmpty();
        }
    }
}
