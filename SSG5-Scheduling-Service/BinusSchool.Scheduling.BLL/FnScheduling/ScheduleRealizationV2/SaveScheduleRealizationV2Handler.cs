using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Auth.Authentications.Jwt;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2.Validator;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{

    public class SaveScheduleRealizationV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SaveScheduleRealizationV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SaveScheduleRealizationV2Request, SaveScheduleRealizationV2Validator>();

            foreach(var dataBody in body.DataScheduleRealizations)
            {
                var existScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                    .Where(x => x.ScheduleDate == dataBody.Date 
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.IdDay == dataBody.IdDay)
                    .FirstOrDefaultAsync(CancellationToken);
                
                var existScheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.ScheduleDate == dataBody.Date 
                                && x.IdBinusian == dataBody.IdUserTeacher
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.IdDay == dataBody.IdDay)
                    .FirstOrDefaultAsync(CancellationToken);

                var dataUser = _dbContext.Entity<MsUser>();
                var dataVenue = _dbContext.Entity<MsVenue>();

                if (dataBody.IdChangeVenue == null)
                    dataBody.IdChangeVenue = dataBody.IdRegularVenue;

                var IdScheduleRealization = string.Empty;

                if (existScheduleRealization == null)
                {
                    IdScheduleRealization = Guid.NewGuid().ToString();

                    var addScheduleRealization = new TrScheduleRealization2
                    {

                        Id = IdScheduleRealization,
                        ScheduleDate = dataBody.Date,
                        IdBinusian = dataBody.IdUserTeacher,
                        TeacherName = dataUser.Where(x => x.Id == dataBody.IdUserTeacher).First().DisplayName,
                        IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher,
                        TeacherNameSubtitute = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).First().DisplayName,
                        IdVenue = dataBody.IdRegularVenue,
                        VenueName = dataVenue.Where(x => x.Id == dataBody.IdRegularVenue).First().Description,
                        IdVenueChange = dataBody.IdChangeVenue,
                        VenueNameChange = dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).First().Description,
                        ClassID = dataBody.ClassID,
                        SessionID = dataBody.SessionID,
                        StartTime = existScheduleLesson.StartTime,
                        EndTime = existScheduleLesson.EndTime,
                        IsCancel = dataBody.IsCancel,
                        IsSendEmail = dataBody.IsSendEmail,
                        NotesForSubtitutions = dataBody.NotesForSubtitutions,
                        Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null,
                        IdDay = dataBody.IdDay,
                        DaysOfWeek = existScheduleLesson.DaysOfWeek,
                        IdLesson = dataBody.IdLesson,
                        IdAcademicYear = existScheduleLesson.IdAcademicYear,
                        IdLevel = dataBody.IdLevel,
                        IdGrade = dataBody.IdGrade
                    };

                    _dbContext.Entity<TrScheduleRealization2>().AddRange(addScheduleRealization);
                }
                else
                {
                    IdScheduleRealization = existScheduleRealization.Id;

                    existScheduleRealization.IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher;
                    existScheduleRealization.TeacherNameSubtitute = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).First().DisplayName;
                    existScheduleRealization.IdVenueChange = dataBody.IdChangeVenue;
                    existScheduleRealization.VenueNameChange = dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).First().Description;
                    existScheduleRealization.Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : existScheduleRealization.Status;
                    existScheduleRealization.IsCancel = dataBody.IsCancel;

                    _dbContext.Entity<TrScheduleRealization2>().UpdateRange(existScheduleRealization);
                }

                //save change to table history schedule realizaation
                var addHistoryScheduleRealization = new HTrScheduleRealization2
                {
                    IdHTrScheduleRealization2 = Guid.NewGuid().ToString(),
                    IdScheduleRealization2 = IdScheduleRealization,
                    ScheduleDate = dataBody.Date,
                    IdBinusian = dataBody.IdUserTeacher,
                    TeacherName = dataUser.Where(x => x.Id == dataBody.IdUserTeacher).First().DisplayName,
                    IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher,
                    TeacherNameSubtitute = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).First().DisplayName,
                    IdVenue = dataBody.IdRegularVenue,
                    VenueName = dataVenue.Where(x => x.Id == dataBody.IdRegularVenue).First().Description,
                    IdVenueChange = dataBody.IdChangeVenue,
                    VenueNameChange = dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).First().Description,
                    ClassID = dataBody.ClassID,
                    SessionID = dataBody.SessionID,
                    StartTime = existScheduleLesson.StartTime,
                    EndTime = existScheduleLesson.EndTime,
                    IsCancel = dataBody.IsCancel,
                    IsSendEmail = dataBody.IsSendEmail,
                    NotesForSubtitutions = dataBody.NotesForSubtitutions,
                    Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null,
                    IdDay = dataBody.IdDay,
                    DaysOfWeek = existScheduleLesson.DaysOfWeek,
                    IdLesson = dataBody.IdLesson,
                    IdAcademicYear = existScheduleLesson.IdAcademicYear,
                    IdLevel = dataBody.IdLevel,
                    IdGrade = dataBody.IdGrade
                };

                _dbContext.Entity<HTrScheduleRealization2>().AddRange(addHistoryScheduleRealization);

            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            foreach (var dataBody in body.DataScheduleRealizations)
            {
                if (dataBody.IdUserTeacher == dataBody.IdUserSubtituteTeacher && dataBody.IsSubtituteChange)
                    continue;

                // var dataGenerate = _dbContext.Entity<TrGeneratedScheduleLesson>().Where(x => x.Id == dataBody.Ids.First()).First();

                var dataGenerate = await _dbContext.Entity<MsScheduleLesson>()
                    .Where(x => x.ScheduleDate == dataBody.Date 
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.IdDay == dataBody.IdDay)
                    .FirstOrDefaultAsync(CancellationToken);

                if (dataBody.IsSendEmail)
                {
                    if (dataBody.IsSubtituteChange && dataBody.IsVenueChange)
                    {
                        var EmailScheduleRealization = new GetEmailScheduleRealizationResult
                        {
                            Ids = dataBody.Ids,
                            Date = dataBody.Date,
                            SessionID = dataBody.SessionID,
                            SessionStartTime = dataGenerate.StartTime,
                            SessionEndTime = dataGenerate.EndTime,
                            ClassID = dataBody.ClassID,
                            IdUserTeacher = new List<string> { dataBody.IdUserTeacher },
                            IdUserSubtituteTeacher = new List<string> { dataBody.IdUserSubtituteTeacher },
                            IdRegularVenue = dataBody.IdRegularVenue,
                            IdChangeVenue = dataBody.IdChangeVenue,
                            NotesForSubtitutions = dataBody.NotesForSubtitutions,
                            DateIn = dataGenerate.DateIn,
                            IdLesson = dataGenerate.IdLesson
                        };

                        if (KeyValues.ContainsKey("EmailScheduleRealization"))
                        {
                            KeyValues.Remove("EmailScheduleRealization");
                        }
                        KeyValues.Add("EmailScheduleRealization", EmailScheduleRealization);
                        var Notification = SendEmailNotifSR1(KeyValues, AuthInfo);
                    }
                    else if (dataBody.IsSubtituteChange)
                    {
                        var EmailScheduleRealization = new GetEmailScheduleRealizationResult
                        {
                            Ids = dataBody.Ids,
                            Date = dataBody.Date,
                            SessionID = dataBody.SessionID,
                            SessionStartTime = dataGenerate.StartTime,
                            SessionEndTime = dataGenerate.EndTime,
                            ClassID = dataBody.ClassID,
                            IdUserTeacher = new List<string> { dataBody.IdUserTeacher },
                            IdUserSubtituteTeacher = new List<string> { dataBody.IdUserSubtituteTeacher },
                            IdRegularVenue = dataBody.IdRegularVenue,
                            IdChangeVenue = dataBody.IdChangeVenue,
                            NotesForSubtitutions = dataBody.NotesForSubtitutions,
                            DateIn = dataGenerate.DateIn,
                            IdLesson = dataGenerate.IdLesson
                        };

                        if (KeyValues.ContainsKey("EmailScheduleRealization"))
                        {
                            KeyValues.Remove("EmailScheduleRealization");
                        }
                        KeyValues.Add("EmailScheduleRealization", EmailScheduleRealization);
                        var Notification = SendEmailNotifSR1(KeyValues, AuthInfo);
                    }
                    else if (dataBody.IsVenueChange)
                    {
                        var EmailScheduleRealization = new GetEmailScheduleRealizationResult
                        {
                            Ids = dataBody.Ids,
                            Date = dataBody.Date,
                            SessionID = dataBody.SessionID,
                            SessionStartTime = dataGenerate.StartTime,
                            SessionEndTime = dataGenerate.EndTime,
                            ClassID = dataBody.ClassID,
                            IdUserTeacher = new List<string> { dataBody.IdUserTeacher },
                            IdUserSubtituteTeacher = new List<string> { dataBody.IdUserSubtituteTeacher },
                            IdRegularVenue = dataBody.IdRegularVenue,
                            IdChangeVenue = dataBody.IdChangeVenue,
                            NotesForSubtitutions = dataBody.NotesForSubtitutions,
                            DateIn = dataGenerate.DateIn,
                            IdLesson = dataGenerate.IdLesson
                        };

                        if (KeyValues.ContainsKey("EmailScheduleRealization"))
                        {
                            KeyValues.Remove("EmailScheduleRealization");
                        }
                        KeyValues.Add("EmailScheduleRealization", EmailScheduleRealization);
                        var Notification = SendEmailNotifSR2(KeyValues, AuthInfo);
                    }
                    else
                    {

                    }
                }
            }

            return Request.CreateApiResult2();
        }

        public static string SendEmailNotifSR1(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SR1")
                {
                    IdRecipients = EmailScheduleRealization.IdUserSubtituteTeacher.Select(e=>e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string SendEmailNotifSR2(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SR2")
                {
                    IdRecipients = EmailScheduleRealization.IdUserSubtituteTeacher.Select(e=>e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

    }
}
