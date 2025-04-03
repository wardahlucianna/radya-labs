using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using FluentEmail.Core;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Validator
{
    public class SaveEquipmentReservationValidator : AbstractValidator<SaveEquipmentReservationRequest>
    {
        public SaveEquipmentReservationValidator()
        {
            RuleFor(x => x.EquipmentReservationMapping).NotNull().NotEmpty();

            When(x => x.EquipmentReservationMapping != null, () =>
            {
                RuleForEach(y => y.EquipmentReservationMapping)
                    .SetValidator(new SaveEquipmentReservationMapppingValidator());
            });
        }
    }

    public class SaveEquipmentReservationMapppingValidator : AbstractValidator<SaveEquipmentReservationRequest_Mapping>
    {
        public SaveEquipmentReservationMapppingValidator()
        {
            RuleFor(x => x.ScheduleStartDate).NotEmpty();
            RuleFor(x => x.ScheduleEndDate).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.EventDescription).NotEmpty();

            When(x => x.ListEquipment != null, () =>
            {
                RuleForEach(y => y.ListEquipment)
                    .SetValidator(new SaveEquipmentReservationEquipmentValidator());
            });

        }
    }

    public class SaveEquipmentReservationEquipmentValidator : AbstractValidator<SaveEquipmentReservationRequest_Equipment>
    {
        public SaveEquipmentReservationEquipmentValidator()
        {
            RuleFor(x => x.IdEquipment).NotEmpty();
            RuleFor(x => x.EquipmentBorrowingQty).NotEmpty().GreaterThan(0);
        }
    }
}
