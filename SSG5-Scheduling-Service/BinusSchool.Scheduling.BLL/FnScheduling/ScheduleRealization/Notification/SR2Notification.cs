using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Notification
{
    public class SR2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<SR2Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private string _schoolName;

        public SR2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<SR2Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("SR2", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "SR_CHANGE_VENUE";
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

        protected override Task Prepare()
        {
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/schedule/teachertracking";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return Task.CompletedTask;
            }
        }

        protected override async Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => new { x.FirebaseToken, x.IdUser })
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens.Select(e => e.FirebaseToken).ToList()))
                return;

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // _notificationData["datanotif"] = EmailScheduleRealization;

            // var SendPushNotificationTaks = new List<Task>();
            // foreach (var item in EmailTextbook.Textbooks)
            // {
            //     _notificationData["Link"] = $"{_notificationData["UrlBase"]}?Id={item.Id}";
            //     PushNotificationData["id"] = item.Id;

            //     var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            //     var PushBody = PushTemplate(_notificationData);

            //     var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            //     var TitleBody = TitleTemplate(_notificationData);


            //     // NOTE: create MulticastMessage object to send push notification
            //     var message = new MulticastMessage
            //     {
            //         Notification = new FirebaseAdmin.Messaging.Notification
            //         {
            //             Title = TitleBody,
            //             Body = PushBody
            //         },
            //         Tokens = tokens.Where(e=>e.IdUser==item.IdUserApproval).Select(e => e.FirebaseToken).ToList(),
            //         Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            //     };

            //     GeneratedTitle = TitleBody;
            //     GeneratedContent = PushBody;
            //     await Task.WhenAll(SendPushNotificationTaks);
            // }
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));
            var saveNotificationTasks = new List<Task>();

            _notificationData["Staff"] = EmailScheduleRealization;

            // foreach (var item in EmailScheduleRealization.IdRegularVenue)
            // {
            //     _notificationData["Link"] = $"{_notificationData["UrlBase"]}?Id={item.Id}";
            //     PushNotificationData["id"] = item.Id;

            //     var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            //     GeneratedContent = pushTemplate(_notificationData);

            //     var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            //     GeneratedTitle = pushTitle(_notificationData);

            //     saveNotificationTasks.Add(NotificationManager.SaveNotification(
            //     CreateNotificationHistory(
            //         new List<string> { item.IdUserApproval },
            //         isBlast,
            //     GeneratedTitle ?? NotificationTemplate.Title,
            //     GeneratedContent ?? NotificationTemplate.Push)));
            //     await Task.WhenAll(saveNotificationTasks);
            // }
        }

        protected override async Task SendEmailNotification()
        {
            var currentAy = await _dbContext.Entity<MsAcademicYear>().OrderByDescending(x => x.Code).FirstOrDefaultAsync(x => x.IdSchool == IdSchool);

            var GetUser = await _dbContext.Entity<MsUser>()
                .Where(x => IdUserRecipients.Contains(x.Id) && x.Status)
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

            if (!GetUser.Any())
                return;

            var userEmails = new List<EmailAddress>();
            var listCc = new List<EmailAddress>();

            var distinctUserEmailTO = GetUser.GroupBy(x => x.EmailAddress.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (!distinctUserEmailTO.Any())
            {
                userEmails.AddRange(GetUser.Select(x => x.EmailAddress).ToList());
            }
            else
            {
                foreach (var emailTo in distinctUserEmailTO)
                {
                    var usrEmail = GetUser.Where(x => x.EmailAddress.Email == emailTo).FirstOrDefault();
                    userEmails.Add(usrEmail.EmailAddress);
                }
            }

            _schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == IdSchool)
                .Select(x => x.Name.ToUpper())
                .FirstOrDefaultAsync(CancellationToken);

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            var GetUserSubstituteTeacher = await _dbContext.Entity<MsUser>()
                .Where(x => EmailScheduleRealization.IdUserSubtituteTeacher.Contains(x.Id) && x.Status)
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

            var distinctUserSubstituteTeacher = GetUserSubstituteTeacher.GroupBy(x => x.EmailAddress.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (!distinctUserSubstituteTeacher.Any())
            {
                listCc.AddRange(GetUserSubstituteTeacher.Select(x => x.EmailAddress).ToList());
            }

            //get SH from teacher old and new
            var listUserSH = new List<string>();

            var listTeacherOld = GetUser.Select(x => x.Id).Distinct().ToList();
            var listTeacherOldSH = GetListUserSHfromTeacherOldandNew(_dbContext, currentAy.Id, listTeacherOld, EmailScheduleRealization.IdLesson, true);
            listUserSH.AddRange(listTeacherOldSH);
            var listTeacherNew = GetUserSubstituteTeacher.Select(x => x.Id).Distinct().ToList();
            var listTeacherNewSH = GetListUserSHfromTeacherOldandNew(_dbContext, currentAy.Id, listTeacherNew, EmailScheduleRealization.IdLesson, false);
            listUserSH.AddRange(listTeacherNewSH);

            var GetUserTeacher = await _dbContext.Entity<MsUser>()
                .Where(x => EmailScheduleRealization.IdUserTeacher.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

            var distinctUserTeacher = GetUserTeacher.GroupBy(x => x.EmailAddress.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (!distinctUserTeacher.Any())
            {
                listCc.AddRange(GetUserTeacher.Select(x => x.EmailAddress).ToList());
            }

            //email cc from master setting email cc
            var GetUserEmailCC = await _dbContext.Entity<MsSettingEmailScheduleRealization>()
                .Include(x => x.Staff)
                .Where(x => x.IdSchool == IdSchool).ToListAsync(CancellationToken);

            var IdUserSpesific = GetUserEmailCC.Where(x => x.IsSetSpecificUser).Select(x => x.IdBinusian).ToList();

            var idTeacherPosition = GetUserEmailCC.Where(x => !x.IsSetSpecificUser).Select(x => x.IdTeacherPosition).ToList();

            var msTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                .Include(x => x.Position)
                                .Where(x => x.IdSchool == IdSchool)
                                .ToListAsync(CancellationToken);

            foreach (var userEmailCC in GetUserEmailCC.Where(x => !x.IsSetSpecificUser))
            {
                var users = await _dbContext.Entity<MsUser>()
                                .Include(x => x.UserSchools)
                                .Include(x => x.UserRoles)
                                    .ThenInclude(x => x.Role)
                                .Where(x => x.UserSchools.Any(a => a.IdSchool == IdSchool) && x.UserRoles.Any(x => x.Role.Id == userEmailCC.IdRole) && x.Status)
                                .ToListAsync(CancellationToken);

                var teacherPositions = msTeacherPosition.Where(x => userEmailCC.IdTeacherPosition.Contains(x.Id)).ToList();

                var msNonTeachingLoad = await _dbContext.Entity<TrNonTeachingLoad>()
                                   .Include(x => x.MsNonTeachingLoad)
                                   .Where(x => teacherPositions.Select(x => x.Id).Contains(x.MsNonTeachingLoad.IdTeacherPosition) && x.MsNonTeachingLoad.IdAcademicYear == currentAy.Id)
                                   .Distinct().ToListAsync(CancellationToken);

                if (users.Where(x => x.UserRoles.Any(y => y.Role.IdRoleGroup == "STF")).Any() && !msNonTeachingLoad.Any())
                {
                    var idUsers = users.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup.Contains("STF"))).ToList().Any() ?
                        users.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup.Contains("STF"))).ToList() :
                        users.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup.Contains("ADM"))).ToList().Any() ?
                        users.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup.Contains("ADM"))).ToList() :
                        users.Where(x => x.UserRoles.Any(x => x.Role.IdRoleGroup.Contains("STD"))).ToList();

                    IdUserSpesific.AddRange(idUsers.Select(x => x.Id));
                }
                else
                {
                    foreach (var teacherPosition in teacherPositions)
                    {
                        if (teacherPosition.Position.Code == "ST")
                        {
                            var msLessonTeacher = _dbContext.Entity<MsLessonTeacher>()
                                .Where(x => x.IdLesson == EmailScheduleRealization.IdLesson).Select(x => x.IdUser)
                                .ToList();

                            IdUserSpesific.AddRange(msLessonTeacher);
                        }
                        else if (teacherPosition.Position.Code == "CA" || teacherPosition.Position.Code == "COT")
                        {
                            var userHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                        .Where(x => teacherPositions.Select(x => x.Id).Contains(x.Id))
                                        .ToListAsync(CancellationToken);

                            IdUserSpesific.AddRange(userHomeroomTeacher.Where(x => x.IdTeacherPosition == userEmailCC.IdTeacherPosition).Select(x => x.IdBinusian).ToList());
                        }
                        else if (teacherPosition.Position.Code == "SH")
                        {
                            //IdUserSpesific.AddRange(users.Select(x => x.Id).ToList());
                            IdUserSpesific.AddRange(listUserSH); // add user sh from teacher old
                        }
                        else
                        {
                            IdUserSpesific.AddRange(msNonTeachingLoad.Select(x => x.IdUser).ToList());
                        }
                    }
                }
            }

            var emailUserCC = await _dbContext.Entity<MsUser>().Where(x => IdUserSpesific.Contains(x.Id) && !GetUser.Select(x => x.Id).ToList().Contains(x.Id) && x.Status)
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                }).ToListAsync(CancellationToken);

            var distinctUserEmailCC = emailUserCC.GroupBy(x => x.EmailAddress.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (!distinctUserEmailCC.Any())
            {
                listCc.AddRange(emailUserCC.Select(x => x.EmailAddress).ToList());
            }

            var listUnionCC = emailUserCC.Union(GetUserSubstituteTeacher).Union(GetUserTeacher).ToList();

            var duplicateEmail = listUnionCC.GroupBy(x => x.EmailAddress.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

            if (duplicateEmail.Count() != 0)
            {
                listUnionCC.RemoveAll(x => duplicateEmail.Contains(x.EmailAddress.Email));

                listUnionCC.RemoveAll(x => userEmails.Select(x => x.Email).Contains(x.EmailAddress.Email));

                var listCCunionListUnion = listUnionCC.Select(x => x.EmailAddress).Concat(listCc).Concat(userEmails).ToList();

                var duplicateCC = listCCunionListUnion.GroupBy(x => x.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

                listCc.RemoveAll(x => duplicateCC.Contains(x.Email));

                listCc.AddRange(listUnionCC.Select(x => x.EmailAddress));
            }
            else
            {
                var listCCUnionUserEmail = listCc.Concat(userEmails).ToList();
                var duplicateCC = listCCUnionUserEmail.GroupBy(x => x.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                listCc.RemoveAll(x => duplicateCC.Contains(x.Email));
            }

            var dataVenue = _dbContext.Entity<MsVenue>();

            foreach (var ItemUser in userEmails)
            {
                // _notificationData["Data"] = EmailTextbook.Textbooks.Where(e=>e.IdUserApproval==ItemUser.Id);

                _notificationData["ClassTeacher"] = ItemUser.Name;
                _notificationData["Date"] = EmailScheduleRealization.Date?.ToString("dd MMMM yyyy");
                _notificationData["StartDate"] = EmailScheduleRealization.StartDate?.ToString("dd MMMM yyyy");
                _notificationData["EndDate"] = EmailScheduleRealization.EndDate?.ToString("dd MMMM yyyy");
                _notificationData["Session"] = "Session " + EmailScheduleRealization.SessionID + " ( " + EmailScheduleRealization.SessionStartTime + " - " + EmailScheduleRealization.SessionEndTime + " ) ";
                _notificationData["ClassID"] = EmailScheduleRealization.ClassID;
                _notificationData["TeacherName"] = string.Join(",", GetUserTeacher.Select(x => x.Name).ToList());
                _notificationData["EntryStatus"] = "Done (by System," + EmailScheduleRealization.DateIn?.ToString("dd MMMM yyyy HH:mm") + ")";
                _notificationData["RegularVenue"] = dataVenue.Where(x => x.Id == EmailScheduleRealization.IdRegularVenue).First().Description;
                _notificationData["SubstituteTeacher"] = string.Join(",", GetUserSubstituteTeacher.Select(x => x.Name).ToList());
                _notificationData["ChangeVenue"] = EmailScheduleRealization.IdChangeVenue == null ? dataVenue.Where(x => x.Id == EmailScheduleRealization.IdRegularVenue).First().Description : dataVenue.Where(x => x.Id == EmailScheduleRealization.IdChangeVenue).First().Description;
                _notificationData["NotesSubstituteTeacher"] = EmailScheduleRealization.NotesForSubtitutions;
                _notificationData["SchoolName"] = _schoolName;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var titleBody = titleTemplate(_notificationData);

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = titleBody,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization
                        {
                            Tos = new List<EmailAddress>
                            {
                                ItemUser
                            },
                            Ccs = listCc.Count() > 0 ? listCc : null
                        }
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

        public List<string> GetListUserSHfromTeacherOldandNew(ISchedulingDbContext _dbContext, string currentAy, List<string> listUser, string idLesson, bool isOldTeacher)
        {
            var result = new List<string>();

            var IdSubjectFromLesson = _dbContext.Entity<MsLesson>().FirstOrDefault(x => x.Id == idLesson)?.IdSubject;

            var msLessonTeacher = _dbContext.Entity<MsLessonTeacher>()
                .Include(x => x.Lesson).ThenInclude(x => x.Subject).ThenInclude(x => x.Grade)
                .Where(x => listUser.Contains(x.IdUser) && x.Lesson.IdAcademicYear == currentAy)
                .Select(x => new
                {
                    idGrade = x.Lesson.Subject.Grade.Id,
                    idSubject = x.Lesson.Subject.Id
                })
                .Distinct().ToList();

            var dataTrNonTeachingLoad = _dbContext.Entity<TrNonTeachingLoad>()
                .Include(x => x.MsNonTeachingLoad)
                .ThenInclude(x => x.TeacherPosition)
                .ThenInclude(x => x.Position)
                .Where(x => x.MsNonTeachingLoad.IdAcademicYear == currentAy && x.MsNonTeachingLoad.TeacherPosition.Position.Code == "SH").ToList();

            foreach (var itemMsLessonTeacher in msLessonTeacher)
            {
                foreach (var itemTrNonTeaching in dataTrNonTeachingLoad)
                {
                    var _dataGrade = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(itemTrNonTeaching.Data);
                    _dataGrade.TryGetValue("Grade", out var _grade);
                    _dataGrade.TryGetValue("Subject", out var _subject);

                    if (isOldTeacher)
                    {
                        if (itemMsLessonTeacher.idGrade == _grade.Id && IdSubjectFromLesson == _subject.Id)
                        {
                            result.Add(itemTrNonTeaching.IdUser);
                        }
                    }
                    else
                    {
                        if (itemMsLessonTeacher.idGrade == _grade.Id && itemMsLessonTeacher.idSubject == _subject.Id)
                        {
                            result.Add(itemTrNonTeaching.IdUser);
                        }
                    }
                }
            }

            return result.Distinct().ToList();
        }
    }
}
