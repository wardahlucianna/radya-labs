using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Validator
{
    public class SaveBookingEquipmentOnlyValidator : AbstractValidator<SaveBookingEquipmentOnlyRequest>
    {
        public SaveBookingEquipmentOnlyValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.EventDescription).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.StartTime).NotEmpty();
            RuleFor(x => x.EndTime).NotEmpty();
            RuleFor(x => x.ListEquipment).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
