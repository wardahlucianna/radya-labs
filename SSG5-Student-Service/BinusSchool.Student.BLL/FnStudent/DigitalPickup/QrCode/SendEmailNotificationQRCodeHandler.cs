using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Data.Api.Util.FnNotification;
using Microsoft.Extensions.Configuration;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;

namespace BinusSchool.Student.FnStudent.DigitalPickup.QrCode
{
    public class SendEmailNotificationQRCodeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISendGrid _sendGridApi;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly INotificationTemplate _notificationTemplate;
        private readonly string _notificationScenario = "DP1";

        public SendEmailNotificationQRCodeHandler(
            IConfiguration configuration,
            IStudentDbContext dbContext,
            ISendGrid sendGridApi,
            INotificationTemplate notificationTemplate
        )
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _sendGridApi = sendGridApi;
            _notificationTemplate = notificationTemplate;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<SendEmailNotificationQRCodeRequest>(nameof(SendEmailNotificationQRCodeRequest.IdDigitalPickupQrCode));

            var res = await SendEmail(param);

            if (res)
            {
                return Request.CreateApiResult2();
            }
            throw new Exception("Failed to send email");
        }

        private async Task<bool> SendEmail(SendEmailNotificationQRCodeRequest param)
        {

            var getData = await _dbContext.Entity<MsDigitalPickupQrCode>()
                .Include(x => x.AcademicYear).ThenInclude(x => x.MsSchool)
                .Include(x => x.Grade)
                .Include(x => x.Student)
                .Where(x => x.Id == param.IdDigitalPickupQrCode)
                .FirstOrDefaultAsync(CancellationToken);

            if(getData != null)
            {
                var hostUrl = _configuration.GetSection("ClientApp:Web:Host").Get<string>();

                if (string.IsNullOrEmpty(hostUrl))
                    throw new BadRequestException("Host url is not set");

                var getemailtemplateapi = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
                {
                    IdScenario = _notificationScenario,
                    IdSchool = getData.AcademicYear.IdSchool
                });

                var getEmailTemplate = getemailtemplateapi.Payload;
                var emailSubjectTemplate = getEmailTemplate.Title;
                var emailHtmlTemplate = getEmailTemplate.Email;

                //Replace Subject
                emailSubjectTemplate = emailSubjectTemplate
                    .Replace("{{IdStudent}}", getData.IdStudent)
                    .Replace("{{StudentName}}", NameUtil.GenerateFullName(getData.Student.FirstName, getData.Student.MiddleName, getData.Student.LastName))
                    .Replace("{{Grade}}", getData.Grade.Description);

                //Replace Content
                emailHtmlTemplate = emailHtmlTemplate
                    .Replace("{{StudentName}}", NameUtil.GenerateFullName(getData.Student.FirstName, getData.Student.MiddleName, getData.Student.LastName))
                    .Replace("{{ActiveDate}}", FormatDate(getData.ActiveDate))
                    .Replace("{{IdDigitalPickup}}", getData.Id)
                    .Replace("{{IdStudent}}", getData.IdStudent)
                    .Replace("{{Grade}}", getData.Grade.Description)
                    .Replace("{{SchoolName}}", getData.AcademicYear.MsSchool.Description);

                //Send Email List
                var toList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                var GetParentEmail = await _dbContext.Entity<MsStudentParent>()
                    .Include(x => x.Parent)
                    .Where(x => x.IdStudent == getData.IdStudent)
                    .Select(x => new SendSendGridEmailRequest_AddressBuilder
                    {
                        Address = x.Parent.PersonalEmailAddress,
                        DisplayName = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName)
                    })
                    .ToListAsync(CancellationToken);

                toList.AddRange(GetParentEmail);

                var sendEmail = await _sendGridApi.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = getData.AcademicYear.IdSchool,
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration()
                    {
                        ToList = toList,
                        CcList = ccList,
                        BccList = bccList
                    },
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = emailSubjectTemplate,
                        BodyHtml = emailHtmlTemplate
                    }
                });

                return sendEmail.Payload.IsSuccess;
            }

            return false; //If data is null
        }

        private static string FormatDate(DateTime date)
        {
            int day = date.Day;

            string suffix;
            if (day % 10 == 1 && day != 11)
                suffix = "st";
            else if (day % 10 == 2 && day != 12)
                suffix = "nd";
            else if (day % 10 == 3 && day != 13)
                suffix = "rd";
            else
                suffix = "th";

            string formattedDate = $"{date.ToString("MMMM")} {day}<sup>{suffix}</sup>, {date.Year}";
            return formattedDate;
        }

    }
}
