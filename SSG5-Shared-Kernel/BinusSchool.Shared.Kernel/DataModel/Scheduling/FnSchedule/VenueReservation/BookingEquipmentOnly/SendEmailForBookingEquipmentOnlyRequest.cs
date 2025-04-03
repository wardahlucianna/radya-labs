using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Bcpg;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class SendEmailForBookingEquipmentOnlyRequest
    {
        public string? RequesterName { get; set; }
        public string UserInputted { get; set; }
        public string BuildingVenue { get; set; }
        public string EventDate { get; set; }
        public string EventTime { get; set; }
        public string EventDescription { get; set; }
        public string Notes { get; set; }
        public string SchoolName { get; set; }
        public string? EquipmentType { get; set; }
        public string Requester { get; set; }
        public string Action { get; set; }
        public string SubjectAction { get; set; }
        public string IdSchool { get; set; }
        public List<SendEmailForBookingEquipmentOnlyRequest_Equipment> EquipmentList { get; set; }
        public string SendToEmail { get; set; }
        public string SendToName { get; set; }
        public List<SendEmailForBookingEquipmentOnlyRequest_PICOwnerEmail>? SendToPIC { get; set; }
    }

    public class SendEmailForBookingEquipmentOnlyRequest_Equipment
    {
        public string EquipmentName { get; set; }
        public string BorrowingQty { get; set; }

    }

    public class SendEmailForBookingEquipmentOnlyRequest_PICOwnerEmail
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsTo { get; set; }
        public bool IsCC { get; set; }
        public bool IsBCC { get; set; }
    }

    public class SendEmailForBookingEquipmentOnlyRequest_EquipmentTemplate
    {
        public string EquipmentTemplate { get; set; } = "<tr><td>{{No}}</td><td>{{EquipmentName}}</td><td class='text-align-right'>{{BorrowingQty}}</td></tr>";
    }
}
