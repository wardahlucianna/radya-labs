﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationOverlapCheckResponse
    {
        public List<GetVenueReservationOverlapCheckResponse_Equipment> Equipments { get; set; }
        public List<GetVenueReservationOverlapCheckResponse_Booking> Bookings { get; set; }
    }

    public class GetVenueReservationOverlapCheckResponse_Equipment
    {
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string EquipmentName { get; set; }
        public string IdEquipment { get; set; }
        public int CurrentStockAvailable { get; set; }
        public int BorrowingQty { get; set; }
    }

    public class GetVenueReservationOverlapCheckResponse_Booking
    {
        public string IdVenueReservation { get; set; }
        public string IdVenue { get; set; }
        public string Requester { get; set; }
        public string EventDescription { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? PreparationTime { get; set; }
        public int? CleanUpTime { get; set; }
        public string Note { get; set; }
        public bool Overlapping { get; set; }
        public bool NeedApproval { get; set; }
        public GetVenueReservationOverlapCheckResponse_Booking_File FileUpload { get; set; }
        public List<GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment> AdditionalEquipments { get; set; }
    }

    public class GetVenueReservationOverlapCheckResponse_Booking_File
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string Url { get; set; }
    }

    public class GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment
    {
        public string IdEquipment { get; set; }
        public int EquipmentBorrowingQty { get; set; }
    }
}
