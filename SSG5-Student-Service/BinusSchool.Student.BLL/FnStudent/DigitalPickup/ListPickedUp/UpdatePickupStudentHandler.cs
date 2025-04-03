using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp;
using BinusSchool.Student.FnStudent.DigitalPickup.Validator;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Common.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Data.Api.Util.FnNotification;
using Microsoft.Extensions.Configuration;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Exceptions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;

namespace BinusSchool.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class UpdatePickupStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;
        private readonly ISendGrid _sendGridApi;
        private readonly IConfiguration _configuration;
        private readonly INotificationTemplate _notificationTemplate;
        private readonly string _notificationScenario = "DP2";

        public UpdatePickupStudentHandler(IStudentDbContext dbContext, 
            IMachineDateTime dateTime,
            IConfiguration configuration,
            ISendGrid sendGridApi,
            INotificationTemplate notificationTemplate)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _configuration = configuration;
            _sendGridApi = sendGridApi;
            _notificationTemplate = notificationTemplate;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdatePickupStudentRequest, UpdatePickupStudentValidator>();

            var updateData = await _dbContext.Entity<TrDigitalPickup>()
                .Include(x => x.AcademicYear).ThenInclude(x => x.MsSchool)
                .Include(x => x.Student)
                .Where(x => param.IdDigitalPickup.Contains(x.Id))
                .Join(_dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                    .Include(x => x.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom),
                    pickup => new { pickup.IdStudent, pickup.IdAcademicYear, pickup.Semester },
                    student => new { student.IdStudent, student.Homeroom.Grade.MsLevel.IdAcademicYear, student.Semester },
                    (pickup, student) => new { pickup, student })
                .Select(x => new
                {
                    pickup = x.pickup,
                    IdLevel = x.student.Homeroom.Grade.IdLevel,
                    IdGrade = x.student.Homeroom.IdGrade,
                    Homeroom = new ItemValueVm
                    {
                        Id = x.student.IdHomeroom,
                        Description = x.student.Homeroom.Grade.Code + x.student.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                    },
                    StudentName = NameUtil.GenerateFullName(x.pickup.Student.FirstName, x.pickup.Student.MiddleName, x.pickup.Student.LastName)

                })
                .ToListAsync(CancellationToken);

            updateData = updateData.Distinct().ToList();

            var result = new List<UpdatePickupStudentResult>();
            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                foreach (var item in updateData)
                {
                    //Status = 1 => Pickup | Status = 2 => Cancel
                    if (param.Status == 1)
                    {
                         if(item.pickup.PickupTime == null)
                        {
                            item.pickup.PickupTime = _dateTime.ServerTime;
                            _dbContext.Entity<TrDigitalPickup>().Update(item.pickup);

                            //Send Email
                            var res = await SendEmail(new SendEmailRequest
                            {
                                IdSchool = item.pickup.AcademicYear.IdSchool,
                                SchoolName = item.pickup.AcademicYear.MsSchool.Description,
                                IdStudent = item.pickup.IdStudent,
                                StudentName = item.StudentName,
                                Grade = item.Homeroom.Description,
                                Date = item.pickup.Date.ToString("dd MMMM yyyy")
                            });

                            //if (!res)
                            //{
                            //    throw new Exception("Failed to send email");
                            //}
                        };
                    }
                    else if(param.Status == 2)
                    {
                        if(item.pickup.PickupTime != null)
                        {
                            item.pickup.PickupTime = null;
                            _dbContext.Entity<TrDigitalPickup>().Update(item.pickup);
                        }
                    }

                    var singleResult = new UpdatePickupStudentResult();
                    singleResult.IdDigitalPickUp = item.pickup.Id;
                    singleResult.IdSchool = item.pickup.AcademicYear.IdSchool;
                    singleResult.IdLevel = item.IdLevel;
                    singleResult.IdGrade = item.IdGrade;
                    singleResult.Homeroom = item.Homeroom;
                    singleResult.StudentName = item.StudentName;
                    singleResult.QRScanTime = item.pickup.QrScanTime;
                    singleResult.PickupTime = item.pickup.PickupTime;
                    result.Add(singleResult);
                }
                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
            }
            catch(Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2(result as object);
        }

        private async Task<bool> SendEmail(SendEmailRequest getData)
        {
            if (getData != null)
            {
                var hostUrl = _configuration.GetSection("ClientApp:Web:Host").Get<string>();

                if (string.IsNullOrEmpty(hostUrl))
                    throw new BadRequestException("Host url is not set");

                var getemailtemplateapi = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
                {
                    IdScenario = _notificationScenario,
                    IdSchool = getData.IdSchool
                });

                var getEmailTemplate = getemailtemplateapi.Payload;
                var emailSubjectTemplate = getEmailTemplate.Title;
                var emailHtmlTemplate = getEmailTemplate.Email;

                //Replace Subject
                emailSubjectTemplate = emailSubjectTemplate
                    .Replace("{{IdStudent}}", getData.IdStudent)
                    .Replace("{{StudentName}}", getData.StudentName)
                    .Replace("{{PickupDate}}", getData.Date)
                    .Replace("{{Grade}}", getData.Grade);


                //Replace Content
                emailHtmlTemplate = emailHtmlTemplate
                    .Replace("{{StudentName}}", getData.StudentName)
                    .Replace("{{Grade}}", getData.Grade)
                    .Replace("{{PickupDate}}", getData.Date)
                    .Replace("{{SchoolName}}", getData.SchoolName);

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
                    IdSchool = getData.IdSchool,
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

        private class SendEmailRequest
        {
            public string IdSchool { get; set; }
            public string SchoolName { get; set; }
            public string IdStudent { get; set; }
            public string StudentName { get; set; }
            public string Grade { get; set; }
            public string Date { get; set; }
        }
    }
}
