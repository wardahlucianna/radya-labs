using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail
{
    public class SendEmailBulkVenueAndEquipmentReservationForUser : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly ISendGrid _sendGrid;
        private readonly INotificationTemplate _notificationTemplate;
        private string _notAvailEquipmentTemplate = "<tr><td>{{No}}</td><td>{{ScheduleDate}}</td><td>{{EquipmentName}}</td><td>{{BorrowingQty}}</td><td>{{StockQty}}</td></tr>";
        private string _equipmentTemplate = "<tr><td>{{No}}</td><td>{{EquipmentName}}</td><td>{{EquipmentQty}}</td></tr>";
        private string _layoutLink = "<a href='{{LayoutLink}}'>Click here to download</a>";
        private string _equipmentData = "";
        private string _notAvailEquipmentData = "";

        public SendEmailBulkVenueAndEquipmentReservationForUser(ISchedulingDbContext context, ISendGrid sendGrid, INotificationTemplate notificationTemplate)
        {
            _context = context;
            _sendGrid = sendGrid;
            _notificationTemplate = notificationTemplate;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<SendEmailBulkVenueAndEquipmentReservationForUserRequest>();

            await SendEmail(request);

            return Request.CreateApiResult2();
        }

        public async Task SendEmail(SendEmailBulkVenueAndEquipmentReservationForUserRequest request)
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

                var getVenueMappingApproval = await _context.Entity<MsVenueMappingApproval>()
                    .Include(a => a.Staff)
                    .Where(a => a.IdVenueMapping == getVenueReservation.IdVenueMapping)
                    .ToListAsync(CancellationToken);

                var getMappingEquipmentReservation = await _context.Entity<TrMappingEquipmentReservation>()
                    .Include(a => a.EquipmentReservations)
                        .ThenInclude(b => b.Equipment)
                    .Where(a => a.IdVenueReservation == item)
                    .FirstOrDefaultAsync(CancellationToken);

                var getUserIn = await _context.Entity<MsUser>()
                    .Where(a => a.Id == getVenueReservation.UserIn)
                    .FirstOrDefaultAsync(CancellationToken);

                var getVenueRequesterEmailTemplate = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
                {
                    IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                    IdScenario = "VR_BVE1"
                });

                // Venue Requester
                var venueRequesterEmailTemplate = getVenueRequesterEmailTemplate.Payload;
                var venueRequesterSubject = venueRequesterEmailTemplate.Title;
                var venueRequesterBody = venueRequesterEmailTemplate.Email;

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

                // mapping approval
                var listApproval = getVenueMappingApproval
                    .Select(a => NameUtil.GenerateFullName(a.Staff.FirstName, a.Staff.LastName))
                    .ToList();

                string approvalName = listApproval.Any()
                    ? string.Join(", ", listApproval)
                    : "-";

                #region venue requester email process
                // subject
                venueRequesterSubject = venueRequesterSubject
                    .Replace("{{EventName}}", getVenueReservation.EventDescription);

                // body
                venueRequesterBody = venueRequesterBody
                    .Replace("{{RequesterName}}", getVenueReservation.User.DisplayName.Trim())
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
                    .Replace("{{ApprovalStatus}}", request.ApprovalStatus == null ? "-" : VenueReservationApprovalStatusHelper.ApprovalStatus((int) request.ApprovalStatus))
                    .Replace("{{ApproverData}}", approvalName)
                    .Replace("{{SchoolName}}", getVenueReservation.VenueMapping.AcademicYear.School.Name);

                // send email to venue requester
                var requesterToList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var requesterBccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var requesterCcList = new List<SendSendGridEmailRequest_AddressBuilder>();

                var userIds = new List<string> { getVenueReservation.IdUser, getVenueReservation.UserIn };

                var getStaff = await _context.Entity<MsStaff>()
                    .Where(a => userIds.Contains(a.IdBinusian))
                    .Select(a => new SendSendGridEmailRequest_AddressBuilder
                    {
                        Address = a.BinusianEmailAddress,
                        DisplayName = NameUtil.GenerateFullName(a.FirstName, a.LastName)
                    })
                    .ToListAsync(CancellationToken);

                requesterToList.AddRange(getStaff);

                var sendEmailToRequester = await _sendGrid.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                    {
                        ToList = requesterToList,
                        CcList = requesterCcList,
                        BccList = requesterBccList
                    },
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = venueRequesterSubject,
                        BodyHtml = venueRequesterBody
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
