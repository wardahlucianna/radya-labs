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

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{

    public class SaveScheduleRealizationByTeacherV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SaveScheduleRealizationByTeacherV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SaveScheduleRealizationByTeacherV2Request, SaveScheduleRealizationByTeacherV2Validator>();

            foreach (var dataBody in body.DataScheduleRealizations)
            {
                var dataScheduleLesson = _dbContext.Entity<MsScheduleLesson>()
                    .Where(x => x.ScheduleDate >= dataBody.StartDate
                                && x.ScheduleDate <= dataBody.EndDate
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.DaysOfWeek == dataBody.DaysOfWeek)
                    .ToList();

                var dataUser = _dbContext.Entity<MsUser>();
                var dataVenue = _dbContext.Entity<MsVenue>();

                if (dataBody.IdChangeVenue == null)
                    dataBody.IdChangeVenue = dataBody.IdRegularVenue;

                foreach (var dataSL in dataScheduleLesson)
                {
                    var existScheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.ScheduleDate == dataSL.ScheduleDate
                                && x.IdBinusian == dataBody.IdUserTeacher
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.IdDay == dataBody.IdDay)
                    .FirstOrDefaultAsync(CancellationToken);

                    var IdScheduleRealization = string.Empty;

                    if (existScheduleRealization == null)
                    {
                        IdScheduleRealization = Guid.NewGuid().ToString();

                        var addScheduleRealization = new TrScheduleRealization2
                        {
                            Id = IdScheduleRealization,
                            ScheduleDate = dataSL.ScheduleDate,
                            IdBinusian = dataBody.IdUserTeacher,
                            TeacherName = dataUser.Where(x => x.Id == dataBody.IdUserTeacher).FirstOrDefault().DisplayName,
                            IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher,
                            TeacherNameSubtitute = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).FirstOrDefault().DisplayName,
                            IdVenue = dataBody.IdRegularVenue,
                            VenueName = dataVenue.Where(x => x.Id == dataBody.IdRegularVenue).FirstOrDefault().Description,
                            IdVenueChange = !string.IsNullOrEmpty(dataBody.IdChangeVenue) ? dataBody.IdChangeVenue : null,
                            VenueNameChange = !string.IsNullOrEmpty(dataBody.IdChangeVenue) ? dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).FirstOrDefault().Description : null,
                            ClassID = dataBody.ClassID,
                            SessionID = dataBody.SessionID,
                            StartTime = dataSL.StartTime,
                            EndTime = dataSL.EndTime,
                            IsCancel = dataBody.IsCancel,
                            IsSendEmail = dataBody.IsSendEmail,
                            NotesForSubtitutions = dataBody.NotesForSubtitutions,
                            Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null,
                            IdDay = dataBody.IdDay,
                            DaysOfWeek = dataSL.DaysOfWeek,
                            IdLesson = dataBody.IdLesson,
                            IdAcademicYear = dataSL.IdAcademicYear,
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
                        existScheduleRealization.Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null;
                        existScheduleRealization.IsCancel = dataBody.IsCancel;
                        existScheduleRealization.NotesForSubtitutions = dataBody.NotesForSubtitutions;

                        _dbContext.Entity<TrScheduleRealization2>().UpdateRange(existScheduleRealization);
                    }

                    var addHistroryScheduleRealization = new HTrScheduleRealization2
                    {
                        IdHTrScheduleRealization2 = Guid.NewGuid().ToString(),
                        IdScheduleRealization2 = IdScheduleRealization,
                        ScheduleDate = dataSL.ScheduleDate,
                        IdBinusian = dataBody.IdUserTeacher,
                        TeacherName = dataUser.Where(x => x.Id == dataBody.IdUserTeacher).FirstOrDefault().DisplayName,
                        IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher,
                        TeacherNameSubtitute = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).FirstOrDefault().DisplayName,
                        IdVenue = dataBody.IdRegularVenue,
                        VenueName = dataVenue.Where(x => x.Id == dataBody.IdRegularVenue).FirstOrDefault().Description,
                        IdVenueChange = !string.IsNullOrEmpty(dataBody.IdChangeVenue) ? dataBody.IdChangeVenue : null,
                        VenueNameChange = !string.IsNullOrEmpty(dataBody.IdChangeVenue) ? dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).FirstOrDefault().Description : null,
                        ClassID = dataBody.ClassID,
                        SessionID = dataBody.SessionID,
                        StartTime = dataSL.StartTime,
                        EndTime = dataSL.EndTime,
                        IsCancel = dataBody.IsCancel,
                        IsSendEmail = dataBody.IsSendEmail,
                        NotesForSubtitutions = dataBody.NotesForSubtitutions,
                        Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null,
                        IdDay = dataBody.IdDay,
                        DaysOfWeek = dataSL.DaysOfWeek,
                        IdLesson = dataBody.IdLesson,
                        IdAcademicYear = dataSL.IdAcademicYear,
                        IdLevel = dataBody.IdLevel,
                        IdGrade = dataBody.IdGrade
                    };

                    _dbContext.Entity<HTrScheduleRealization2>().AddRange(addHistroryScheduleRealization);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            foreach (var dataBody in body.DataScheduleRealizations)
            {
                var dataGenerate = await _dbContext.Entity<MsScheduleLesson>()
                    .Where(x => x.ScheduleDate >= dataBody.StartDate
                                && x.ScheduleDate <= dataBody.EndDate
                                && x.ClassID == dataBody.ClassID
                                && x.SessionID == dataBody.SessionID
                                && x.IdLesson == dataBody.IdLesson
                                && x.IdAcademicYear == dataBody.IdAcademicYear
                                && x.IdLevel == dataBody.IdLevel
                                && x.IdGrade == dataBody.IdGrade
                                && x.DaysOfWeek == dataBody.DaysOfWeek)
                    .FirstOrDefaultAsync(CancellationToken);

                if (dataBody.IsSubtituteChange && dataBody.IsVenueChange)
                {
                    var EmailScheduleRealization = new GetEmailScheduleRealizationResult
                    {
                        Ids = dataBody.Ids,
                        StartDate = dataBody.StartDate,
                        EndDate = dataBody.EndDate,
                        DaysOfWeek = dataBody.DaysOfWeek,
                        SessionID = dataBody.SessionID,
                        SessionStartTime = dataGenerate.StartTime,
                        SessionEndTime = dataGenerate.EndTime,
                        ClassID = dataBody.ClassID,
                        IdUserTeacher = new List<string> { dataBody.IdUserTeacher },
                        IdUserSubtituteTeacher = new List<string> { dataBody.IdUserSubtituteTeacher },
                        IdRegularVenue = dataBody.IdRegularVenue,
                        IdChangeVenue = dataBody.IdChangeVenue,
                        NotesForSubtitutions = dataBody.NotesForSubtitutions,
                        DateIn = dataGenerate.DateIn
                    };

                    if (KeyValues.ContainsKey("EmailScheduleRealization"))
                    {
                        KeyValues.Remove("EmailScheduleRealization");
                    }
                    KeyValues.Add("EmailScheduleRealization", EmailScheduleRealization);
                    var Notification = SendEmailNotifSR3(KeyValues, AuthInfo);

                    if (KeyValues.ContainsKey("EmailScheduleRealization"))
                    {
                        KeyValues.Remove("EmailScheduleRealization");
                    }
                    KeyValues.Add("EmailScheduleRealization", EmailScheduleRealization);
                    var NotificationSR4 = SendEmailNotifSR4(KeyValues, AuthInfo);
                }
                else if (dataBody.IsSubtituteChange)
                {
                    var EmailScheduleRealization = new GetEmailScheduleRealizationResult
                    {
                        Ids = dataBody.Ids,
                        StartDate = dataBody.StartDate,
                        EndDate = dataBody.EndDate,
                        DaysOfWeek = dataBody.DaysOfWeek,
                        SessionID = dataBody.SessionID,
                        SessionStartTime = dataGenerate.StartTime,
                        SessionEndTime = dataGenerate.EndTime,
                        ClassID = dataBody.ClassID,
                        IdUserTeacher = new List<string> { dataBody.IdUserTeacher },
                        IdUserSubtituteTeacher = new List<string> { dataBody.IdUserSubtituteTeacher },
                        IdRegularVenue = dataBody.IdRegularVenue,
                        IdChangeVenue = dataBody.IdChangeVenue,
                        NotesForSubtitutions = dataBody.NotesForSubtitutions,
                        DateIn = dataGenerate.DateIn
                    };

                    if (KeyValues.ContainsKey("EmailScheduleRealization"))
                    {
                        KeyValues.Remove("EmailScheduleRealization");
                    }
                    KeyValues.Add("EmailScheduleRealization", EmailScheduleRealization);
                    var Notification = SendEmailNotifSR3(KeyValues, AuthInfo);
                }
                else if (dataBody.IsVenueChange)
                {
                    var EmailScheduleRealization = new GetEmailScheduleRealizationResult
                    {
                        Ids = dataBody.Ids,
                        StartDate = dataBody.StartDate,
                        EndDate = dataBody.EndDate,
                        DaysOfWeek = dataBody.DaysOfWeek,
                        SessionID = dataBody.SessionID,
                        SessionStartTime = dataGenerate.StartTime,
                        SessionEndTime = dataGenerate.EndTime,
                        ClassID = dataBody.ClassID,
                        IdUserTeacher = new List<string> { dataBody.IdUserTeacher },
                        IdUserSubtituteTeacher = new List<string> { dataBody.IdUserSubtituteTeacher },
                        IdRegularVenue = dataBody.IdRegularVenue,
                        IdChangeVenue = dataBody.IdChangeVenue,
                        NotesForSubtitutions = dataBody.NotesForSubtitutions,
                        DateIn = dataGenerate.DateIn
                    };

                    if (KeyValues.ContainsKey("EmailScheduleRealization"))
                    {
                        KeyValues.Remove("EmailScheduleRealization");
                    }
                    KeyValues.Add("EmailScheduleRealization", EmailScheduleRealization);
                    var Notification = SendEmailNotifSR4(KeyValues, AuthInfo);
                }
                else
                {

                }
            }

            return Request.CreateApiResult2();
        }

        public static string SendEmailNotifSR3(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SR3")
                {
                    IdRecipients = EmailScheduleRealization.IdUserSubtituteTeacher.Select(e => e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string SendEmailNotifSR4(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            var EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SR4")
                {
                    IdRecipients = EmailScheduleRealization.IdUserSubtituteTeacher.Select(e => e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

    }
}
