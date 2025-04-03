using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterGroup.Validator
{
    public class UpdateMasterGroupValidator : AbstractValidator<UpdateMasterGroupRequest>
    {
        public UpdateMasterGroupValidator()
        {
            // Only update status
            When(x => string.IsNullOrEmpty(x.Group.Name), () =>
            {
                RuleFor(x => x.Status).NotEmpty();
            });

            // Update whole data
            When(x => !string.IsNullOrEmpty(x.Group.Name), () =>
            {
                RuleFor(x => x.IdSchool).NotEmpty();
                RuleFor(x => x.Group.Name).NotEmpty();
                RuleFor(x => x.Status).Empty();
            });
        }
    }
}
