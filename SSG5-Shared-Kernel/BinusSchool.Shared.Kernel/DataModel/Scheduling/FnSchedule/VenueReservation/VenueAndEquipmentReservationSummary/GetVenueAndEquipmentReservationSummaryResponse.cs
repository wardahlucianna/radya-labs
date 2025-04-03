using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetVenueAndEquipmentReservationSummaryResponse
    {
        public string IdBooking { get; set; }
        public DateTime ScheduleDate { get; set; }
        public GetVenueAndEquipmentReservationSummaryResponse_Time Time { get; set; }
        public ItemValueVm Building { get; set; }
        public ItemValueVm Venue { get; set; }
        public ItemValueVm Requester { get; set; }
        public string Event { get; set; }
        public List<GetVenueAndEquipmentReservationSummaryResponse_Equipment> Equipments { get; set; }
        public List<ItemValueVm> VenueApprovalUsers { get; set; }
        public GetVenueAndEquipmentReservationSummaryResponse_BookingStatus BookingStatus { get; set; }
        public string Note { get; set; }
        public int? PreparationTime { get; set; }
        public int? CleanUpTime { get; set; }
        public GetVenueAndEquipmentReservationSummaryResponse_FileUpload FileUpload { get; set; }
    }

    public class GetVenueAndEquipmentReservationSummaryResponse_Time
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class GetVenueAndEquipmentReservationSummaryResponse_Equipment
    {
        public string IdEquipment { get; set; }
        public string EquipmentName { get; set; }
        public int EquipmentBorrowingQty { get; set; }
        public string IdEquipmentType { get; set; }
        public string EquipmentTypeName { get; set; }
    }

    public class GetVenueAndEquipmentReservationSummaryResponse_BookingStatus
    {
        public int IdBooking { get; set; }
        public string BookingDesc { get; set; }
        public string RejectionReason { get; set; }
        public bool IsOverlapping { get; set; }
    }

    public class GetVenueAndEquipmentReservationSummaryResponse_FileUpload
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal? FileSize { get; set; }
        public string Url { get; set; }
    }
}
