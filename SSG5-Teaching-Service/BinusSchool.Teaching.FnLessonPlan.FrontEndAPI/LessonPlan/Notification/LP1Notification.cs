using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Notification
{
    public class LP1Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;

        private readonly ITeachingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LP1Notification> _logger;
        private readonly IMachineDateTime _dateTime;

        public LP1Notification(INotificationManager notificationManager,
            DbContextOptions<TeachingDbContext> options,
            IConfiguration configuration, 
            ILogger<LP1Notification> logger,
            IMachineDateTime dateTime) :
        base("LP1", notificationManager, configuration, logger)
        {
            _dbContext = new TeachingDbContext(options);
            _configuration = configuration;
            _logger = logger;
            _dateTime = dateTime;
        }

        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = true,
                    Web = true
                }
            };

            return Task.CompletedTask;
        }

        protected override async Task Prepare()
        {
            try
            {
                var url = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/lessonplan/lessonplanlist";

                // get data
                var query = (
                       from tlp in _dbContext.Entity<TrLessonPlan>()
                       join mlt in _dbContext.Entity<MsLessonTeacher>() on tlp.IdLessonTeacher equals mlt.Id
                       join ml in _dbContext.Entity<MsLesson>() on mlt.IdLesson equals ml.Id
                       join ms in _dbContext.Entity<MsSubject>() on ml.IdSubject equals ms.Id
                       join mg in _dbContext.Entity<MsGrade>() on ms.IdGrade equals mg.Id
                       join ml2 in _dbContext.Entity<MsLevel>() on mg.IdLevel equals ml2.Id
                       join may in _dbContext.Entity<MsAcademicYear>() on ml2.IdAcademicYear equals may.Id
                       join sch in _dbContext.Entity<MsSchool>() on may.IdSchool equals sch.Id
                       join ms2 in _dbContext.Entity<MsStaff>() on mlt.IdUser equals ms2.IdBinusian
                       join mp in _dbContext.Entity<MsPeriod>() on mg.Id equals mp.IdGrade
                       join mwsd in _dbContext.Entity<MsWeekSettingDetail>() on tlp.IdWeekSettingDetail equals mwsd.Id
                       join msmsl in _dbContext.Entity<MsSubjectMappingSubjectLevel>() on tlp.IdSubjectMappingSubjectLevel equals msmsl.Id into leftMsml
                       from subMsml in leftMsml.DefaultIfEmpty()
                       join msl in _dbContext.Entity<MsSubjectLevel>() on subMsml.IdSubjectLevel equals msl.Id into leftMsl
                       from subMsl in leftMsl.DefaultIfEmpty()
                       join mws in _dbContext.Entity<MsWeekSetting>() on new { A = mp.Id, B = mwsd.IdWeekSetting } equals new { A = mws.IdPeriod, B = mws.Id }
                       where  sch.Id == IdSchool && mwsd.DeadlineDate != null && tlp.Status == "Unsubmitted"  
                       orderby may.Code descending, ml2.Code descending, mg.Description descending, mp.Code descending, ms.Description descending, mwsd.WeekNumber descending

                       select new GetLessonPlanNotificationResult
                       {
                           IdLessonPlan = tlp.Id,
                           IdUser = mlt.IdUser,
                           IdPeriod = mp.Id,
                           IdSubject = ms.Id,
                           IdSubjectLevel = subMsl.Id,
                           IdWeekSettingDetail = mwsd.Id,
                           AcademicYear = may.Description,
                           Level = ml2.Code,
                           Grade = mg.Description,
                           Term = mp.Code,
                           Subject = ms.Description,
                           SubjectLevel = subMsl != null ? subMsl.Code : "-",
                           Periode = "Week " + mwsd.WeekNumber,
                           DeadlineDate = mwsd.DeadlineDate.Value.Date.ToString("dd MMM yyyy"),
                           SchoolName = sch.Name,
                           TeacherName = string.Join(" ", (new string[] { ms2.FirstName, ms2.LastName })),
                           CanUpload =  mwsd.DeadlineDate < _dateTime.ServerTime ? false : true,
                           Link = url,
                       }
                   ).AsQueryable();

                var lessonPlansData = await query.Where(x => x.CanUpload == true).ToListAsync();

                IdUserRecipients = lessonPlansData.Select(x => x.IdUser)
                                      .Distinct()
                                      .ToList();

                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();

                paramTemplateNotification.Add("lessonPlansData", lessonPlansData);

                KeyValues = paramTemplateNotification;

                KeyValues.Add("email", "");

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }
 
        }

        protected override async Task SendPushNotification()
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

                var tokens = await _dbContext.Entity<MsUserPlatform>()
                             .Where(x
                                 => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                                 && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                             .Select(x => new
                             {
                                 x.IdUser,
                                 x.FirebaseToken
                             }
                             )
                             .ToListAsync(CancellationToken);

                var staff = await _dbContext.Entity<MsUser>()
                            .Where(x => IdUserRecipients.Contains(x.Id))
                            .Select(x => new
                            {
                                Id = x.Id,
                                EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                            })
                            .ToListAsync(CancellationToken);

                var sendPushTasks = new List<Task>();
                PushNotificationData["action"] = "LP_LIST";

                _notificationData = new Dictionary<string, object>
                {
                    { "email" , ""}
                };
                foreach (var idReceipment in IdUserRecipients)
                {
                    var tokenByUser = tokens.Where(x => x.IdUser == idReceipment).Select(x => x.FirebaseToken).ToList();

                    if (!EnsureAnyPushTokens(tokenByUser))
                        continue;

                    var staffEmail = staff.Where(x => x.Id == idReceipment).Select(x => x.EmailAddress).FirstOrDefault();
                    if (!EnsureAnyEmails(staff.Where(x => x.Id == idReceipment).Select(x => x.EmailAddress)))
                    {
                        GeneratedContent = "Your lesson plan for this following week has not been uploaded. Click to see more information or check your email registered in this app";
                    }
                    else
                    {
                        _notificationData["email"] = staffEmail.Email;
                        var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                        GeneratedContent = pushTemplate(_notificationData);
                    }

                    var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                    GeneratedTitle = pushTitle(_notificationData);

                    // NOTE: create MulticastMessage object to send push notification
                    var message = new MulticastMessage
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = GeneratedTitle,
                            Body = GeneratedContent
                        },
                        Tokens = tokenByUser,
                        Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                    };

                    // send push notification
                    sendPushTasks.Add(NotificationManager.SendPushNotification(message));
                }
                await Task.WhenAll(sendPushTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }            

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

                var lessonPlanDatas = KeyValues["lessonPlansData"] as List<GetLessonPlanNotificationResult>;
                var sendEmailTasks = new List<Task>();

                var staff = await _dbContext.Entity<MsUser>()
                            .Where(x => IdUserRecipients.Contains(x.Id))
                            .Select(x => new
                            {
                                Id = x.Id,
                                EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                            })
                            .ToListAsync(CancellationToken);

                foreach (var idUser in IdUserRecipients)
                {

                    if (!EnsureAnyEmails(staff.Where(x => x.Id == idUser).Select(x => x.EmailAddress)))
                        continue;

                    var lessonPlanData = lessonPlanDatas.Where(x => x.IdUser == idUser).ToList();

                    string teacherName = "", schoolName = "";
                    var staffEmail = staff.Where(x => x.Id == idUser).Select(x => x.EmailAddress).FirstOrDefault();

                    teacherName = lessonPlanData.Select(x=>x.TeacherName).FirstOrDefault();
                    schoolName = lessonPlanData.Select(x => x.SchoolName).FirstOrDefault();

                    _notificationData = new Dictionary<string, object>
                    {
                        { "TeacherName", teacherName.TrimStart().TrimEnd() },
                        { "SchoolName", schoolName.ToUpper() },
                        { "Data" , lessonPlanData}
                    };

                    var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                    var emailBody = emailTemplate(_notificationData);

                    // NOTE: create SendGridMessage object to send email
                    var message = new SendGridMessage
                    {
                        Subject = NotificationTemplate.Title,
                        Personalizations = new List<Personalization>
                        {
                            new Personalization { Tos = new List<EmailAddress> { staffEmail } }
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
                    { "email" , ""}
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
                        GeneratedContent = "Your lesson plan for this following week has not been uploaded. Click to see more information or check your email registered in this app";
                        //continue;
                    }
                    else
                    {
                        _notificationData["email"] = staffEmail.Email;
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

        //private static string LPDataItem(string dataItem, string AcademicYear, string Level, string Grade, string Term, string Subject, string SubjectLevel, string Periode, string Deadline, string Link)
        //{
        //    dataItem = dataItem + "<tr>" +
        //            "<td>" + AcademicYear + "</td>" +
        //            "<td>" + Level + "</td>" +
        //            "<td>" + Grade + "</td>" +
        //            "<td>" + Term + "</td>" +
        //            "<td>" + Subject + "</td>" +
        //            "<td>" + SubjectLevel + "</td>" +
        //            "<td>" + Periode + "</td>" +
        //            "<td>" + Deadline + "</td>" +
        //            "<td><a href='" + Link + "'>Link</a></td>" +
        //        "</tr>";

        //    return dataItem;
        //}

        public void Dispose()
        {
            (_dbContext as TeachingDbContext)?.Dispose();
        }
    }
}
