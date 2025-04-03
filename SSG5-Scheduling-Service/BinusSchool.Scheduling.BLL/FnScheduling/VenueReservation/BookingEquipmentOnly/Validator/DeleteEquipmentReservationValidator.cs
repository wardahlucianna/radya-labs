using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Validator
{
    public class DeleteEquipmentReservationValidator : AbstractValidator<DeleteEquipmentReservationRequest>
    {
        public DeleteEquipmentReservationValidator()
        {
            RuleFor(x => x.EquipmentReservationRequestMappings).NotNull();
            RuleFor(x => x.IdSchool).NotEmpty();

            When(x => x.EquipmentReservationRequestMappings != null, () =>
            {
                RuleForEach(x => x.EquipmentReservationRequestMappings)
                .SetValidator(new DeleteEquipmentReservationMappingValidator());
            });
        }
    }

    public class DeleteEquipmentReservationMappingValidator : AbstractValidator<DeleteEquipmentReservationRequest_Mapping>
    {
        public DeleteEquipmentReservationMappingValidator()
        {
            RuleFor(x => x.IdMappingEquipmentReservation).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
        }
    }
}
