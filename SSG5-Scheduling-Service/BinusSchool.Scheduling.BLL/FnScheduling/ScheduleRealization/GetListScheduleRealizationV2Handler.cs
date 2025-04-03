// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using BinusSchool.Common.Abstractions;
// using BinusSchool.Common.Extensions;
// using BinusSchool.Common.Functions.Handler;
// using BinusSchool.Common.Model;
// using BinusSchool.Common.Model.Abstractions;
// using BinusSchool.Common.Model.Enums;
// using BinusSchool.Common.Utils;
// using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
// using BinusSchool.Persistence.SchedulingDb.Abstractions;
// using BinusSchool.Persistence.SchedulingDb.Entities;
// using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
// using Microsoft.EntityFrameworkCore;

// namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
// {
//     public class GetListScheduleRealizationV2Handler : FunctionsHttpSingleHandler
//     {
//         private readonly ISchedulingDbContext _dbContext;
//         private readonly IMachineDateTime _dateTime;
//         public GetListScheduleRealizationV2Handler(
//             ISchedulingDbContext dbContext,
//             IMachineDateTime dateTime)
//         {
//             _dbContext = dbContext;
//             _dateTime = dateTime;
//         }
//         private static readonly string[] _columns = { "Date", "Session" };

//         protected async override Task<ApiErrorResult<object>> Handler()
//         {
//             var param = Request.ValidateParams<GetListScheduleRealizationRequest>(nameof(GetListScheduleRealizationRequest.IdAcademicYear), nameof(GetListScheduleRealizationRequest.IdLevel), nameof(GetSessionByTeacherDateReq.StartDate), nameof(GetSessionByTeacherDateReq.EndDate));

//             var predicate = PredicateBuilder.Create<MsScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear);
//             var predicateLesson = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);
//             var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization2>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear);
            
//             var predicateLessonTeacher = PredicateBuilder.Create<MsLessonTeacher>(e => true);

//             if(param.IdUserTeacher != null)
//                 predicateLessonTeacher = predicateLessonTeacher.And(e => param.IdUserTeacher.Contains(e.IdUser));

//             var listIdLesson = await _dbContext.Entity<MsLessonTeacher>()
//                           .Include(e => e.Lesson)
//                           .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
//                                   && param.IdGrade.Contains(e.Lesson.IdGrade)
//                                   && e.IsAttendance)
//                           .Where(predicateLessonTeacher)
//                           .Select(e => e.IdLesson)
//                           .ToListAsync(CancellationToken);

//             if(param.IdUserTeacher != null)
//                 predicate = predicate.And(x => listIdLesson.Contains(x.IdLesson));
            
//             if(!string.IsNullOrWhiteSpace(param.IdLevel))
//             {
//                 predicate = predicate.And(x => x.IdLevel == param.IdLevel);
//                 predicateSubtitute = predicateSubtitute.And(x => x.IdLevel == param.IdLevel);
//             }

//             if(param.IdGrade != null)
//                 {
//                     predicate = predicate.And(x => param.IdGrade.Contains(x.IdGrade));
//                     predicateLesson = predicateLesson.And(x => param.IdGrade.Contains(x.Lesson.Subject.IdGrade));
//                     predicateSubtitute = predicateSubtitute.And(x => param.IdGrade.Contains(x.IdGrade));
//                 }

//             if(param.SessionID != null)
//                 predicate = predicate.And(x => param.SessionID.Contains(x.SessionID));

//             if(param.IdVenue != null)
//             {
//                 predicate = predicate.And(x => param.IdVenue.Contains(x.IdVenue));
//                 predicateSubtitute = predicateSubtitute.And(x => param.IdVenue.Contains(x.IdVenue));
//             }

//             if (!string.IsNullOrWhiteSpace(param.Search))
//                 predicate = predicate.And(x
//                     => EF.Functions.Like(x.VenueName.ToUpper(), $"%{param.Search.ToUpper()}%")
//                     // || EF.Functions.Like(x.VenueName.ToUpper(), $"%{param.Search.ToUpper()}%")
//                     );

//             var query = _dbContext.Entity<MsScheduleLesson>()
//                                  .Include(x => x.Venue)
//                                  .Where(predicate);

//             var getScheduleLesson = _dbContext.Entity<MsScheduleLesson>()
//                                  .Include(x => x.Venue)
//                                  .Where(predicate);

//             var getDataTeacher =  _dbContext.Entity<MsLessonTeacher>()
//                                     .Include(e => e.Lesson).ThenInclude(e => e.LessonTeachers).ThenInclude(e => e.Staff)
//                                     .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
//                                             && param.IdGrade.Contains(e.Lesson.IdGrade)
//                                             && e.Lesson.LessonTeachers.Any(y => y.IsAttendance));
            
