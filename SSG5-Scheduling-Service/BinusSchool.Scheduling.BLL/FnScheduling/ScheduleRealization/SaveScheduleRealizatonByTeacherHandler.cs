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
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using BinusSchool.Auth.Authentications.Jwt;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{

    public class SaveScheduleRealizationByTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SaveScheduleRealizationByTeacherHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SaveScheduleRealizationByTeacherRequest, SaveScheduleRealizationByTeacherValidator>();

            foreach(var dataBody in body.DataScheduleRealizations)
            {
                var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => dataBody.Ids.Contains(x.Id));
                
                predicate = predicate.And(x => x.IdUser == dataBody.IdUserTeacher && x.DaysOfWeek == dataBody.DaysOfWeek && x.ScheduleDate >= dataBody.StartDate && x.ScheduleDate <= dataBody.EndDate);
                
                var dataGenerate = _dbContext.Entity<TrGeneratedScheduleLesson>().Where(predicate).ToList();

                foreach(var dataG in dataGenerate)
                {
                    var existScheduleLesson = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                    .FirstOrDefaultAsync(x => x.Id == dataG.Id, CancellationToken);

                    var existScheduleRealization = await _dbContext.Entity<TrScheduleRealization>()
                    .FirstOrDefaultAsync(x => x.Id == dataG.Id, CancellationToken);

                    var dataUser = _dbContext.Entity<MsUser>();
                    var dataVenue = _dbContext.Entity<MsVenue>();

                    if(dataBody.IsCancel)
                    {
                        existScheduleLesson.IsCancelScheduleRealization = true;
                        existScheduleLesson.IsSetScheduleRealization = false;
                        existScheduleLesson.IsGenerated = false;
                    }
                    else if(dataBody.IsSubtituteChange && dataBody.IsVenueChange)
                    {
                        existScheduleLesson.IdUser = dataBody.IdUserSubtituteTeacher;
                        existScheduleLesson.TeacherName = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).First().DisplayName;
                        existScheduleLesson.IdVenue = dataBody.IdChangeVenue;
                        existScheduleLesson.VenueName = dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).First().Description;
                        existScheduleLesson.IdBinusianOld = dataBody.IdUserTeacher;
                        existScheduleLesson.TeacherNameOld = dataUser.Where(x => x.Id == dataBody.IdUserTeacher).First().DisplayName;
                        existScheduleLesson.IdVenueOld = dataBody.IdRegularVenue;
                        existScheduleLesson.VenueNameOld = dataVenue.Where(x => x.Id == dataBody.IdRegularVenue).First().Description;
                        existScheduleLesson.IsCancelScheduleRealization = false;
                        existScheduleLesson.IsSetScheduleRealization = true;
                        existScheduleLesson.IsGenerated = true;
                    }
                    else if(dataBody.IsSubtituteChange)
                    {
                        existScheduleLesson.IdUser = dataBody.IdUserSubtituteTeacher;
                        existScheduleLesson.TeacherName = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).First().DisplayName;
                        existScheduleLesson.IdBinusianOld = dataBody.IdUserTeacher;
                        existScheduleLesson.TeacherNameOld = dataUser.Where(x => x.Id == dataBody.IdUserTeacher).First().DisplayName;
                        existScheduleLesson.IsCancelScheduleRealization = false;
                        existScheduleLesson.IsSetScheduleRealization = true;
                        existScheduleLesson.IsGenerated = true;
                    }
                    else if(dataBody.IsVenueChange)
                    {
                        existScheduleLesson.IdVenue = dataBody.IdChangeVenue;
                        existScheduleLesson.VenueName = dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).First().Description;
                        existScheduleLesson.IdVenueOld = dataBody.IdRegularVenue;
                        existScheduleLesson.VenueNameOld = dataVenue.Where(x => x.Id == dataBody.IdRegularVenue).First().Description;
                        existScheduleLesson.IsCancelScheduleRealization = false;
                        existScheduleLesson.IsSetScheduleRealization = true;
                        existScheduleLesson.IsGenerated = true;
                    }
                    else
                    {
                        existScheduleLesson.IsSetScheduleRealization = true;
                        existScheduleLesson.IsGenerated = true;
                    }


                    _dbContext.Entity<TrGeneratedScheduleLesson>().UpdateRange(existScheduleLesson);
                    
                    if(existScheduleRealization == null)
                    {
                        var addScheduleRealization = new TrScheduleRealization
                        {
                            Id = dataG.Id.ToString(),
                            IdGeneratedScheduleLesson = dataG.Id,
                            ScheduleDate = dataG.ScheduleDate,
                            IdBinusian = dataG.IdBinusianOld != null ? dataG.IdBinusianOld : dataG.IdUser,
                            TeacherName = dataG.TeacherNameOld != null ? dataG.TeacherNameOld : dataG.TeacherName,
                            IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher,
                            TeacherNameSubtitute = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).First().DisplayName,
                            IdVenue = dataG.IdVenueOld != null ? dataG.IdVenueOld : dataG.IdVenue,
                            VenueName = dataG.VenueNameOld != null ? dataG.VenueNameOld : dataG.VenueName,
                            IdVenueChange = dataBody.IdChangeVenue,
                            VenueNameChange = dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).First().Description,
                            IdHomeroom = dataG.IdHomeroom,
                            HomeroomName = dataG.HomeroomName,
                            ClassID = dataBody.ClassID,
                            SessionID = dataBody.SessionID,
                            StartTime = dataG.StartTime,
                            EndTime = dataG.EndTime,
                            IsCancel = dataBody.IsCancel,
                            IsSendEmail = dataBody.IsSendEmail,
                            NotesForSubtitutions = dataBody.NotesForSubtitutions,
                            Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null,
                            DaysOfWeek = dataG.DaysOfWeek
                        };

                        _dbContext.Entity<TrScheduleRealization>().AddRange(addScheduleRealization);
                    }
                    else
                    {
                        existScheduleRealization.IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher;
                        existScheduleRealization.TeacherNameSubtitute = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).First().DisplayName;
                        existScheduleRealization.IdVenueChange = dataBody.IdChangeVenue;
                        existScheduleRealization.VenueNameChange = dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).First().Description;
                        existScheduleRealization.Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null;
                        existScheduleRealization.IsCancel = dataBody.IsCancel;

                        _dbContext.Entity<TrScheduleRealization>().UpdateRange(existScheduleRealization);
                    }
                    
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            foreach(var dataBody in body.DataScheduleRealizations)
            {
                var dataGenerate = _dbContext.Entity<TrGeneratedScheduleLesson>().Where(x => x.Id == dataBody.Ids.First()).First();

                if(dataBody.IsSubtituteChange && dataBody.IsVenueChange)
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
                        IdUserTeacher = new List<string> {dataBody.IdUserTeacher},
                        IdUserSubtituteTeacher = new List<string> {dataBody.IdUserSubtituteTeacher},
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
                else if(dataBody.IsSubtituteChange) 
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
                        IdUserTeacher = new List<string> {dataBody.IdUserTeacher},
                        IdUserSubtituteTeacher = new List<string> {dataBody.IdUserSubtituteTeacher},
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
                else if(dataBody.IsVenueChange)
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
                        IdUserTeacher = new List<string> {dataBody.IdUserTeacher},
                        IdUserSubtituteTeacher = new List<string> {dataBody.IdUserSubtituteTeacher},
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
                    IdRecipients = EmailScheduleRealization.IdUserSubtituteTeacher.Select(e=>e).Distinct().ToList(),
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
                    IdRecipients = EmailScheduleRealization.IdUserSubtituteTeacher.Select(e=>e).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
