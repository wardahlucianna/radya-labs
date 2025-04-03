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
// using Microsoft.EntityFrameworkCore;

// namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
// {
//     public class GetListScheduleRealizationByTeacher2Handler : FunctionsHttpSingleHandler
//     {
//         private readonly ISchedulingDbContext _dbContext;
//         private readonly IMachineDateTime _dateTime;
//         public GetListScheduleRealizationByTeacher2Handler(
//             ISchedulingDbContext dbContext,
//             IMachineDateTime dateTime)
//         {
//             _dbContext = dbContext;
//             _dateTime = dateTime;
//         }
//         private static readonly string[] _columns = { "Date", "Session" };

//         protected async override Task<ApiErrorResult<object>> Handler()
//         {
//             var param = Request.ValidateParams<GetListScheduleRealizationByTeacherRequest>(nameof(GetListScheduleRealizationByTeacherRequest.IdAcademicYear),
//                                                                                            nameof(GetListScheduleRealizationByTeacherRequest.StartDate),
//                                                                                            nameof(GetListScheduleRealizationByTeacherRequest.EndDate),
//                                                                                            nameof(GetListScheduleRealizationByTeacherRequest.IdUserTeacher),
//                                                                                            nameof(GetListScheduleRealizationByTeacherRequest.ClassID));

//             var predicate = PredicateBuilder.Create<MsScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear && param.ClassID.Contains(x.ClassID));
//             var predicateLessonTeacher = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear && x.IdUser == param.IdUserTeacher && x.IsAttendance);
//             var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization2>(x => x.ScheduleDate >= param.StartDate 
//                                                                                         && x.ScheduleDate <= param.EndDate 
//                                                                                         && x.IdAcademicYear == param.IdAcademicYear
//                                                                                         && x.IdBinusian == param.IdUserTeacher
//                                                                                         && param.ClassID.Contains(x.ClassID));

//             var listIdLesson = await _dbContext.Entity<MsLessonTeacher>()
//                           .Include(e => e.Lesson)
//                           .Where(predicateLessonTeacher)
//                           .Select(e => e.IdLesson)
//                           .ToListAsync(CancellationToken);

//             predicate = predicate.And(x => listIdLesson.Contains(x.IdLesson));
            
//             var query = _dbContext.Entity<MsScheduleLesson>()
//                                  .Include(x => x.Venue)
//                                  .Where(predicate);

//             var getScheduleLesson = _dbContext.Entity<MsScheduleLesson>()
//                                  .Include(x => x.Venue)
//                                  .Where(predicate);

//             var getDataTeacher =  _dbContext.Entity<MsLessonTeacher>()
//                                     .Include(e => e.Lesson).ThenInclude(e => e.LessonTeachers).ThenInclude(e => e.Staff)
//                                     .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
//                                             && listIdLesson.Contains(e.IdLesson)
//                                             && e.Lesson.LessonTeachers.Any(y => y.IsAttendance));

//             var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization2>()
//                                  .Where(predicateSubtitute);

//             //ordering
//             switch (param.OrderBy)
//             {
//                 // case "Date":
//                 //     query = param.OrderType == OrderType.Desc
//                 //         ? query.OrderByDescending(x => x.ScheduleDate)
//                 //         : query.OrderBy(x => x.ScheduleDate);
//                 //     break;

//                 case "Session":
//                     query = param.OrderType == OrderType.Desc
//                         ? query.OrderByDescending(x => x.SessionID)
//                         : query.OrderBy(x => x.SessionID);
//                     break;
//             };

//             IReadOnlyList<GetListScheduleRealizationByTeacherResult> items;
//             IReadOnlyList<GetListScheduleRealizationByTeacherResult> dataItems;

//             items = await query
//                 .Select(x => new GetListScheduleRealizationByTeacherResult
//                     {
//                         ClassID = x.ClassID,
//                         DaysOfWeek = x.DaysOfWeek,
//                         SessionID = x.SessionID,
//                         SessionStartTime = x.StartTime,
//                         SessionEndTime = x.EndTime,
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
//                     .Select(x => new GetListScheduleRealizationByTeacherResult
//                     {
//                         Ids = getScheduleLesson.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek).Select(y => y.Id).ToList(),
//                         ClassID = x.ClassID,
//                         DaysOfWeek = x.DaysOfWeek,
//                         SessionID = x.SessionID,
//                         SessionStartTime = x.SessionStartTime,
//                         SessionEndTime = x.SessionEndTime,
//                         IdVenue = x.IdVenueOld != null ? x.IdVenueOld : x.IdVenue,
//                         VenueName = x.VenueNameOld != null ? x.VenueNameOld : x.VenueName,
//                         ChangeVenue = new ItemValueVm(x.IdVenue,x.VenueName),
//                         DataSubtituteTeachers = getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher && x.IsCancelClass == false).Count() > 0 ? getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher && y.IsCancel == false).Select(y => new SubtituteTeacher
//                                         {
//                                             Id = y.IdBinusianSubtitute,
//                                             Description = y.TeacherNameSubtitute
//                                         }).First() :
//                                         getDataTeacher.Where(y => y.IdUser == param.IdUserTeacher).Select(y => new SubtituteTeacher
//                                         {
//                                             Id = y.IdUser,
//                                             Description = y.Staff.FirstName + " " + y.Staff.LastName
//                                         }).First(),
//                         // EntryStatusBy = "System",
//                         // EntryStatusDate = getDataTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek).Select(y => y.DateIn).First() != null ? getDataTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek).Select(y => y.DateIn).First() : null,
//                         Status = x.IsSetScheduleRealization == true && getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher).First() != null ? getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher).Select(y => y.Status).FirstOrDefault() : null,
//                         CanEnableDisable = param.EndDate.Date < _dateTime.ServerTime.Date ? false : true,
//                         IsCancelClass = x.IsCancelClass,
//                         IsSendEmail = getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher && y.IsSendEmail == true).Count() > 0 ? true : false,
//                         CanPrint = x.IsSetScheduleRealization,
//                         IdHomeroom = x.IdHomeroom,
//                         IdLesson = x.IdLesson,
//                         IdAcademicYear = x.IdAcademicYear,
//                         IdLevel = x.IdLevel,
//                         IdGrade = x.IdGrade,
//                         IdDay = x.IdDay
//                     }    
//                 ).ToList();

//             var countAll = await query
//                 .Select(x => new GetListScheduleRealizationByTeacherResult
//                     {
//                         SessionID = x.SessionID,
//                         SessionStartTime = x.StartTime,
//                         SessionEndTime = x.EndTime,
//                         ClassID = x.ClassID,
//                         IdVenue = x.IdVenue,
//                         DaysOfWeek = x.DaysOfWeek
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
