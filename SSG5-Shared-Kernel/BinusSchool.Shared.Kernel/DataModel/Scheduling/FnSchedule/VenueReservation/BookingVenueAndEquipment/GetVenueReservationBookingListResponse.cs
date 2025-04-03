using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationBookingListResponse
    {
        public string IdBooking { get; set; }
        public DateTime ScheduleDate { get; set; }
        public ItemValueVm Venue { get; set; }
        public GetVenueReservationBookingListResponse_Time Time { get; set; }
        public int? PreparationTime { get; set; }
        public int? CleanUpTime { get; set; }
        public string EventName { get; set; }
        public ItemValueVm Requester { get; set; }
        public GetVenueReservationBookingListResponse_Status BookingStatus { get; set; }
        public List<GetVenueReservationBookingListResponse_Equipment> Equipments { get; set; }
        public GetVenueReservationBookingListResponse_Action Action { get; set; }
    }

    public class GetVenueReservationBookingListResponse_Time
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class GetVenueReservationBookingListResponse_Action
    {
        public bool CanDelete { get; set; }
        public bool CanEdit { get; set; }
        public GetVenueReservationBookingListResponse_Action_PrepTime CanEditPrepTime { get; set; }
    }

    public class GetVenueReservationBookingListResponse_Action_PrepTime
    {
        public bool IsVisible { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class GetVenueReservationBookingListResponse_Status
    {
        public int IdBooking { get; set; }
        public string BookingDesc { get; set; }
        public string RejectionReason { get; set; }
        public bool IsOverlapping { get; set; }
    }

    public class GetVenueReservationBookingListResponse_Equipment
    {
        public string IdEquipment { get; set; }
        public string EquipmentName { get; set; }
        public int EquipmentBorrowingQty { get; set; }
        public string IdEquipmentType { get; set; }
        public string EquipmentTypeName { get; set; }
    }
}
