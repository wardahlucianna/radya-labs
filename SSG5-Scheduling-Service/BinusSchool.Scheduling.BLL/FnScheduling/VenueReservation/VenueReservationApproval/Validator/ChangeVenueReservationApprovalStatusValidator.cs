using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Validator
{
    public class ChangeVenueReservationApprovalStatusValidator : AbstractValidator<List<ChangeVenueReservationApprovalStatusRequest>>
    {
        public ChangeVenueReservationApprovalStatusValidator()
        {
            RuleFor(model => model)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(a => a.IdUser)
                        .NotEmpty();

                    data.RuleFor(a => a.IdBooking)
                        .NotEmpty();

                    data.RuleFor(a => a.ApprovalStatus)
                        .NotEmpty();

                    data.RuleFor(a => a.RejectionReason)
                        .NotEmpty()
                        .When(a => a.ApprovalStatus == VenueApprovalStatus.Rejected)
                        .WithMessage("Rejection reason must be fill");
                }));
        }
    }
}
