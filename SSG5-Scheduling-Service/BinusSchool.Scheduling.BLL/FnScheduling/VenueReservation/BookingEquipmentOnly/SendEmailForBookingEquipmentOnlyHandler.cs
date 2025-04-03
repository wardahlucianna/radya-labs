using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Data.Api.Util.FnNotification;
using Microsoft.Extensions.Configuration;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Common.Functions.Queue;
using BinusSchool.Common.Functions.Abstractions;
using SendGrid.Helpers.Mail;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Utils;
using FluentEmail.Core;
using BinusSchool.Persistence.SchedulingDb.Entities.User;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class SendEmailForBookingEquipmentOnlyHandler : FunctionsHttpSingleHandler
    {
        private readonly IConfiguration _configuration;
        private readonly string _notificationScenarioRequester = "VR2";
        private readonly string _notificationScenarioPIC = "VR1";
        protected readonly INotificationManager NotificationManager;
        private readonly ISchedulingDbContext _dbContext;

        private readonly ISendGrid _sendGridEmailApi;

        public SendEmailForBookingEquipmentOnlyHandler(
            IConfiguration configuration, INotificationManager notificationManager, ISchedulingDbContext dbContext, ISendGrid sendGridEmailApi)
        {
            _configuration = configuration;
            NotificationManager = notificationManager;
            _dbContext = dbContext;
            _sendGridEmailApi = sendGridEmailApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<SendEmailForBookingEquipmentOnlyRequest>();

            var res = await SendEmailForBookingEquipmentOnly_ToRequester(param);
            if (!res.isSuccess)
            {
                throw new Exception("Failed to send email to requester");
            }

            res = await SendEmailForBookingEquipmentOnly_ToPICOwner(param);
            if (!res.isSuccess)
            {
                throw new Exception("Failed to send email to requester");
            }

            return Request.CreateApiResult2();
        }

        public async Task<(bool isSuccess, string errorMessage)> SendEmailForBookingEquipmentOnly_ToRequester(SendEmailForBookingEquipmentOnlyRequest param)
        {
            if(param != null)
            {
                var hostUrl = _configuration.GetSection("ClientApp:Web:Host").Get<string>();

                if (string.IsNullOrEmpty(hostUrl))
                    throw new BadRequestException("Host url is not set");

                var getEmailTemplate = await NotificationManager.GetTemplate(param.IdSchool, _notificationScenarioRequester);

                var emailSubjectTemplate = getEmailTemplate.Title;
                var emailHtmlTemplate = getEmailTemplate.Email;

                //Replace Subject
                emailSubjectTemplate = emailSubjectTemplate
                    .Replace("{{SubjectAction}}", param.SubjectAction);

                //Replace Content
                if (param.RequesterName == null)
                {
                    return (false, "Requester Name is null");
                }
                emailHtmlTemplate = emailHtmlTemplate
                    .Replace("{{RequesterName}}", param.RequesterName)
                    .Replace("{{Action}}", param.Action)
                    .Replace("{{UserInputted}}", param.UserInputted)
                    .Replace("{{Requester}}", param.Requester)
                    .Replace("{{BuildingVenue}}", param.BuildingVenue)
                    .Replace("{{EventDate}}", param.EventDate)
                    .Replace("{{EventTime}}", param.EventTime)
                    .Replace("{{EventDescription}}", param.EventDescription)
                    .Replace("{{Notes}}", param.Notes)
                    .Replace("{{SchoolName}}", param.SchoolName);

                var equipmentList = "";
                foreach (var item in param.EquipmentList)
                {
                    var equipmentTemplate = new SendEmailForBookingEquipmentOnlyRequest_EquipmentTemplate().EquipmentTemplate;
                    equipmentTemplate = equipmentTemplate
                        .Replace("{{No}}", (param.EquipmentList.IndexOf(item) + 1).ToString())
                        .Replace("{{EquipmentName}}", item.EquipmentName)
                        .Replace("{{BorrowingQty}}", item.BorrowingQty);
                    equipmentList += equipmentTemplate;
                }
                emailHtmlTemplate = emailHtmlTemplate.Replace("{{EquipmentList}}", equipmentList);

                //Send Email List
                var toList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                if(string.IsNullOrEmpty(param.SendToEmail))
                {
                    return (false, "Email Address is empty");
                }

                var userEmail = new SendSendGridEmailRequest_AddressBuilder
                {
                    Address = param.SendToEmail,
                    DisplayName = param.SendToName
                };
                toList.Add(userEmail);

                //Temporary
                var listBCC = await _dbContext.Entity<MsUser>()
                    .Where(x => x.Id == "1100011573" || x.Id == "1701309620" || x.Id == "BN001489230")
                    .Select(x => new SendSendGridEmailRequest_AddressBuilder
                    {
                        DisplayName = x.DisplayName,
                        Address = x.Email
                    })
                    .ToListAsync(CancellationToken);

                bccList.AddRange(listBCC);

                if(!toList.Any())
                {
                    return (false, "To List is empty");
                }

                var sendEmail = await _sendGridEmailApi.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = param.IdSchool,
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = emailSubjectTemplate,
                        BodyHtml = emailHtmlTemplate
                    },
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                    {
                        ToList = toList,
                        BccList = bccList
                    }
                });

                if (!sendEmail.Payload.IsSuccess)
                {
                    return (false, sendEmail.Payload.ErrorMessage);
                }

            }

            return (true, null);
        }

        public async Task<(bool isSuccess, string errorMessage)> SendEmailForBookingEquipmentOnly_ToPICOwner(SendEmailForBookingEquipmentOnlyRequest param)
        {
            if (param != null)
            {
                var hostUrl = _configuration.GetSection("ClientApp:Web:Host").Get<string>();

                if (string.IsNullOrEmpty(hostUrl))
                    throw new BadRequestException("Host url is not set");

                var getEmailTemplate = await NotificationManager.GetTemplate(param.IdSchool, _notificationScenarioPIC);
                var emailSubjectTemplate = getEmailTemplate.Title;
                var emailHtmlTemplate = getEmailTemplate.Email;

                //Replace Subject
                emailSubjectTemplate = emailSubjectTemplate
                    .Replace("{{SubjectAction}}", param.SubjectAction);

                //Replace Content
                if (param.EquipmentType == null)
                {
                    return (false, "Equipment Type is null");
                }
                emailHtmlTemplate = emailHtmlTemplate
                    .Replace("{{EquipmentType}}", param.EquipmentType)
                    .Replace("{{Action}}", param.Action)
                    .Replace("{{UserInputted}}", param.UserInputted)
                    .Replace("{{Requester}}", param.Requester)
                    .Replace("{{BuildingVenue}}", param.BuildingVenue)
                    .Replace("{{EventDate}}", param.EventDate)
                    .Replace("{{EventTime}}", param.EventTime)
                    .Replace("{{EventDescription}}", param.EventDescription)
                    .Replace("{{Notes}}", param.Notes)
                    .Replace("{{SchoolName}}", param.SchoolName);

                var equipmentList = "";
                foreach (var item in param.EquipmentList)
                {
                    var equipmentTemplate = new SendEmailForBookingEquipmentOnlyRequest_EquipmentTemplate().EquipmentTemplate;
                    equipmentTemplate = equipmentTemplate
                        .Replace("{{No}}", (param.EquipmentList.IndexOf(item) + 1).ToString())
                        .Replace("{{EquipmentName}}", item.EquipmentName)
                        .Replace("{{BorrowingQty}}", item.BorrowingQty);
                    equipmentList += equipmentTemplate;
                }
                emailHtmlTemplate = emailHtmlTemplate.Replace("{{EquipmentList}}", equipmentList);

                //Send Email List
                var toList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                if (param.SendToPIC == null)
                {
                    return (false, "Send to PIC is null");
                }

                foreach (var item in param.SendToPIC)
                {
                    var email = new SendSendGridEmailRequest_AddressBuilder();

                    email.DisplayName = item.Name;
                    email.Address = item.Email;

                    if (string.IsNullOrEmpty(email.Address))
                    {
                        return (false, "Email Address is empty");
                    }

                    if (item.IsCC)
                    {
                        ccList.Add(email);
                    }
                    else if (item.IsTo)
                    {
                        toList.Add(email);
                    }
                    else if (item.IsBCC)
                    {
                        bccList.Add(email);
                    }
                }

                //Temporary
                var listBCC = await _dbContext.Entity<MsUser>()
                    .Where(x => x.Id == "1100011573" || x.Id == "1701309620" || x.Id == "BN001489230")
                    .Select(x => new SendSendGridEmailRequest_AddressBuilder
                    {
                        DisplayName = x.DisplayName,
                        Address = x.Email
                    })
                    .ToListAsync(CancellationToken);

                bccList.AddRange(listBCC);

                if (!toList.Any())
                {
                    return (false, "To List is empty");
                }
                

                var sendEmail = await _sendGridEmailApi.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = param.IdSchool,
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = emailSubjectTemplate,
                        BodyHtml = emailHtmlTemplate
                    },
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                    {
                        ToList = toList,
                        CcList = ccList,
                        BccList = bccList
                    }
                });

                if (!sendEmail.Payload.IsSuccess)
                {
                    return (false, sendEmail.Payload.ErrorMessage);
                }

            }

            return (true, null);
        }
    }
}