//             var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization2>()
//                                  .Include(x => x.Staff)
//                                  .Include(x => x.StaffSubtitute)
//                                  .Where(predicateSubtitute);

//             //ordering
//             switch (param.OrderBy)
//             {
//                 case "Date":
//                     query = param.OrderType == OrderType.Desc
//                         ? query.OrderByDescending(x => x.ScheduleDate)
//                         : query.OrderBy(x => x.ScheduleDate);
//                     break;

//                 case "Session":
//                     query = param.OrderType == OrderType.Desc
//                         ? query.OrderByDescending(x => x.SessionID)
//                         : query.OrderBy(x => x.SessionID);
//                     break;
//             };

//             IReadOnlyList<GetListScheduleRealizationResult> items;
//             IReadOnlyList<GetListScheduleRealizationResult> dataItems;

//             items = await query
//                 .Select(x => new GetListScheduleRealizationResult
//                     {
                        
//                         Date = x.ScheduleDate,
//                         SessionID = x.SessionID,
//                         SessionStartTime = x.StartTime,
//                         SessionEndTime = x.EndTime,
//                         ClassID = x.ClassID,
//                         IdVenue = x.IdVenue,
//                         VenueName = x.VenueName,
//                         IdLesson = x.IdLesson,
//                         IdAcademicYear = x.IdAcademicYear,
//                         IdLevel = x.IdLevel,
//                         IdGrade = x.IdGrade,
//                         IdDay = x.IdDay
//                     }
//                 )
//                 .Distinct()
//                 .OrderBy(x => x.SessionID)
//                 .SetPagination(param)
//                 .ToListAsync(CancellationToken);

//             dataItems = items
//                     .Select(x => new GetListScheduleRealizationResult
//                     {
//                         Ids = getScheduleLesson.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Id).ToList(),
//                         Date = x.Date,
//                         SessionID = x.SessionID,
//                         SessionStartTime = x.SessionStartTime,
//                         SessionEndTime = x.SessionEndTime,
//                         ClassID = x.ClassID,
//                         IdVenue = x.IdVenueOld != null ? x.IdVenueOld : x.IdVenue,
//                         VenueName = x.VenueNameOld != null ? x.VenueNameOld : x.VenueName,
//                         ChangeVenue = new ItemValueVm(x.IdVenue,x.VenueName),
//                         DataTeachers =  getDataTeacher.Where(y => y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).Select(y => new ListTeacher
//                                         {
//                                             Id = y.IdUser,
//                                             Description = y.Staff.FirstName + " " + y.Staff.LastName
//                                         }).Distinct().ToList(),
//                         DataSubtituteTeachers = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel == false).Any() ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new ListSubtituteTeacher
//                                         {
//                                             Id = y.IdBinusianSubtitute,
//                                             Description = y.StaffSubtitute.FirstName + " " + y.StaffSubtitute.LastName
//                                         }).Distinct().ToList() :
//                                         getDataTeacher.Where(y => y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).Select(y => new ListSubtituteTeacher
//                                         {
//                                             Id = y.IdUser,
//                                             Description = y.Staff.FirstName + " " + y.Staff.LastName
//                                         }).Distinct().ToList(),
//                         // EntryStatusBy = "System",
//                         // EntryStatusDate = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.DateIn).First(),
//                         Status = x.IsSetScheduleRealization == true ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Status).FirstOrDefault() : null,
//                         CanEnableDisable = x.Date.Date < _dateTime.ServerTime.Date ? false : true,
//                         IsCancelClass = x.IsCancelClass,
//                         IsSendEmail = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsSendEmail == true).Count() > 0 ? true : false,
//                         CanPrint = x.IsSetScheduleRealization,
//                         IdLesson = x.IdLesson,
//                         IdAcademicYear = x.IdAcademicYear,
//                         IdLevel = x.IdLevel,
//                         IdGrade = x.IdGrade,
//                         IdDay = x.IdDay
//                     }    
//                 ).ToList();

//             var countAll = await query
//                 .Select(x => new GetListScheduleRealizationResult
//                     {
                        
//                         Date = x.ScheduleDate,
//                         SessionID = x.SessionID,
//                         SessionStartTime = x.StartTime,
//                         SessionEndTime = x.EndTime,
//                         ClassID = x.ClassID,
//                         IdVenue = x.IdVenue
//                     }
//                 )
//                 .Distinct().CountAsync(CancellationToken);

//             var count = param.CanCountWithoutFetchDb(items.Count)
//                 ? items.Count
//                 : countAll;

//             return Request.CreateApiResult2(dataItems as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
//         }
//     }
// }
