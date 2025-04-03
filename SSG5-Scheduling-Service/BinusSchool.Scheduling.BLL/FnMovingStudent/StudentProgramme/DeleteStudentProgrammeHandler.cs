using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnMovingStudent;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Scheduling.FnMovingStudent.StudentProgramme.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnMovingStudent.StudentProgramme
{
    public class DeleteStudentProgrammeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IEmailRecepient _emailRecepient;

        public DeleteStudentProgrammeHandler(ISchedulingDbContext schoolDbContext, IEmailRecepient EmailRecepient)
        {
            _dbContext = schoolDbContext;
            _emailRecepient = EmailRecepient;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteStudentProgrammeRequest, DeleteStudentProgrammeValidator>();
            var predicate = PredicateBuilder.Create<TrStudentProgramme>(x => x.Id==body.id);

            var studentProgramme = await _dbContext.Entity<TrStudentProgramme>()
                                        .Include(e => e.Student)
                                       .Where(predicate)
                                       .FirstOrDefaultAsync(CancellationToken);

            if (studentProgramme == null)
                throw new BadRequestException("Student program is not found");

            studentProgramme.IsActive = false;
            _dbContext.Entity<TrStudentProgramme>().UpdateRange(studentProgramme);

            var studentProgrammeHistory = await _dbContext.Entity<HTrStudentProgramme>()
                                      .Where(e => e.IdStudentProgramme == (studentProgramme.Id))
                                      .ToListAsync(CancellationToken);

            studentProgrammeHistory.ForEach(e => e.IsActive = false);
            _dbContext.Entity<HTrStudentProgramme>().UpdateRange(studentProgrammeHistory);

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region email
            var getEmailRecepient = await _emailRecepient.GetEmailBccAndTos(new GetEmailBccAndTosRequest
            {
                Type=TypeEmailRecepient.StudentProgram
            });

            var EmailRecepient = getEmailRecepient.Payload;

            var emailStudentProgramme = new EmailStudentProgrammeResult
            {
                studentId = studentProgramme.IdStudent,
                studentName = NameUtil.GenerateFullName(studentProgramme.Student.FirstName, studentProgramme.Student.MiddleName, studentProgramme.Student.LastName),
                homeroom = body.homeroom,
                oldProgramme = studentProgramme.Programme.GetDescription(),
                effectiveDate = Convert.ToDateTime(studentProgramme.StartDate).ToString("dd MMM yyyy"),
            };

            emailStudentProgramme.idUserBcc = EmailRecepient.Bcc;
            emailStudentProgramme.idUserTo = EmailRecepient.Tos;

            if (KeyValues.ContainsKey("emailStudentProgramme"))
            {
                KeyValues.Remove("emailStudentProgramme");
            }
            KeyValues.Add("emailStudentProgramme", emailStudentProgramme);
            var Notification = ESP2Notification(KeyValues, AuthInfo);

            #endregion
            return Request.CreateApiResult2();
        }

        public static string ESP2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "emailStudentProgramme").Value;
            var emailStudentProgramme = JsonConvert.DeserializeObject<EmailStudentProgrammeResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ESP2")
                {
                    IdRecipients = emailStudentProgramme.idUserTo,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }


      
    }
}
