using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting.Validator
{
    public class UpdateLockerReservationPeriodPolicyValidator : AbstractValidator<IEnumerable<UpdateLockerReservationPeriodPolicyRequest>>
    {
        public UpdateLockerReservationPeriodPolicyValidator()
        {
            RuleForEach(x => x)
                .SetValidator(new UpdateLockerReservationPeriodPolicyRequestValidator());
        }
    }

    public class UpdateLockerReservationPeriodPolicyRequestValidator : AbstractValidator<UpdateLockerReservationPeriodPolicyRequest>
    {
        public UpdateLockerReservationPeriodPolicyRequestValidator()
        {
            RuleFor(x => x.IdLockerReservationPeriod)
                .NotEmpty();
        }
    }
}
