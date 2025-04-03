using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Extensions;
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
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail
{
    public class SendEmailBulkOverlapStatus : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly ISendGrid _sendGrid;
        private readonly INotificationTemplate _notificationTemplate;
        private string _overlapsTemp = "<tr>\r\n    <td>{{No}}</td>\r\n    <td>{{TeacherName}}</td>\r\n    <td>{{BuildingDesc}}</td>\r\n    <td>{{VenueDesc}}</td>\r\n    <td>{{ClassDate}}</td>\r\n    <td>{{ClassTime}}</td>\r\n    <td>{{SubjectDesc}}</td>\r\n</tr>";
        private string _overlapsData = "";

        public SendEmailBulkOverlapStatus(ISchedulingDbContext context, ISendGrid sendGrid, INotificationTemplate notificationTemplate)
        {
            _context = context;
            _sendGrid = sendGrid;
            _notificationTemplate = notificationTemplate;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<SendEmailBulkOverlapStatusRequest>();

            await SendEmail(request);

            return Request.CreateApiResult2();
        }

        public async Task SendEmail(SendEmailBulkOverlapStatusRequest request)
        {
            var getSpecialRoleVenue = await _context.Entity<MsSpecialRoleVenue>()
                .Where(a => a.AllSuperAccess == true
                    && a.CanOverrideAnotherReservation == true
                    && a.Role.IdSchool == request.IdSchool)
                .ToListAsync();

            var getUser = await _context.Entity<MsUserRole>()
                .Include(a => a.User)
                .Where(a => a.Role.IdSchool == request.IdSchool)
                .ToListAsync();

            var joinData = from srv in getSpecialRoleVenue
                           join u in getUser on srv.IdRole equals u.IdRole
                           select u;

            var admin = joinData
                .Select(a => new VenueAdmin
                {
                    AdminName = a.User.DisplayName.Trim(),
                    AdminEmail = a.User.Email,
                }).ToList();

            foreach (var item in request.Overlaps)
            {
                var getVenueReserve = await _context.Entity<TrVenueReservation>()
                    .Include(a => a.User)
                    .Include(a => a.VenueMapping.Venue)
                    .Include(a => a.VenueMapping.Floor.Building)
                    .Include(a => a.VenueMapping.AcademicYear.School)
                    .Where(a => a.Id == item.IdBooking)
                    .FirstOrDefaultAsync();

                var getTemplate = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
                {
                    IdSchool = getVenueReserve.VenueMapping.AcademicYear.IdSchool,
                    IdScenario = "VR_BVE2"
                });

                // Venue Admin
                var emailTemplate = getTemplate.Payload;
                var subject = emailTemplate.Title;
                var body = emailTemplate.Email;

                // Overlaps Data
                if (!item.Overlaps.Any())
                {
                    _overlapsTemp = _overlapsTemp
                        .Replace("{{No}}", "-")
                        .Replace("{{TeacherName}}", "-")
                        .Replace("{{BuildingDesc}}", "-")
                        .Replace("{{VenueDesc}}", "-")
                        .Replace("{{ClassDate}}", "-")
                        .Replace("{{ClassTime}}", "-")
                        .Replace("{{SubjectDesc}}", "-");

                    _overlapsData = _overlapsTemp;
                }
                else
                {
                    var number = 1;
                    var _listOverlaps = new StringBuilder();
                    var rawOverlaps = "";

                    foreach (var overlap in item.Overlaps)
                    {
                        rawOverlaps = _overlapsTemp
                            .Replace("{{No}}", number.ToString())
                            .Replace("{{TeacherName}}", overlap.Teacher)
                            .Replace("{{BuildingDesc}}", overlap.Building)
                            .Replace("{{VenueDesc}}", overlap.Venue)
                            .Replace("{{ClassDate}}", overlap.Date.ToString("dd MMMM yyyy"))
                            .Replace("{{ClassTime}}", $"{overlap.Start.Hours:D2}:{overlap.Start.Minutes:D2} - {overlap.End.Hours:D2}:{overlap.End.Minutes:D2}")
                            .Replace("{{SubjectDesc}}", overlap.Subject);

                        _listOverlaps.Append(rawOverlaps);
                        number++;
                    }

                    _overlapsData = _listOverlaps.ToString();
                }

                // subject
                subject = subject
                    .Replace("{{EventName}}", getVenueReserve.EventDescription);

                var rawBody = "";

                foreach (var venueAdmin in admin)
                {
                    // body
                    rawBody = body
                        .Replace("{{VenueAdminName}}", venueAdmin.AdminName)
                        .Replace("{{RequesterData}}", getVenueReserve.User.DisplayName.Trim() + " - " + getVenueReserve.IdUser + " (" + getVenueReserve.User.Email + ")")
                        .Replace("{{BuildingVenueData}}", getVenueReserve.VenueMapping.Floor.Building.Description + " - " + getVenueReserve.VenueMapping.Venue.Description)
                        .Replace("{{ScheduleDate}}", getVenueReserve.ScheduleDate.ToString("dd MMMM yyyy"))
                        .Replace("{{BookingTime}}", getVenueReserve.StartTime + " - " + getVenueReserve.EndTime)
                        .Replace("{{EventName}}", getVenueReserve.EventDescription)
                        .Replace("{{OverlapsData}}", _overlapsData)
                        .Replace("{{SchoolName}}", getVenueReserve.VenueMapping.AcademicYear.School.Name);

                    // send email to venue admin
                    var toList = new List<SendSendGridEmailRequest_AddressBuilder>()
                    {
                        new SendSendGridEmailRequest_AddressBuilder
                        {
                            Address = venueAdmin.AdminEmail,
                            DisplayName = venueAdmin.AdminName
                        }
                    };
                    var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                    var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                    var sendEmailToRequester = await _sendGrid.SendSendGridEmail(new SendSendGridEmailRequest
                    {
                        IdSchool = getVenueReserve.VenueMapping.AcademicYear.IdSchool,
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

        private class VenueAdmin
        {
            public string AdminName { get; set; }
            public string AdminEmail { get; set; }
        }
    }
}
