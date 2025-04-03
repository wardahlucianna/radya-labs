using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentStatus.SendEmail;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.StudentStatus.SendEmail
{
    public class SendEmailStudentStatusSpecialNeedFutureAdmission : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly ISendGrid _sendGrid;
        private readonly INotificationTemplate _notificationTemplate;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;

        public SendEmailStudentStatusSpecialNeedFutureAdmission(IStudentDbContext context, ISendGrid sendGrid, INotificationTemplate notificationTemplate, IConfiguration configuration, IMachineDateTime dateTime)
        {
            _context = context;
            _sendGrid = sendGrid;
            _notificationTemplate = notificationTemplate;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<SendEmailStudentStatusSpecialNeedFutureAdmissionRequest>();

            await SendEmail(request);

            return Request.CreateApiResult2();
        }

        public async Task SendEmail(SendEmailStudentStatusSpecialNeedFutureAdmissionRequest request)
        {
            var date = _dateTime.ServerTime.Date;

            var domain = _configuration.GetSection("ClientApp:Web:Host").Get<string>();

            var student = _context.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Student.Id == request.IdStudent
                    && a.Homeroom.Grade.MsLevel.IdAcademicYear == request.IdAcademicYear)
                .FirstOrDefault();

            var studentStatusSpecial = _context.Entity<LtStudentStatusSpecial>()
                .Where(a => a.IdStudentStatusSpecial == request.IdStudentStatusSpecial)
                .FirstOrDefault();

            var principal = _context.Entity<TrNonTeachingLoad>()
                .Include(a => a.MsNonTeachingLoad.TeacherPosition)
                .Where(a => a.MsNonTeachingLoad.TeacherPosition.Description == "Principal"
                    && a.MsNonTeachingLoad.IdAcademicYear == request.IdAcademicYear)
                .AsEnumerable()
                .Where(a => JObject.Parse(a.Data)["Level"]["id"].ToString() == student.Homeroom.Grade.IdLevel)
                .ToList();

            var joinStaff = principal
                .GroupJoin(
                    _context.Entity<MsStaff>().AsEnumerable(),
                    load => load.IdUser,
                    staff => staff.IdBinusian,
                    (load, staffGroup) => new
                    {
                        Load = load,
                        Staffs = staffGroup.ToList()
                    }
                )
                .FirstOrDefault();

            var getEmailTemp = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
            {
                IdSchool = request.IdSchool,
                IdScenario = "FAD1"
            });

            var emailTemp = getEmailTemp.Payload;
            var emailSubject = emailTemp.Title;
            var emailBody = emailTemp.Email;

            emailBody = emailBody
                .Replace("{{GenderName}}", joinStaff.Staffs.FirstOrDefault().GenderName == "Male" ? "Mr." : "Ms.")
                .Replace("{{PrincipalName}}", NameUtil.GenerateFullName(joinStaff.Staffs.FirstOrDefault().FirstName, joinStaff.Staffs.FirstOrDefault().LastName))
                .Replace("{{SchoolName}}", student.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Description)
                .Replace("{{StudentId}}", request.IdStudent)
                .Replace("{{StudentName}}", NameUtil.GenerateFullName(student.Student.FirstName, student.Student.LastName))
                .Replace("{{ClassHomeroom}}", $"{student.Homeroom.Grade.Code} {student.Homeroom.MsGradePathwayClassroom.Classroom.Code}")
                .Replace("{{StudentStatusSpecial}}", $"{studentStatusSpecial.LongDesc} ({studentStatusSpecial.Remarks})")
                .Replace("{{ApprovalLink}}", $"{domain}/StudentOperation/StudentUnderAttention?id={AESCBCEncryptionUtil.EncryptBase64Url($"{student.Homeroom.Grade.MsLevel.IdAcademicYear}#{student.Homeroom.Grade.MsLevel.MsAcademicYear.Description}#{request.IdStudent}#{date}")}");

            var requesterToList = new List<SendSendGridEmailRequest_AddressBuilder>();
            var requesterBccList = new List<SendSendGridEmailRequest_AddressBuilder>();
            var requesterCcList = new List<SendSendGridEmailRequest_AddressBuilder>();

            var staff = await _context.Entity<MsStaff>()
                .Where(a => a.IdBinusian == "0860002090")
                .FirstOrDefaultAsync(CancellationToken);

            var toList = new SendSendGridEmailRequest_AddressBuilder
            {
                Address = joinStaff.Staffs.FirstOrDefault().BinusianEmailAddress,
                DisplayName = NameUtil.GenerateFullName(joinStaff.Staffs.FirstOrDefault().FirstName, joinStaff.Staffs.FirstOrDefault().LastName)
            };

            var bccList = new SendSendGridEmailRequest_AddressBuilder
            {
                Address = "itdevschool@binus.edu",
                DisplayName = "IT Dev School"
            };

            var ccList = new SendSendGridEmailRequest_AddressBuilder
            {
                Address = staff.BinusianEmailAddress,
                DisplayName = NameUtil.GenerateFullName(staff.FirstName, staff.LastName)
            };

            requesterToList.Add(toList);
            requesterBccList.Add(bccList);
            requesterCcList.Add(ccList);

            var sendEmail = await _sendGrid.SendSendGridEmail(new SendSendGridEmailRequest
            {
                IdSchool = request.IdSchool,
                RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                {
                    ToList = requesterToList,
                    BccList = requesterBccList,
                    CcList = requesterCcList,
                },
                MessageContent = new SendSendGridEmailRequest_MessageContent
                {
                    Subject = emailSubject,
                    BodyHtml = emailBody,
                }
            });
        }
    }
}
