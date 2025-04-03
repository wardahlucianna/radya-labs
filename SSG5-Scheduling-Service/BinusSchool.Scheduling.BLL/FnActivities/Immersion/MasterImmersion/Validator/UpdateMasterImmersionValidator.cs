using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using FluentValidation;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion.Validator
{
    public class UpdateMasterImmersionValidator : AbstractValidator<UpdateMasterImmersionRequest>
    {
        public UpdateMasterImmersionValidator()
        {
            RuleFor(x => x.IdImmersion).NotEmpty();
            RuleFor(x => x.IdGradeList).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.Destination).NotEmpty();
            RuleFor(x => x.IdImmersionPeriod).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x.IdBinusianPIC).NotEmpty();
            RuleFor(x => x.PICEmail).NotEmpty();
            RuleFor(x => x.PICPhone).NotEmpty();
            RuleFor(x => x.MinParticipant).NotNull();
            RuleFor(x => x.MaxParticipant).NotNull();
            RuleFor(x => x.IdCurrency).NotEmpty();
            RuleFor(x => x.IdImmersionPaymentMethod).NotEmpty();
            RuleFor(x => x.RegistrationFee).NotNull();
            RuleFor(x => x.TotalCost).NotNull();
        }
    }
}
