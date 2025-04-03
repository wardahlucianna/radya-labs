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
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.SendEmail;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.SendEmail
{
    public class SendEmailBulkVenueReservationApprovalForApprover : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly INotificationTemplate _notificationTemplate;
        private readonly ISendGrid _sendGrid;
        private string _layoutLink = "<a href='{{LayoutLink}}'>Click here to download</a>";
        private string _bookTemplate = "<tr><td>{{ScheduleDate}}</td><td>{{BookingTime}}</td><td>{{PrepTime}}</td><td>{{CleanUpTime}}</td></tr>";
        private string _bookData = "";

        public SendEmailBulkVenueReservationApprovalForApprover(ISchedulingDbContext context, INotificationTemplate notificationTemplate, ISendGrid sendGrid)
        {
            _context = context;
            _notificationTemplate = notificationTemplate;
            _sendGrid = sendGrid;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<SendEmailBulkVenueReservationApprovalForApproverRequest>();

            await SendEmail(request);

            return Request.CreateApiResult2();
        }

        public async Task SendEmail(SendEmailBulkVenueReservationApprovalForApproverRequest request)
        {
            foreach (var book in request.IdBooking)
            {
                var getVenueReservation = await _context.Entity<TrVenueReservation>()
                    .Include(a => a.User)
                    .Include(a => a.VenueMapping.Venue)
                    .Include(a => a.VenueMapping.Floor.Building)
                    .Include(a => a.VenueMapping.AcademicYear.School)
                    .Include(a => a.VenueMapping.ReservationOwner)
                    .Where(a => a.Id == book)
                    .FirstOrDefaultAsync(CancellationToken);

                var getUserIn = await _context.Entity<MsUser>()
                    .Where(a => a.Id == getVenueReservation.UserIn)
                    .FirstOrDefaultAsync(CancellationToken);

                var getVenueMappingApproval = await _context.Entity<MsVenueMappingApproval>()
                    .Include(a => a.Staff)
                    .Where(a => a.IdVenueMapping == getVenueReservation.IdVenueMapping)
                    .ToListAsync(CancellationToken);

                var getEmailTemplate = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
                {
                    IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                    IdScenario = "VR_VRA2"
                });

                var emailTemplate = getEmailTemplate.Payload;
                var subject = emailTemplate.Title;
                var body = emailTemplate.Email;

                // booking data

                if (request.Recurrence.Any())
                {
                    var _listDate = new StringBuilder();
                    var rawBookTemp = "";
                    foreach (var recurrence in request.Recurrence)
                    {
                        rawBookTemp = _bookTemplate
                            .Replace("{{ScheduleDate}}", recurrence.ToString("dd MMMM yyyy"))
                            .Replace("{{BookingTime}}", getVenueReservation.StartTime + " - " + getVenueReservation.EndTime)
                            .Replace("{{PrepTime}}", getVenueReservation.PreparationTime.ToString() ?? "-")
                            .Replace("{{CleanUpTime}}", getVenueReservation.CleanUpTime.ToString() ?? "-");

                        _listDate.Append(rawBookTemp);
                    }
                    _bookData = _listDate.ToString();
                }
                else
                {
                    _bookTemplate = _bookTemplate
                            .Replace("{{ScheduleDate}}", getVenueReservation.ScheduleDate.ToString("dd MMMM yyyy"))
                            .Replace("{{BookingTime}}", getVenueReservation.StartTime + " - " + getVenueReservation.EndTime)
                            .Replace("{{PrepTime}}", getVenueReservation.PreparationTime.ToString() ?? "-")
                            .Replace("{{CleanUpTime}}", getVenueReservation.CleanUpTime.ToString() ?? "-");

                    _bookData = _bookTemplate;
                }

                // Mapping Layout
                _layoutLink = _layoutLink
                    .Replace("{{LayoutLink}}", getVenueReservation.URL ?? "#");

                // subject

                subject = subject
                    .Replace("{{EventName}}", getVenueReservation.EventDescription);

                var rawBody = "";
                // body
                foreach (var approver in getVenueMappingApproval)
                {
                    rawBody = body
                        .Replace("{{ApproverName}}", NameUtil.GenerateFullName(approver.Staff.FirstName, approver.Staff.LastName))
                        .Replace("{{UserIn}}", getUserIn.DisplayName.Trim() + " - " + getUserIn.Id + " (" + getUserIn.Email + ")")
                        .Replace("{{RequesterData}}", getVenueReservation.User.DisplayName.Trim() + " - " + getVenueReservation.IdUser + " (" + getVenueReservation.User.Email + ")")
                        .Replace("{{BuildingVenueData}}", getVenueReservation.VenueMapping.Floor.Building.Description + " - " + getVenueReservation.VenueMapping.Venue.Description)
                        .Replace("{{EventName}}", getVenueReservation.EventDescription)
                        .Replace("{{LayoutData}}", getVenueReservation.URL == null ? "No Layout" : _layoutLink)
                        .Replace("{{ReservationData}}", _bookData)
                        .Replace("{{SchoolName}}", getVenueReservation.VenueMapping.AcademicYear.School.Name);

                    var toList = new List<SendSendGridEmailRequest_AddressBuilder>()
                    {
                        new SendSendGridEmailRequest_AddressBuilder
                        {
                            Address = approver.Staff.BinusianEmailAddress,
                            DisplayName = NameUtil.GenerateFullName(approver.Staff.FirstName, approver.Staff.LastName)
                        }
                    };
                    var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                    var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                    var sendEmailToRequester = await _sendGrid.SendSendGridEmail(new SendSendGridEmailRequest
                    {
                        IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                        RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                        {
                            ToList = toList,
                            CcList = ccList,
                            BccList = bccList
                        },
                        MessageContent = new SendSendGridEmailRequest_MessageContent
                        {
                            Subject = subject,
                            BodyHtml = rawBody
                        }
                    });
                }
            }
        }
    }
}
