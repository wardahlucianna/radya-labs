// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using BinusSchool.Common.Constants;
// using BinusSchool.Common.Exceptions;
// using BinusSchool.Common.Extensions;
// using BinusSchool.Common.Functions.Handler;
// using BinusSchool.Common.Model;
// using BinusSchool.Common.Model.Enums;
// using BinusSchool.Common.Utils;
// using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
// using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
// using BinusSchool.Persistence.SchedulingDb.Abstractions;
// using BinusSchool.Persistence.SchedulingDb.Entities;
// using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
// using BinusSchool.Persistence.SchedulingDb.Entities.User;
// using BinusSchool.Persistence.SchedulingDb.Entities.Student;
// using BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Validator;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Internal;
// using BinusSchool.Persistence.SchedulingDb.Entities.School;
// using BinusSchool.Auth.Authentications.Jwt;
// using Newtonsoft.Json;
// using Microsoft.Azure.WebJobs;

// namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
// {

//     public class SaveScheduleRealization2Handler : FunctionsHttpSingleHandler
//     {
//         private readonly ISchedulingDbContext _dbContext;

//         public SaveScheduleRealization2Handler(ISchedulingDbContext dbContext)
//         {
//             _dbContext = dbContext;
//         }

//         protected override async Task<ApiErrorResult<object>> Handler()
//         {
//             var body = await Request.ValidateBody<SaveScheduleRealizationRequest, SaveScheduleRealizationValidator>();

//             foreach(var dataBody in body.DataScheduleRealizations)
//             {
//                 // if(dataBody.IsCancel)
//                 // {
//                 //     predicate = predicate.And(x => x.IdBinusianOld == dataBody.IdUserTeacher);
//                 // }
//                 // else
//                 // {
//                 //     predicate = predicate.And(x => x.IdUser == dataBody.IdUserTeacher);
//                 // }

//                 var existScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
//                     .Where(x => x.ScheduleDate == dataBody.Date 
//                                 && x.ClassID == dataBody.ClassID
//                                 && x.SessionID == dataBody.SessionID
//                                 && x.IdLesson == dataBody.IdLesson
//                                 // && x.IdAcademicYear == dataBody.IdAcademicYear
//                                 && x.IdLevel == dataBody.IdLevel
//                                 && x.IdGrade == dataBody.IdGrade
//                                 && x.IdDay == dataBody.IdDay)
//                     .FirstOrDefaultAsync(CancellationToken);
                
//                 var existScheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
//                     .Where(x => x.ScheduleDate == dataBody.Date 
//                                 && x.IdBinusian == dataBody.IdUserTeacher
//                                 && x.ClassID == dataBody.ClassID
//                                 && x.SessionID == dataBody.SessionID
//                                 && x.IdLesson == dataBody.IdLesson
//                                 // && x.IdAcademicYear == dataBody.IdAcademicYear
//                                 && x.IdLevel == dataBody.IdLevel
//                                 && x.IdGrade == dataBody.IdGrade
//                                 && x.IdDay == dataBody.IdDay)
//                     .FirstOrDefaultAsync(CancellationToken);

//                 var dataUser = _dbContext.Entity<MsUser>();
//                 var dataVenue = _dbContext.Entity<MsVenue>();

//                 if(existScheduleRealization == null)
//                 {
//                     var addScheduleRealization = new TrScheduleRealization2
//                     {
//                         Id = Guid.NewGuid().ToString(),
//                         ScheduleDate = dataBody.Date,
//                         IdBinusian = dataBody.IdUserTeacher,
//                         IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher,
//                         IdVenue = dataBody.IdRegularVenue,
//                         IdVenueChange = dataBody.IdChangeVenue,
//                         ClassID = dataBody.ClassID,
//                         SessionID = dataBody.SessionID,
//                         StartTime = existScheduleLesson.StartTime,
//                         EndTime = existScheduleLesson.EndTime,
//                         IsCancel = dataBody.IsCancel,
//                         IsSendEmail = dataBody.IsSendEmail,
//                         NotesForSubtitutions = dataBody.NotesForSubtitutions,
//                         Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null,
//                         IdDay = dataBody.IdDay,
//                         DaysOfWeek = existScheduleLesson.DaysOfWeek,
//                         IdLesson = dataBody.IdLesson,
//                         IdAcademicYear = existScheduleLesson.IdAcademicYear,
//                         IdLevel = dataBody.IdLevel,
//                         IdGrade = dataBody.IdGrade
//                     };

//                     _dbContext.Entity<TrScheduleRealization2>().AddRange(addScheduleRealization);
//                 }
//                 else
//                 {
//                     existScheduleRealization.IdBinusianSubtitute = dataBody.IdUserSubtituteTeacher;
//                     existScheduleRealization.TeacherNameSubtitute = dataUser.Where(x => x.Id == dataBody.IdUserSubtituteTeacher).First().DisplayName;
//                     existScheduleRealization.IdVenueChange = dataBody.IdChangeVenue;
//                     existScheduleRealization.VenueNameChange = dataVenue.Where(x => x.Id == dataBody.IdChangeVenue).First().Description;
//                     existScheduleRealization.Status = dataBody.IsCancel ? "Cancelled" : (dataBody.IsSubtituteChange && dataBody.IsVenueChange) ? "Subtituted & Venue Change" : dataBody.IsSubtituteChange ? "Subtituted" : dataBody.IsVenueChange ? "Venue Change" : null;
//                     existScheduleRealization.IsCancel = dataBody.IsCancel;

//                     _dbContext.Entity<TrScheduleRealization2>().UpdateRange(existScheduleRealization);
//                 }
//             }

//             await _dbContext.SaveChangesAsync(CancellationToken);

//             return Request.CreateApiResult2();
//         }

//     }
// }
