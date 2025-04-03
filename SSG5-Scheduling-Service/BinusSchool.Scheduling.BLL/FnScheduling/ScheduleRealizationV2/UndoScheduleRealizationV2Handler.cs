using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2.Validator;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using FluentEmail.Core.Models;
using BinusSchool.Auth.Authentications.Jwt;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class UndoScheduleRealizationV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UndoScheduleRealizationV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UndoScheduleRealizationV2Request, UndoScheduleRealizationV2Validator>();

            var undoScheduleEmail = new List<GetEmailScheduleRealizationResult>();
            var undoStatusCancelScheduleEmail = new List<GetEmailScheduleRealizationResult>();

            foreach (var dataBody in body.DataScheduleRealizations)
            {
                var existScheduleRealizationHistory = await _htrScheduleRealization(dataBody);

                if (existScheduleRealizationHistory.Count > 1)
                {
                    var historyScheduleRealization = existScheduleRealizationHistory.FirstOrDefault();
                    historyScheduleRealization.IsActive = false;
                    _dbContext.Entity<HTrScheduleRealization2>().Update(historyScheduleRealization);

                    var lastUpdateHistory = existScheduleRealizationHistory.Where(x => x.IsActive == true).OrderByDescending(x => x.DateIn).FirstOrDefault();

                    var trScheduleRealization = _trScheduleRealization(dataBody).Result;

                    trScheduleRealization.IdBinusianSubtitute = lastUpdateHistory.IdBinusianSubtitute;
                    trScheduleRealization.TeacherNameSubtitute = lastUpdateHistory.TeacherNameSubtitute;

                    trScheduleRealization.IdVenueChange = lastUpdateHistory.IdVenueChange;
                    trScheduleRealization.VenueNameChange = lastUpdateHistory.VenueNameChange;

                    var isCancelled = trScheduleRealization.IsCancel;

                    trScheduleRealization.IsCancel = lastUpdateHistory.IsCancel;
                    trScheduleRealization.IsSendEmail = lastUpdateHistory.IsSendEmail;
                    trScheduleRealization.NotesForSubtitutions = lastUpdateHistory.NotesForSubtitutions;
                    trScheduleRealization.Status = lastUpdateHistory.Status;


                    _dbContext.Entity<TrScheduleRealization2>().Update(trScheduleRealization);

                    //collecting data for email
                    if (isCancelled)
                    {
                        undoStatusCancelScheduleEmail.Add(DataEmail(trScheduleRealization));
                    }
                    else
                    {
                        undoScheduleEmail.Add(DataEmail(trScheduleRealization));
                    }
                }
                else
                {
                    if (existScheduleRealizationHistory.Count == 1)
                    {
                        var historyScheduleRealization = existScheduleRealizationHistory.FirstOrDefault();
                        historyScheduleRealization.IsActive = false;
                        _dbContext.Entity<HTrScheduleRealization2>().Update(historyScheduleRealization);

                        var trScheduleRealization = _trScheduleRealization(dataBody).Result;

                        var isCancelled = trScheduleRealization.IsCancel;

                        if (isCancelled)
                        {
                            undoStatusCancelScheduleEmail.Add(DataEmail(trScheduleRealization));
                        }
                        else
                        {
                            undoScheduleEmail.Add(DataEmail(trScheduleRealization));
                        }

                        trScheduleRealization.IsActive = false;
                        _dbContext.Entity<TrScheduleRealization2>().Update(trScheduleRealization);
                    }
                    else
                    {
                        var trScheduleRealization = _trScheduleRealization(dataBody).Result;
                        trScheduleRealization.IsActive = false;
                        _dbContext.Entity<TrScheduleRealization2>().Update(trScheduleRealization);
                    }
                }

            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            foreach (var item in undoScheduleEmail)
            {
                if (KeyValues.ContainsKey("EmailScheduleRealization"))
                {
                    KeyValues.Remove("EmailScheduleRealization");
                }
                KeyValues.Add("EmailScheduleRealization", item);
                var Notification = SendEmailNotifSR7(KeyValues, AuthInfo);
            }

            foreach (var item in undoStatusCancelScheduleEmail)
            {
                if (KeyValues.ContainsKey("EmailScheduleRealization"))
                {
                    KeyValues.Remove("EmailScheduleRealization");
                }
                KeyValues.Add("EmailScheduleRealization", item);
                var Notification = SendEmailNotifSR9(KeyValues, AuthInfo);
            }

            return Request.CreateApiResult2();
        }

        private async Task<TrScheduleRealization2> _trScheduleRealization(UndoDataScheduleRealizationV2 dataBody)
        {
            var data = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.ScheduleDate == dataBody.Date
                                && x.IdBinusian == dataBody.IdUserTeacher
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.IdDay == dataBody.IdDay)
                    .OrderByDescending(x => x.DateIn)
                    .FirstOrDefaultAsync(CancellationToken);

            return data;
        }

        private async Task<List<HTrScheduleRealization2>> _htrScheduleRealization(UndoDataScheduleRealizationV2 dataBody)
        {
            var data = await _dbContext.Entity<HTrScheduleRealization2>()
                    .Where(x => x.ScheduleDate == dataBody.Date
                                && x.IdBinusian == dataBody.IdUserTeacher
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.IdDay == dataBody.IdDay)
                    .OrderByDescending(x => x.DateIn)
                    .ToListAsync(CancellationToken);

            return data;
        }

        public GetEmailScheduleRealizationResult DataEmail(TrScheduleRealization2 trScheduleRealization)
        {
            var emailData = new GetEmailScheduleRealizationResult
            {
                Date = trScheduleRealization.ScheduleDate,
                DaysOfWeek = trScheduleRealization.DaysOfWeek,
                SessionID = trScheduleRealization.SessionID,
                SessionStartTime = trScheduleRealization.StartTime,
                SessionEndTime = trScheduleRealization.EndTime,
                ClassID = trScheduleRealization.ClassID,
                IdUserTeacher = new List<string> { trScheduleRealization.IdBinusian },
                IdUserSubtituteTeacher = new List<string> { trScheduleRealization.IdBinusianSubtitute },
                IdRegularVenue = trScheduleRealization.IdVenue,
                IdChangeVenue = trScheduleRealization.IdVenueChange,
                NotesForSubtitutions = trScheduleRealization.NotesForSubtitutions,
                DateIn = trScheduleRealization.DateIn,
                IdLesson = trScheduleRealization.IdLesson,
                Status = trScheduleRealization.Status,
                IdAcademicYear = trScheduleRealization.IdAcademicYear,
            };

            return emailData;
        }

        public static string SendEmailNotifSR7(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SR7") //undo by teacher
                {
                    IdRecipients = EmailScheduleRealization.IdUserSubtituteTeacher.Select(e => e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string SendEmailNotifSR9(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SR9") //cancel by teacher
                {
                    IdRecipients = EmailScheduleRealization.IdUserTeacher.Select(e => e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
