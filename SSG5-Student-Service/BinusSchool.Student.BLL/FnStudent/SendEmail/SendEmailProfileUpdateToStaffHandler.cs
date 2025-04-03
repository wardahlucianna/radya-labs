using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.SendEmail.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.SendEmail
{
    public class SendEmailProfileUpdateToStaffHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly INotificationTemplate _notificationTemplate;
        private readonly ISmtpEmail _smtpEmailApi;
        private readonly IStudent _studentApi;
        private readonly IConfiguration _configuration;

        private readonly string _notificationScenario = "STD1";

        public SendEmailProfileUpdateToStaffHandler(
            IStudentDbContext dbContext,
            IMachineDateTime dateTime,
            INotificationTemplate notificationTemplateApi,
            ISmtpEmail smtpEmailApi,
            IStudent studentApi,
            IConfiguration configuration
            )
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _notificationTemplate = notificationTemplateApi;
            _smtpEmailApi = smtpEmailApi;
            _studentApi = studentApi;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SendEmailProfileUpdateToStaffRequest, SendEmailProfileUpdateToStaffValidator>();

            var hostUrl = _configuration.GetSection("ClientApp:Web:Host").Get<string>();

            if(string.IsNullOrEmpty(hostUrl))
                throw new BadRequestException("Host url is not set");

            var profileChangesDetailApi = await _studentApi.GetStudentInformationChangesHistory(new GetStudentInformationChangesHistoryRequest
            {
                IdStudent = param.IdStudent,
                IdStudentInfoUpdateList = param.IdStudentInfoUpdateList,
                GetAll = true
            });

            var profileChangesDetailList = profileChangesDetailApi.Payload.ToList();

            if(!profileChangesDetailList.Any())
                throw new BadRequestException("No any changes were found");

            // send email for changes by parent only
            if (profileChangesDetailList.FirstOrDefault().IsParentUpdate == 0)
                return Request.CreateApiResult2();

            var studentData = await _dbContext.Entity<MsStudent>()
                                    .Include(x => x.School)
                                    .Where(x => x.Id == param.IdStudent)
                                    .ToListAsync(CancellationToken);

            if (studentData == null || !studentData.Any())
                throw new BadRequestException("Student data is not found");

            var studentGradeDetail = studentData
                                    .GroupJoin(
                                        _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(hs => hs.Homeroom)
                                        .ThenInclude(h => h.MsGradePathwayClassroom)
                                        .ThenInclude(gpc => gpc.Classroom)
                                        .Include(x => x.Homeroom)
                                        .ThenInclude(x => x.Grade)
                                        .ThenInclude(x => x.MsLevel)
                                        .ThenInclude(x => x.MsAcademicYear)
                                        .ThenInclude(x => x.MsSchool)
                                        .Join(_dbContext.Entity<MsPeriod>()
                                            .Where(x => _dateTime.ServerTime >= x.StartDate &&
                                                        _dateTime.ServerTime <= x.EndDate)
                                            .OrderByDescending(x => x.StartDate),
                                            homeroomStudent => new { gradeId = homeroomStudent.Homeroom.Grade.Id, homeroomStudent.Semester },
                                            period => new { gradeId = period.IdGrade, period.Semester },
                                            (homeroomStudent, period) => new { homeroomStudent, period }),
                                        student => student.Id,
                                        homeroomStudent => homeroomStudent.homeroomStudent.IdStudent,
                                        (student, homeroomStudent) => new { student, homeroomStudent }
                                    )
                                    .SelectMany(
                                            x => x.homeroomStudent.DefaultIfEmpty(),
                                            (student, homeroomStudent) => new
                                            {
                                                School = new ItemValueVm()
                                                {
                                                    Id = student.student.IdSchool,
                                                    Description = student.student.School.Description,
                                                },
                                                AcadYear = new CodeWithIdVm()
                                                {
                                                    Id = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Id,
                                                    Code = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Code,
                                                    Description = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                                                },
                                                Level = new CodeWithIdVm()
                                                {
                                                    Id = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.MsLevel.Id,
                                                    Code = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.MsLevel.Code,
                                                    Description = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.MsLevel.Description,
                                                },
                                                Grade = new CodeWithIdVm()
                                                {
                                                    Id = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.Id,
                                                    Code = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.Code,
                                                    Description = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Grade.Description,
                                                },
                                                Semester = homeroomStudent == null ? 0 : homeroomStudent.homeroomStudent.Semester,
                                                Student = new NameValueVm
                                                {
                                                    Id = student.student.Id,
                                                    Name = NameUtil.GenerateFullName(student.student.FirstName, student.student.LastName)
                                                },
                                                Homeroom = new NameValueVm
                                                {
                                                    Id = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Homeroom.Id,
                                                    Name = homeroomStudent == null ? null : string.Format("{0}{1}", homeroomStudent.homeroomStudent.Homeroom.Grade.Code, homeroomStudent.homeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Code)
                                                },
                                                IdHomeroomStudent = homeroomStudent == null ? null : homeroomStudent.homeroomStudent.Id
                                            })
                                    .FirstOrDefault();

            if (studentGradeDetail == null)
                throw new BadRequestException("Student data is not found");

            var getEmailTemplateApi = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
            {
                IdSchool = studentGradeDetail.School.Id,
                IdScenario = _notificationScenario
            });

            var getEmailTemplate = getEmailTemplateApi.Payload;

            if(getEmailTemplate == null)
                throw new BadRequestException("Email template is not found");

            var emailHtmlTemplate = getEmailTemplate.Email;
            var tableDataTemplate = @"
                                            <tr>
                                                <td style='border: 1px solid; padding: 0 5px; vertical-align: middle; text-align: left;'>
                                                    {{FieldName}}
                                                </td>
                                                <td style='border: 1px solid; padding: 0 5px; vertical-align: middle; text-align: left;'>
                                                    {{OldValue}}
                                                </td>
                                                <td style='border: 1px solid; padding: 0 5px; vertical-align: middle; text-align: left;'>
                                                    {{NewValue}}
                                                </td>
                                                <td style='border: 1px solid; padding: 0 5px; vertical-align: middle; text-align: center;'>
                                                    {{RequestedDate}}<br>{{RequestedTime}}
                                                </td>
                                            </tr>";

            var encryptedIdStudent = EncryptStringUtil.Encrypt(param.IdStudent.Trim());
            var encryptedIsParentUpdate = EncryptStringUtil.Encrypt(profileChangesDetailList.FirstOrDefault().IsParentUpdate.ToString());

            string emailHtmlFinal = emailHtmlTemplate;
            string tableDataFinal = "";

            // build table data
            foreach (var profileChangeDetail in profileChangesDetailList)
            {
                string tempTableDataTemplate = tableDataTemplate;
                tempTableDataTemplate = tempTableDataTemplate
                                            .Replace("{{FieldName}}", profileChangeDetail.FieldName)
                                            .Replace("{{OldValue}}", profileChangeDetail.OldValue)
                                            .Replace("{{NewValue}}", profileChangeDetail.NewValue)
                                            .Replace("{{RequestedDate}}", profileChangeDetail.ActionDate.HasValue ? profileChangeDetail.ActionDate.Value.ToString("MMM dd, yyyy") : "N/A")
                                            .Replace("{{RequestedTime}}", profileChangeDetail.ActionDate.HasValue ? profileChangeDetail.ActionDate.Value.ToString("hh:mm:ss tt") : "");

                tableDataFinal += tempTableDataTemplate;
            }

            emailHtmlFinal = emailHtmlFinal
                                .Replace("{{IdStudent}}", studentGradeDetail.Student.Id)
                                .Replace("{{StudentName}}", studentGradeDetail.Student.Name)
                                .Replace("{{HomeroomName}}", string.IsNullOrWhiteSpace(studentGradeDetail.Homeroom.Name) ? "-" : studentGradeDetail.Homeroom.Name)
                                .Replace("{{WebHost}}", hostUrl.Trim().ToString())
                                .Replace("{{EncryptedIdStudent}}", encryptedIdStudent)
                                .Replace("{{EncryptedIsParentUpdate}}", encryptedIsParentUpdate)
                                .Replace("[[TableData]]", tableDataFinal);


            // send email
            var toList = new List<SendSmtpEmailRequest_AddressBuilder>();
            var ccList = new List<SendSmtpEmailRequest_AddressBuilder>();
            var bccList = new List<SendSmtpEmailRequest_AddressBuilder>();

            var getReceivers = await _dbContext.Entity<MsStudentEmailAdditionalReceiver>()
                                .Include(x => x.User)
                                .Where(x => x.Scenario == _notificationScenario &&
                                            x.IdSchool == studentGradeDetail.School.Id)
                                .ToListAsync();

            toList.AddRange(
                getReceivers
                    .Where(x => x.AddressType.ToUpper() == "TO" && !string.IsNullOrWhiteSpace(x.User.Email))
                    .Select(x => new SendSmtpEmailRequest_AddressBuilder
                    {
                        Address = x.User.Email,
                        DisplayName = x.User.DisplayName
                    })
                    .ToList()
                );

            ccList.AddRange(
                getReceivers
                    .Where(x => x.AddressType.ToUpper() == "CC" && !string.IsNullOrWhiteSpace(x.User.Email))
                    .Select(x => new SendSmtpEmailRequest_AddressBuilder
                    {
                        Address = x.User.Email,
                        DisplayName = x.User.DisplayName
                    })
                    .ToList()
                );

            bccList.AddRange(
                getReceivers
                    .Where(x => x.AddressType.ToUpper() == "BCC" && !string.IsNullOrWhiteSpace(x.User.Email))
                    .Select(x => new SendSmtpEmailRequest_AddressBuilder
                    {
                        Address = x.User.Email,
                        DisplayName = x.User.DisplayName
                    })
                    .ToList()
                );

            var sendSmtpEmailApi = await _smtpEmailApi.SendSmtpEmail(new SendSmtpEmailRequest
            {
                IdSchool = studentGradeDetail.School.Id,
                MessageContent = new SendSmtpEmailRequest_MessageContent
                {
                    Subject = getEmailTemplate.Title,
                    BodyHtml = emailHtmlFinal
                },
                RecepientConfiguration = new SendSmtpEmailRequest_RecepientConfiguration
                {
                    ToList = toList,
                    CcList = ccList,
                    BccList = bccList
                }
            });

            return Request.CreateApiResult2();
        }
    }
}
