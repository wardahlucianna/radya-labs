using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.FnStudent.EmergencyAttendance.Notification;
using FluentEmail.Core.Models;
using Google.Apis.Logging;
using HandlebarsDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Persistence.StudentDb.Entities.School;
using SendGrid.Helpers.Mail;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentExitForm.Notification
{
    public class SXT1Notification : FunctionsNotificationHandler
    {
        private string _userName = "", _schoolName = "", _url = "";
        private StudentExitNotificationResult _studentExitResult;
        private IDictionary<string, object> _notificationData;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SXT1Notification> _logger;

        public SXT1Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<SXT1Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
            base("SXT1", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = false,
                    Web = true
                }
            };

            return Task.CompletedTask;
        }

        protected override Task Prepare()
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "studentExitResult").Value;
            _studentExitResult = JsonConvert.DeserializeObject<StudentExitNotificationResult>(JsonConvert.SerializeObject(Object));
            _url = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/studentexitsubmission/detail?id={_studentExitResult.IdStudentExit}";

            var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            GeneratedTitle = titleTemplate(_notificationData);

            var msSchool = _dbContext.Entity<MsSchool>().FirstOrDefault(x => x.Id == IdSchool);
            _schoolName = msSchool.Name;

            return Task.CompletedTask;
        }

        protected override async Task SendEmailNotification()
        {
            try
            {
                if (KeyValues is null)
                {
                    _logger.LogInformation($"Skip sending notification. No data");
                    return;
                }

                if (IdUserRecipients is null)
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");

                var sendEmailTasks = new List<Task>();

                var users = await _dbContext.Entity<MsUser>()
                            .Where(x => IdUserRecipients.Contains(x.Id))
                            .Select(x => new
                            {
                                Id = x.Id,
                                DisplayName = x.DisplayName.TrimStart().TrimEnd(),
                                EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                            })
                            .ToListAsync(CancellationToken);

                _userName = string.Join(", ", users.Select(x => x.DisplayName).ToArray());

                _notificationData = new Dictionary<string, object>
                    {
                        { "Acop", _userName },
                        { "AcademicYear", _studentExitResult.AcademicYear },
                        { "Semester", _studentExitResult.Semester },
                        { "Level", _studentExitResult.Level },
                        { "Grade", _studentExitResult.Grade },
                        { "Homeroom", _studentExitResult.Homeroom },
                        { "BinusianID", _studentExitResult.BinusianId },
                        { "StudentName", _studentExitResult.StudentName },
                        { "Status", _studentExitResult.Status },
                        { "Link" , _url},
                        { "SchoolName", _schoolName.ToUpper() }
                    };

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = NotificationTemplate.Title,
                    Personalizations = new List<Personalization>
                        {
                            new Personalization { Tos =  users.Select(x => x.EmailAddress).ToList()}
                        }
                };

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = emailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));

                // send batch email
                await Task.WhenAll(sendEmailTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            try
            {
                if (KeyValues is null)
                {
                    _logger.LogInformation($"Skip sending notification. No data");
                    return;
                }
                _notificationData = new Dictionary<string, object>
                {
                    { "Parent" , ""}
                };


                if (IdUserRecipients is null)
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");

                var staff = await _dbContext.Entity<MsUser>()
                    .Where(x => IdUserRecipients.Contains(x.Id))
                    .Select(x => new
                    {
                        Id = x.Id,
                        EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                    })
                    .ToListAsync(CancellationToken);

                var saveNotificationTasks = new List<Task>();
                foreach (var idUser in IdUserRecipients)
                {
                    var staffEmail = staff.Where(x => x.Id == idUser).Select(x => x.EmailAddress).FirstOrDefault();

                    if (!EnsureAnyEmails(staff.Where(x => x.Id == idUser).Select(x => x.EmailAddress)))
                    {
                        GeneratedContent = $"{_studentExitResult.ParentName} has submitted student exit data. Please do approval on the Student Exit Submission Menu.";
                    }
                    else
                    {
                        _notificationData["Parent"] = _studentExitResult.ParentName;
                        var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                        GeneratedContent = pushTemplate(_notificationData);

                    }
                    var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                    GeneratedTitle = pushTitle(_notificationData);

                    var idRecepient = new[] { idUser };

                    saveNotificationTasks.Add(NotificationManager.SaveNotification(
                    CreateNotificationHistory(
                        idUserRecipients,
                        isBlast,
                    GeneratedTitle ?? NotificationTemplate.Title,
                    GeneratedContent ?? NotificationTemplate.Push)));
                }
                await Task.WhenAll(saveNotificationTasks);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }
        }

        protected override Task SendPushNotification()
        {
            return Task.CompletedTask;
        }
    }
}
