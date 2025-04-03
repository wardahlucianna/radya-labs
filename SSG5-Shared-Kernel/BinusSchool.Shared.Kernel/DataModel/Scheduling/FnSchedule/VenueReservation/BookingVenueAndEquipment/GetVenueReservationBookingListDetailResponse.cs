using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationBookingListDetailResponse
    {
        public string IdVenueReservation { get; set; }
        public ItemValueVm Venue { get; set; }
        public ItemValueVm Requester { get; set; }
        public string EventDescription { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Note { get; set; }
        public GetVenueReservationBookingListDetailResponse_File FileUpload { get; set; }
        public GetVenueReservationBookingListDetailResponse_VenueApprovalStatus VenueApprovalStatus { get; set; }
        public int? PreparationTime { get; set; }
        public int? CleanUpTime { get; set; }
        public List<GetVenueReservationBookingListDetailResponse_VenueEquipment> VenueEquipments { get; set; }
        public string IdMappingEquipmentReservation { get; set; }
        public List<GetVenueReservationBookingListDetailResponse_Equipment> Equipments { get; set; }
    }

    public class GetVenueReservationBookingListDetailResponse_File
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string Url { get; set; }
    }

    public class GetVenueReservationBookingListDetailResponse_VenueApprovalStatus
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string RejectionReason { get; set; }
        public bool IsOverlapping { get; set; }
        public string IdUserAction { get; set; }
        public string IdUserActionName { get; set; }
        public List<ItemValueVm> VenueApprovalUser { get; set; }
    }

    public class GetVenueReservationBookingListDetailResponse_VenueEquipment
    {
        public string IdEquipment { get; set; }
        public string EquipmentName { get; set; }
        public int EquipmentBorrowingQty { get; set; }
        public string IdEquipmentType { get; set; }
        public string EquipmentTypeName { get; set; }
    }

    public class GetVenueReservationBookingListDetailResponse_Equipment
    {
        public string IdEquipment { get; set; }
        public string EquipmentName { get; set; }
        public int EquipmentBorrowingQty { get; set; }
        public string IdEquipmentType { get; set; }
        public string EquipmentType { get; set; }
    }
}
