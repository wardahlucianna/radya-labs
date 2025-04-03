using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail
{
    public class SendEmailBulkVenueAndEquipmentReservationForPIC : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly ISendGrid _sendGrid;
        private readonly INotificationTemplate _notificationTemplate;
        private string _equipmentTemplate = "<tr><td>{{No}}</td><td>{{EquipmentName}}</td><td>{{EquipmentQty}}</td></tr>";
        private string _notAvailEquipmentTemplate = "<tr><td>{{No}}</td><td>{{ScheduleDate}}</td><td>{{EquipmentName}}</td><td>{{BorrowingQty}}</td><td>{{StockQty}}</td></tr>";
        private string _layoutLink = "<a href='{{LayoutLink}}'>Click here to download</a>";
        private string _equipmentData = "";
        private string _notAvailEquipmentData = "";

        public SendEmailBulkVenueAndEquipmentReservationForPIC(ISchedulingDbContext context, ISendGrid sendGrid, INotificationTemplate notificationTemplate)
        {
            _context = context;
            _sendGrid = sendGrid;
            _notificationTemplate = notificationTemplate;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<SendEmailBulkVenueAndEquipmentReservationForPICRequest>();

            await SendEmail(request);

            return Request.CreateApiResult2();
        }

        public async Task SendEmail(SendEmailBulkVenueAndEquipmentReservationForPICRequest request)
        {
            foreach (var item in request.IdBooking)
            {
                var getVenueReservation = await _context.Entity<TrVenueReservation>()
                    .Include(a => a.User)
                    .Include(a => a.VenueMapping.Venue)
                    .Include(a => a.VenueMapping.Floor.Building)
                    .Include(a => a.VenueMapping.AcademicYear.School)
                    .Include(a => a.VenueMapping.ReservationOwner)
                    .Where(a => a.Id == item)
                    .FirstOrDefaultAsync(CancellationToken);

                var getMappingEquipmentReservation = await _context.Entity<TrMappingEquipmentReservation>()
                    .Include(a => a.EquipmentReservations)
                        .ThenInclude(b => b.Equipment)
                        .ThenInclude(b => b.EquipmentType)
                        .ThenInclude(b => b.ReservationOwner)
                    .Where(a => a.IdVenueReservation == item)
                    .FirstOrDefaultAsync(CancellationToken);

                var getUserIn = await _context.Entity<MsUser>()
                    .Where(a => a.Id == getVenueReservation.UserIn)
                    .FirstOrDefaultAsync(CancellationToken);

                var getPICOwnerEmailTemplate = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
                {
                    IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                    IdScenario = "VR_BVE_PIC"
                });

                // PIC Owner
                var picOwnerEmailTemplate = getPICOwnerEmailTemplate.Payload;
                var picOwnerSubject = picOwnerEmailTemplate.Title;
                var picOwnerBody = picOwnerEmailTemplate.Email;

                // Equipment Mapping Data
                if (getMappingEquipmentReservation == null && !request.CanceledEquipments.Any() || !getMappingEquipmentReservation.EquipmentReservations.Where(a => a.IsActive == true).Any() && !request.CanceledEquipments.Any())
                {
                    _equipmentTemplate = _equipmentTemplate
                        .Replace("{{No}}", "-")
                        .Replace("{{EquipmentName}}", "-")
                        .Replace("{{EquipmentQty}}", "-");

                    _equipmentData = _equipmentTemplate;
                }
                else
                {
                    var number = 1;
                    var _listEquipment = new StringBuilder();
                    var rawEquipment = "";
                    if (request.CanceledEquipments.Any())
                    {
                        foreach (var equipment in request.CanceledEquipments)
                        {
                            rawEquipment = _equipmentTemplate
                                .Replace("{{No}}", number.ToString())
                                .Replace("{{EquipmentName}}", equipment.EquipmentName)
                                .Replace("{{EquipmentQty}}", equipment.EquipmentQty.ToString());

                            _listEquipment.Append(rawEquipment);
                            number++;
                        }
                    }
                    else
                    {
                        foreach (var equipment in getMappingEquipmentReservation.EquipmentReservations.Where(a => a.IsActive == true))
                        {
                            rawEquipment = _equipmentTemplate
                                .Replace("{{No}}", number.ToString())
                                .Replace("{{EquipmentName}}", equipment.Equipment.EquipmentName)
                                .Replace("{{EquipmentQty}}", equipment.EquipmentBorrowingQty.ToString());

                            _listEquipment.Append(rawEquipment);
                            number++;
                        }
                    }

                    _equipmentData = _listEquipment.ToString();
                }

                // Equipment Not Avail Data
                if (request.NotAvailableEquipments.Any())
                {
                    var no = 1;
                    var _listNotAvailEquipment = new StringBuilder();
                    var rawNotAvailEquipment = "";

                    foreach (var equipment in request.NotAvailableEquipments)
                    {
                        rawNotAvailEquipment = _notAvailEquipmentTemplate
                            .Replace("{{No}}", no.ToString())
                            .Replace("{{ScheduleDate}}", equipment.ScheduleDate)
                            .Replace("{{EquipmentName}}", equipment.EquipmentName)
                            .Replace("{{BorrowingQty}}", equipment.BorrowingQty)
                            .Replace("{{StockQty}}", equipment.StockQty);

                        _listNotAvailEquipment.Append(rawNotAvailEquipment);
                        no++;
                    }

                    _notAvailEquipmentData = _listNotAvailEquipment.ToString();
                }
                else
                {
                    _notAvailEquipmentTemplate = _notAvailEquipmentTemplate
                            .Replace("{{No}}", "-")
                            .Replace("{{ScheduleDate}}", "-")
                            .Replace("{{EquipmentName}}", "-")
                            .Replace("{{BorrowingQty}}", "-")
                            .Replace("{{StockQty}}", "-");

                    _notAvailEquipmentData = _notAvailEquipmentTemplate;
                }

                // Mapping Layout
                _layoutLink = _layoutLink
                    .Replace("{{LayoutLink}}", getVenueReservation.URL ?? "#");

                // recurrence
                var recurrenceValue = request.Recurrence
                    .Select(a => a.ToString("dddd, dd MMMM yyyy"))
                    .ToList();

                string recurrence = recurrenceValue.Any()
                    ? string.Join(",<br>", recurrenceValue)
                    : "-";

                #region pic owner email process
                // subject
                picOwnerSubject = picOwnerSubject
                    .Replace("{{EventName}}", getVenueReservation.EventDescription);

                // body
                picOwnerBody = picOwnerBody
                    .Replace("{{BookStatus}}", request.Action)
                    .Replace("{{UserIn}}", getUserIn.DisplayName.Trim() + " - " + getUserIn.Id + " (" + getUserIn.Email + ")")
                    .Replace("{{RequesterData}}", getVenueReservation.User.DisplayName.Trim() + " - " + getVenueReservation.IdUser + " (" + getVenueReservation.User.Email + ")")
                    .Replace("{{BuildingVenueData}}", getVenueReservation.VenueMapping.Floor.Building.Description + " - " + getVenueReservation.VenueMapping.Venue.Description)
                    .Replace("{{ScheduleDate}}", request.Recurrence.Any()
                        ? request.Recurrence.FirstOrDefault().ToString("dd MMMM yyyy") + " - " + request.Recurrence.LastOrDefault().ToString("dd MMMM yyyy")
                        : getVenueReservation.ScheduleDate.ToString("dd MMMM yyyy"))
                    .Replace("{{BookingTime}}", getVenueReservation.StartTime + " - " + getVenueReservation.EndTime)
                    .Replace("{{BookingTimeFull}}", getVenueReservation.StartTime.Subtract(TimeSpan.FromMinutes(getVenueReservation.PreparationTime ?? 0)) + " - " + getVenueReservation.EndTime.Add(TimeSpan.FromMinutes(getVenueReservation.CleanUpTime ?? 0)))
                    .Replace("{{PrepTime}}", getVenueReservation.PreparationTime.ToString() ?? "0")
                    .Replace("{{CleanUpTime}}", getVenueReservation.CleanUpTime.ToString() ?? "0")
                    .Replace("{{EventName}}", getVenueReservation.EventDescription)
                    .Replace("{{LayoutData}}", getVenueReservation.URL == null ? "No Layout" : _layoutLink)
                    .Replace("{{RecurrenceValue}}", recurrence)
                    .Replace("{{Note}}", string.IsNullOrEmpty(getVenueReservation.Notes) ? "-" : getVenueReservation.Notes ?? "-")
                    .Replace("{{EquipmentData}}", _equipmentData)
                    .Replace("{{EquipmentNotAvailData}}", _notAvailEquipmentData)
                    .Replace("{{SchoolName}}", getVenueReservation.VenueMapping.AcademicYear.School.Name);

                // send email to venue requester
                var PICToList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var PICBccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var PICCcList = new List<SendSendGridEmailRequest_AddressBuilder>();

                var ownerEmails = new List<MsReservationOwnerEmail>();

                if (getMappingEquipmentReservation != null)
                {
                    var equipmentReservationOwnerIds = getMappingEquipmentReservation.EquipmentReservations
                        .Where(b => b.IsActive)
                        .Select(b => b.Equipment.EquipmentType.IdReservationOwner)
                        .ToList();

                    ownerEmails = await _context.Entity<MsReservationOwnerEmail>()
                        .Where(a => a.IdReservationOwner == getVenueReservation.VenueMapping.IdReservationOwner
                            || equipmentReservationOwnerIds.Contains(a.IdReservationOwner))
                        .ToListAsync(CancellationToken);
                }
                else
                {
                    ownerEmails = await _context.Entity<MsReservationOwnerEmail>()
                        .Where(a => a.IdReservationOwner == getVenueReservation.VenueMapping.IdReservationOwner)
                        .ToListAsync(CancellationToken);
                }

                PICToList = ownerEmails
                    .Where(a => a.IsOwnerEmailTo == true)
                    .Select(a => new SendSendGridEmailRequest_AddressBuilder
                    {
                        Address = a.OwnerEmail,
                        DisplayName = a.ReservationOwner.OwnerName
                    }).ToList();

                PICBccList = ownerEmails
                    .Where(a => a.IsOwnerEmailBCC == true)
                    .Select(a => new SendSendGridEmailRequest_AddressBuilder
                    {
                        Address = a.OwnerEmail,
                        DisplayName = a.ReservationOwner.OwnerName
                    }).ToList();

                PICCcList = ownerEmails
                    .Where(a => a.IsOwnerEmailCC == true)
                    .Select(a => new SendSendGridEmailRequest_AddressBuilder
                    {
                        Address = a.OwnerEmail,
                        DisplayName = a.ReservationOwner.OwnerName
                    }).ToList();

                var sendEmailToPIC = await _sendGrid.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                    {
                        ToList = PICToList,
                        CcList = PICCcList,
                        BccList = PICBccList
                    },
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = picOwnerSubject,
                        BodyHtml = picOwnerBody
                    }
                });
                #endregion
            }
        }

        private class EquipmentList
        {
            public string ScheduleDate { get; set; }
            public string EquipmentName { get; set; }
            public string BorrowingQty { get; set; }
            public string StockQty { get; set; }
        }
    }
}
