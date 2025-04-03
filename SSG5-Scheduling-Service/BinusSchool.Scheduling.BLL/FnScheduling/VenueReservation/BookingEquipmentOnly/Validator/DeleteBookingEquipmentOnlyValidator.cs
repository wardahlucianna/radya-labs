using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Validator
{
    public class DeleteBookingEquipmentOnlyValidator : AbstractValidator<DeleteBookingEquipmentOnlyRequest>
    {
        public DeleteBookingEquipmentOnlyValidator()
        {
            RuleFor(x => x.IdMappingEquipmentReservations).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
