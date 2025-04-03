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
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.SendEmail
{
    public class SendEmailBulkVenueReservationApprovalHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly ISendGrid _sendGrid;
        private readonly INotificationTemplate _notificationTemplate;
        private readonly string _notifScenario = "VR_VRA1";

        public SendEmailBulkVenueReservationApprovalHandler(ISchedulingDbContext context, ISendGrid sendGrid, INotificationTemplate notificationTemplate)
        {
            _context = context;
            _sendGrid = sendGrid;
            _notificationTemplate = notificationTemplate;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<SendEmailBulkVenueReservationApprovalRequest>();

            await SendEmailBulkVenueReservationApproval(request);

            return Request.CreateApiResult2();
        }

        public async Task SendEmailBulkVenueReservationApproval(SendEmailBulkVenueReservationApprovalRequest request)
        {
            foreach (var item in request.IdBooking)
            {
                var getVenueReservation = await _context.Entity<TrVenueReservation>()
                    .Include(a => a.User)
                    .Include(a => a.VenueMapping.Venue)
                    .Include(a => a.VenueMapping.Floor.Building)
                    .Include(a => a.VenueMapping.AcademicYear.School)
                    .Where(a => a.Id == item)
                    .FirstOrDefaultAsync(CancellationToken);

                var getVenueMappingApprover = await _context.Entity<MsVenueMappingApproval>()
                    .Include(a => a.Staff)
                    .Where(a => a.IdBinusian == getVenueReservation.IdUserAction
                        && a.IdVenueMapping == getVenueReservation.IdVenueMapping)
                    .FirstOrDefaultAsync(CancellationToken);

                var getEmailTemplate = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
                {
                    IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                    IdScenario = _notifScenario
                });

                var emailTemplate = getEmailTemplate.Payload;
                var emailSubject = emailTemplate.Title;
                var emailBody = emailTemplate.Email;

                // replace email subject
                emailSubject = emailSubject
                    .Replace("{{EventName}}", getVenueReservation.EventDescription);

                // replace email body
                emailBody = emailBody
                    .Replace("{{RequesterName}}", getVenueReservation.User.DisplayName.Trim())
                    .Replace("{{Status}}", VenueReservationApprovalStatusHelper.ApprovalStatus(getVenueReservation.Status))
                    .Replace("{{RequesterData}}", getVenueReservation.IdUser + " - " + getVenueReservation.User.DisplayName + " (" + getVenueReservation.User.Email + ")")
                    .Replace("{{BuildingVenueData}}", getVenueReservation.VenueMapping.Floor.Building.Description + " - " + getVenueReservation.VenueMapping.Venue.Description)
                    .Replace("{{ScheduleDate}}", getVenueReservation.ScheduleDate.ToString("dd MMMM yyyy"))
                    .Replace("{{BookingTime}}", getVenueReservation.StartTime + " - " + getVenueReservation.EndTime)
                    .Replace("{{EventName}}", getVenueReservation.EventDescription)
                    .Replace("{{StatusColor}}", getVenueReservation.Status == 1 ? "#00b050" : "#ff0000")
                    .Replace("{{Status}}", VenueReservationApprovalStatusHelper.ApprovalStatus(getVenueReservation.Status))
                    .Replace("{{RejectionReason}}", getVenueReservation.RejectionReason ?? "-")
                    .Replace("{{VenueApproverName}}", getVenueMappingApprover != null ? NameUtil.GenerateFullName(getVenueMappingApprover?.Staff?.FirstName, getVenueMappingApprover?.Staff?.LastName) : "SYSTEM")
                    .Replace("{{SchoolName}}", getVenueReservation.VenueMapping.AcademicYear.School.Name);

                // send email list
                var toList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                var userIds = new List<string> { getVenueReservation.IdUser, getVenueReservation.UserIn };

                var getStaff = await _context.Entity<MsStaff>()
                    .Where(a => userIds.Contains(a.IdBinusian))
                    .Select(a => new SendSendGridEmailRequest_AddressBuilder
                    {
                        Address = a.BinusianEmailAddress,
                        DisplayName = NameUtil.GenerateFullName(a.FirstName, a.LastName)
                    })
                    .ToListAsync(CancellationToken);

                toList.AddRange(getStaff);

                var sendEmail = await _sendGrid.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration()
                    {
                        ToList = toList,
                        CcList = ccList,
                        BccList = bccList
                    },
                    MessageContent = new SendSendGridEmailRequest_MessageContent()
                    {
                        Subject = emailSubject,
                        BodyHtml = emailBody
                    }
                });
            }
        }
    }
}
