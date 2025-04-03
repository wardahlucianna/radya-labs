using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class DeleteEquipmentReservationRequest
    {
        public List<DeleteEquipmentReservationRequest_Mapping> EquipmentReservationRequestMappings { get; set; }
        public string IdSchool { get; set; }
        public string IdUserLogin { get; set; }
    }

    public class DeleteEquipmentReservationRequest_Mapping
    {
        public List<string> IdMappingEquipmentReservation { get; set; }
        public string IdUser { get; set; }
    }
}
