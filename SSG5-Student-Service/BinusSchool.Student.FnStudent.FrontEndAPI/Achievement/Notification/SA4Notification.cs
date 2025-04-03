using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Student.FnStudent.Achievement.Notification
{
    public class SA4Notification : FunctionsNotificationHandler
    {
        private string _userName = "", _schoolName = "", _url = "";
        private StudentAchievementNotificationResult _studentAchievementResult;
        private Dictionary<string, object> _notificationData;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SA4Notification> _logger;

        public SA4Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<SA4Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
            base("SA4", notificationManager, configuration, logger)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
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
            var Object = KeyValues.FirstOrDefault(e => e.Key == "studentAchievementResult").Value;
            _studentAchievementResult = JsonConvert.DeserializeObject<StudentAchievementNotificationResult>(JsonConvert.SerializeObject(Object));
            _url = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/studentachievement/myachievement";

            var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            GeneratedTitle = titleTemplate(_notificationData);

            _notificationData = new Dictionary<string, object>
            {

            };

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

                if (IdUserRecipients is null || !IdUserRecipients.Any())
                {
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");
                    return;
                }

                var sendEmailTasks = new List<Task>();

                var students = await _dbContext.Entity<MsStudent>()
                            .Where(x => IdUserRecipients.Contains(x.Id))
                            .Select(x => new
                            {
                                Id = x.Id,
                                DisplayName = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                                EmailAddress = new EmailAddress(x.BinusianEmailAddress, NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName))
                            })
                            .ToListAsync(CancellationToken);

                var msSchool = await _dbContext.Entity<MsSchool>().FirstOrDefaultAsync(x => x.Id == IdSchool, CancellationToken);

                foreach (var idUser in IdUserRecipients)
                {
                    if (!EnsureAnyEmails(students.Where(x => x.Id == idUser).Select(x => x.EmailAddress)))
                        continue;

                    var userEmail = students.Where(x => x.Id == idUser).Select(x => x.EmailAddress).FirstOrDefault();

                    _userName = students.Select(x => x.DisplayName).FirstOrDefault();
                    _schoolName = msSchool.Name;

                    _notificationData = new Dictionary<string, object>
                    {
                        { "Student", _userName.TrimStart().TrimEnd() },
                        { "VerifyingTeacher", $"{_studentAchievementResult.VerifyingTeacher} - {_studentAchievementResult.IdVerifyingTeacher}" },
                        { "AcademicYear", _studentAchievementResult.AcademicYear },
                        { "Semester", _studentAchievementResult.Semester },
                        { "AchievementName", _studentAchievementResult.AchievementName },
                        { "AchievementCategory", _studentAchievementResult.AchievementCategory },
                        { "FocusArea", _studentAchievementResult.FocusArea },
                        { "DateCompletion", _studentAchievementResult.DateCompletion.ToString("dd MMMM yyyy")},
                        { "Point", _studentAchievementResult.Point },
                        { "CreatedBy", _studentAchievementResult.CreatedBy },
                        { "ApprovalNotes", _studentAchievementResult.ApprovalNotes },
                        { "Status", _studentAchievementResult.Status },
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
                            new Personalization { Tos = new List<EmailAddress> { userEmail } }
                        }
                    };

                    if (NotificationTemplate.EmailIsHtml)
                        message.HtmlContent = emailBody;
                    else
                        message.PlainTextContent = emailBody;

                    sendEmailTasks.Add(NotificationManager.SendEmail(message));
                }

                // send batch email
                await Task.WhenAll(sendEmailTasks);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        protected override async Task SendPushNotification()
        {

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
                    { "VerifyingTeacher" , ""}
                };


                if (IdUserRecipients is null || !IdUserRecipients.Any())
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");

                var student = await _dbContext.Entity<MsStudent>()
                    .Where(x => IdUserRecipients.Contains(x.Id))
                    .Select(x => new
                    {
                        Id = x.Id,
                        EmailAddress = new EmailAddress(x.BinusianEmailAddress, NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName))
                    })
                    .ToListAsync(CancellationToken);

                var saveNotificationTasks = new List<Task>();
                foreach (var idUser in IdUserRecipients)
                {
                    var studentEmail = student.Where(x => x.Id == idUser).Select(x => x.EmailAddress).FirstOrDefault();

                    if (!EnsureAnyEmails(student.Where(x => x.Id == idUser).Select(x => x.EmailAddress)))
                    {
                        GeneratedContent = $"{_studentAchievementResult.VerifyingTeacher} has declined your achievement. Please check on My Achievement menu.";
                    }
                    else
                    {
                        _notificationData["VerifyingTeacher"] = _studentAchievementResult.VerifyingTeacher;
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
            catch (System.Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}
