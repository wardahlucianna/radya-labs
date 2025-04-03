using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Finance.FnPayment.SendEmail;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.SendEmail;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Finance.FnPayment.SendEmail.Validator;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Finance.FnPayment.SendEmail
{
    public class SendEmailCancelExtracurricularToParentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly ISendGrid _sendGridApi;
        private readonly IOnlineRegistration _scpOnlineRegistration;

        public SendEmailCancelExtracurricularToParentHandler(
            ISchedulingDbContext dbContext,
            ISendGrid sendGridApi,
            IOnlineRegistration scpOnlineRegistration)
        {
            _dbContext = dbContext;
            _sendGridApi = sendGridApi;
            _scpOnlineRegistration = scpOnlineRegistration;
        }

        private string _emailSubject = "Information of Close Elective Registration";
        private string _emailTemplate = @"<head>
    <title></title>
</head>
<body>
    <table role='presentation' cellpadding='0' cellspacing='0' width='100%'>
        <tr>
            <td>
                <table role='presentation' cellpadding='0' cellspacing='0' width='600px' style='margin: 0 auto;'>
                    <tr>
                        <td style='padding: 0 30px 30px 30px;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='margin: 0 auto;'>
                                <tr>
                                    <td style='width: 100%; padding: 50px; vertical-align: top; text-align: center;'>
                                        <img src='https://bssschoolstorage.blob.core.windows.net/school-logo/BinusSchool.png' alt=''>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <p>
                                            <b>Dear Parents/Guardians of {{StudentName}},</b>
                                        </p>
                                        <p>
                                            Please be informed that your chosen Electives: <b>{{ExtracurricularName}}</b> didn’t reach the minimum number of participants and has been closed. You may re-register your child to the other Electives this Academic Year pending available seats.
                                        </p>
                                        <p>
                                            The available Electives are:<br>
                                        <ol>
                                            {{AvailableExtracurricularList}}
                                        </ol>
                                        </p>
                                        <p>
                                            For further info, please contact AcOp.
                                        </p>
                                        <p>
                                            Thank you.
                                        </p>
                                        <p>
                                            Regards,<br>
                                            <b>BINUS SCHOOL {{SchoolName}}</b>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>";

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var paramList = await Request.ValidateBody<List<SendEmailCancelExtracurricularToParentRequest>, SendEmailCancelExtracurricularToParentValidator>();

            var getStudentParentList = await _dbContext.Entity<MsStudentParent>()
                                            .Include(sp => sp.Student)
                                            .Include(sp => sp.Parent)
                                            .Where(x => paramList.Select(y => y.Student.Id).Any(y => y == x.IdStudent))
                                            .ToListAsync(CancellationToken);

            foreach (var param in paramList)
            {
                string emailTemplate = _emailTemplate;

                var getStudentParent = getStudentParentList
                                        .Where(x => x.IdStudent == param.Student.Id)
                                        .ToList();

                // email
                var toListAll = new List<SendSendGridEmailRequest_AddressBuilder>();
                var toPrimaryList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var toAdditionalList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccAdditionalList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccAdditionalList = new List<SendSendGridEmailRequest_AddressBuilder>();

                var toPrimaryListData = getStudentParent
                                        .Select(x => new SendSendGridEmailRequest_AddressBuilder
                                        {
                                            Address = x.Parent.PersonalEmailAddress,
                                            DisplayName = NameUtil.GenerateFullName(x.Parent.FirstName ?? null, x.Parent.MiddleName ?? null, x.Parent.LastName ?? null)
                                        })
                                        .ToList();

                toPrimaryList.AddRange(toPrimaryListData);

                string primaryTo = "";
                foreach (var toPrimary in toPrimaryListData)
                {
                    primaryTo += toPrimary.Address + ";";
                }

                // additional receivers
                string additionalTo = "", additionalCC = "", additionalBCC = "";

                toListAll.AddRange(toPrimaryList);
                toListAll.AddRange(toAdditionalList);

                // add bcc for staff (sementara dibuat sedikit hardcode karena keterbatasan waktu development)
                if (param.School.Id == "1")
                {
                    // Ivan Devara Candra Lukito
                    var staffBcc = await _dbContext.Entity<MsStaff>()
                                    .Where(x => x.IdBinusian == "BN000964322")
                                    .FirstOrDefaultAsync(CancellationToken);

                    if (staffBcc != null)
                    {
                        var tempBcc = new SendSendGridEmailRequest_AddressBuilder
                        {
                            Address = staffBcc.BinusianEmailAddress,
                            DisplayName = null
                        };

                        bccAdditionalList.Add(tempBcc);

                        additionalBCC += tempBcc + ";";
                    }
                }
                if (param.School.Id == "2")
                {
                    // Petra Tauli Pasaribu
                    var staffBcc = await _dbContext.Entity<MsStaff>()
                                    .Where(x => x.IdBinusian == "1360621614")
                                    .FirstOrDefaultAsync(CancellationToken);

                    if (staffBcc != null)
                    {
                        var tempBcc = new SendSendGridEmailRequest_AddressBuilder
                        {
                            Address = staffBcc.BinusianEmailAddress,
                            DisplayName = null
                        };

                        bccAdditionalList.Add(tempBcc);

                        additionalBCC += tempBcc + ";";
                    }
                }
                if (param.School.Id == "3")
                {
                    // Nur Fatiyah
                    var staffBcc = await _dbContext.Entity<MsStaff>()
                                    .Where(x => x.IdBinusian == "BN001253534")
                                    .FirstOrDefaultAsync(CancellationToken);

                    if (staffBcc != null)
                    {
                        var tempBcc = new SendSendGridEmailRequest_AddressBuilder
                        {
                            Address = staffBcc.BinusianEmailAddress,
                            DisplayName = null
                        };

                        bccAdditionalList.Add(tempBcc);

                        additionalBCC += tempBcc + ";";
                    }
                }
                if (param.School.Id == "4")
                {
                    // Edita Prestiwi Arsyta Ayu
                    var staffBcc = await _dbContext.Entity<MsStaff>()
                                    .Where(x => x.IdBinusian == "1360617983")
                                    .FirstOrDefaultAsync(CancellationToken);

                    if (staffBcc != null)
                    {
                        var tempBcc = new SendSendGridEmailRequest_AddressBuilder
                        {
                            Address = staffBcc.BinusianEmailAddress,
                            DisplayName = null
                        };

                        bccAdditionalList.Add(tempBcc);

                        additionalBCC += tempBcc + ";";
                    }
                }

                emailTemplate = emailTemplate
                                        .Replace("{{SchoolName}}", ((param.School.Name) ?? "{{SchoolName}}"))
                                        .Replace("{{StudentName}}", ((param.Student.Name) ?? "{{StudentName}}"))
                                        .Replace("{{ExtracurricularName}}", ((param.Extracurricular.Name) ?? "{{ExtracurricularName}}"));

                var getAvailableExtracurricularApi = await _scpOnlineRegistration.GetExtracurricularListByStudent(new GetExtracurricularListByStudentRequest
                {
                    IdStudent = param.Student.Id
                });

                var getAvailableExtracurricularList = new List<GetExtracurricularListByStudentResult>();

                if (getAvailableExtracurricularApi.Payload == null || getAvailableExtracurricularApi.Payload.Count() == 0)
                    getAvailableExtracurricularList = null;

                getAvailableExtracurricularList = getAvailableExtracurricularApi.Payload.ToList();

                string availableExtracurricularTemplate = "";
                foreach (var getAvailableExtracurricular in getAvailableExtracurricularList)
                {
                    availableExtracurricularTemplate += ("<li>" + getAvailableExtracurricular.Extracurricular.Name + "</li>");
                }

                emailTemplate = emailTemplate
                                    .Replace("{{AvailableExtracurricularList}}", ((availableExtracurricularTemplate) ?? "{{AvailableExtracurricularList}}"));

                // send email
                var sendEmail = await _sendGridApi.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = param.School.Id,
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = _emailSubject,
                        BodyHtml = emailTemplate
                    },
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                    {
                        ToList = toListAll,
                        CcList = ccAdditionalList,
                        BccList = bccAdditionalList
                    }
                });
            }

            return Request.CreateApiResult2();
        }
    }
}
