using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetListScheduleRealizationV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetListScheduleRealizationV2Handler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListScheduleRealizationV2Request>(nameof(GetListScheduleRealizationV2Request.IdAcademicYear), nameof(GetListScheduleRealizationV2Request.IdLevel), nameof(GetListScheduleRealizationV2Request.StartDate), nameof(GetListScheduleRealizationV2Request.EndDate));

            var predicate = PredicateBuilder.Create<MsScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear);
            var predicateLesson = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);
            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization2>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear); 
            var predicateLessonTeacher = PredicateBuilder.Create<MsLessonTeacher>(e => true);
            var predicateSchedule = PredicateBuilder.Create<MsSchedule>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);
            var predicateHistory = PredicateBuilder.Create<HTrScheduleRealization2>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);

            if (param.IdUserTeacher != null)
                predicateLessonTeacher = predicateLessonTeacher.And(e => param.IdUserTeacher.Contains(e.IdUser));

            var listIdLesson = await _dbContext.Entity<MsLessonTeacher>()
                          .Include(e => e.Lesson)
                          .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                  && param.IdGrade.Contains(e.Lesson.IdGrade)
                                  && e.IsAttendance)
                          .Where(predicateLessonTeacher)
                          .Select(e => e.IdLesson)
                          .ToListAsync(CancellationToken);

            if(param.IdUserTeacher != null)
            {
                predicate = predicate.And(x => listIdLesson.Contains(x.IdLesson));
                predicateSchedule = predicateSchedule.And(x => listIdLesson.Contains(x.IdLesson));
            }
            
            if(!string.IsNullOrWhiteSpace(param.IdLevel))
            {
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);
                predicateSubtitute = predicateSubtitute.And(x => x.IdLevel == param.IdLevel);
                predicateSchedule = predicateSchedule.And(x => x.Lesson.Grade.IdLevel == param.IdLevel);
                predicateHistory = predicateHistory.And(x => x.Lesson.Grade.IdLevel == param.IdLevel);
            }

            if (param.IdGrade != null)
            {
                predicate = predicate.And(x => param.IdGrade.Contains(x.IdGrade));
                predicateLesson = predicateLesson.And(x => param.IdGrade.Contains(x.Lesson.Subject.IdGrade));
                predicateSubtitute = predicateSubtitute.And(x => param.IdGrade.Contains(x.IdGrade));
                predicateSchedule = predicateSchedule.And(x => param.IdGrade.Contains(x.Lesson.IdGrade));
                predicateHistory = predicateHistory.And(x => param.IdGrade.Contains(x.Lesson.IdGrade));
            }

            if (param.SessionID != null)
            {
                predicate = predicate.And(x => param.SessionID.Contains(x.SessionID));
                predicateSchedule = predicateSchedule.And(x => param.SessionID.Contains(x.Sessions.SessionID.ToString()));
            }

            if(param.IdVenue != null)
            {
                predicate = predicate.And(x => param.IdVenue.Contains(x.IdVenue));
                predicateSubtitute = predicateSubtitute.And(x => param.IdVenue.Contains(x.IdVenue));
            }

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.VenueName.ToUpper(), $"%{param.Search.ToUpper()}%")
                    || x.Lesson.LessonTeachers.Any(y => (y.Staff.FirstName + " " + y.Staff.LastName).ToUpper().Contains(param.Search.ToUpper()))
                    );

            var listSchedule = await _dbContext.Entity<MsSchedule>()
                                .Where(predicateSchedule)
                                .ToListAsync(CancellationToken);


            var query = _dbContext.Entity<MsScheduleLesson>()
                                 .Include(x => x.Venue)
                                 .Where(predicate);



            var getScheduleLesson = _dbContext.Entity<MsScheduleLesson>()
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataTeacher =  _dbContext.Entity<MsLessonTeacher>()
                                    .Include(e => e.Lesson).ThenInclude(e => e.LessonTeachers).ThenInclude(e => e.Staff)
                                    .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                            && param.IdGrade.Contains(e.Lesson.IdGrade)
                                            && e.Lesson.LessonTeachers.Any(y => y.IsAttendance));

            var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization2>()
                                 .Include(x => x.Staff)
                                 .Include(x => x.StaffSubtitute)
                                 .Where(predicateSubtitute);

            //ordering
            switch (param.OrderBy)
            {
                case "Date":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ScheduleDate)
                        : query.OrderBy(x => x.ScheduleDate);
                    break;

                case "Session":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SessionID)
                        : query.OrderBy(x => x.SessionID);
                    break;
            };

            IReadOnlyList<GetListScheduleRealizationV2Result> items;
            List<GetListScheduleRealizationV2Result> dataItems = new List<GetListScheduleRealizationV2Result>();

            items = await query
                .Select(x => new GetListScheduleRealizationV2Result
                {
                    Date = x.ScheduleDate,
                    SessionID = x.SessionID,
                    SessionStartTime = x.StartTime,
                    SessionEndTime = x.EndTime,
                    ClassID = x.ClassID,
                    IdVenueOld = x.IdVenue,
                    VenueNameOld = x.VenueName,
                    IdLesson = x.IdLesson,
                    IdAcademicYear = x.IdAcademicYear,
                    IdLevel = x.IdLevel,
                    IdGrade = x.IdGrade,
                    IdDay = x.IdDay,
                    IdSession = x.IdSession
                })
                .Distinct()
                .OrderBy(x => x.SessionID)
                .SetPagination(param)
            .ToListAsync(CancellationToken);

            var listHistory = await _dbContext.Entity<HTrScheduleRealization2>()
                                                .Where(predicateHistory)
                                                .ToListAsync(CancellationToken);


            foreach (var x in items)
            {
                var listTecaherSchedule = listSchedule
                                           .Where(e => e.IdLesson == x.IdLesson && e.IdDay == x.IdDay && e.IdSession == x.IdSession)
                                           .Select(e => e.IdUser).ToList();

                if (!listTecaherSchedule.Any())
                    continue;

                var getScheduleLessonByItem = getScheduleLesson
                                                .Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID)
                                                .ToList();
                var getDataSubtituteTeacherByItem = getDataSubtituteTeacher
                                                        .Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).ToList();
                var getDataTeacherByItem = getDataTeacher.Where(y => y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).ToList();

                var Ids = getScheduleLessonByItem.Select(y => y.Id).ToList();
                var Date = x.Date;
                var SessionID = x.SessionID;
                var IdSession = x.IdSession;
                var SessionStartTime = x.SessionStartTime;
                var SessionEndTime = x.SessionEndTime;
                var ClassID = x.ClassID;
                var IdVenue = x.IdVenueOld != null ? x.IdVenueOld : x.IdVenue;
                var VenueName = x.VenueNameOld != null ? x.VenueNameOld : x.VenueName;
                var ChangeVenue = new ItemValueVm(getDataSubtituteTeacherByItem.Select(y => y.IdVenueChange).FirstOrDefault() == null
                                    ? x.IdVenue
                                    : getDataSubtituteTeacherByItem.Select(y => y.IdVenueChange).FirstOrDefault(),
                                        getDataSubtituteTeacherByItem.Select(y => y.IdVenueChange).FirstOrDefault() == null
                                            ? x.VenueNameOld
                                            : getDataSubtituteTeacherByItem.Select(y => y.VenueNameChange).FirstOrDefault());

                var DataTeachers = getDataTeacherByItem
                                    .Where(e=> listTecaherSchedule.Contains(e.IdUser))
                                    .Select(y => new ListTeacher
                                    {
                                        Id = y.IdUser,
                                        Description = y.Staff.FirstName + " " + y.Staff.LastName
                                    }).Distinct().OrderBy(x => x.Description).ToList();

                var DataSubtituteTeachers = getDataSubtituteTeacherByItem.Where(y => y.IsCancel == false).Any() 
                                            ? getDataTeacherByItem.Select(y => new ListSubtituteTeacher
                                                {
                                                    Id = getDataSubtituteTeacherByItem
                                                            .Where(z => z.IdBinusian == y.IdUser).Select(e=>e.IdBinusianSubtitute).FirstOrDefault(),
                                                    Code = y.Staff.FirstName + " " + y.Staff.LastName,
                                                    Description = getDataSubtituteTeacherByItem
                                                                    .Where(z => z.IdBinusian == y.IdUser)
                                                                    .Select(z => z.StaffSubtitute.FirstName + " " + z.StaffSubtitute.LastName).FirstOrDefault()
                                                }).Distinct().OrderBy(x => x.Code).ToList() 
                                            : DataTeachers.Select(y => new ListSubtituteTeacher
                                                {
                                                    Id = y.Id,
                                                    Description = y.Description
                                                }).Distinct().OrderBy(x => x.Description).ToList();

                var EntryStatusBy = "System";
                var EntryStatusDate = getScheduleLessonByItem.Select(y => y.ScheduleDate).FirstOrDefault();
                var Status = getDataSubtituteTeacherByItem.FirstOrDefault() != null ? getDataSubtituteTeacherByItem.Select(y => y.Status).FirstOrDefault() : null;
                var CanEnableDisable = x.Date.Date < _dateTime.ServerTime.Date ? false : true;
                var IsCancelClass = getDataSubtituteTeacherByItem.Where(y => y.IsCancel).FirstOrDefault() != null ? true : false;
                var IsSendEmail = getDataSubtituteTeacherByItem.Where(y => y.IsSendEmail == true).Count() > 0 ? true : false;
                var CanPrint = getDataSubtituteTeacherByItem
                                .Where(y => y.Status == "Subtituted" || y.Status == "Venue Change" || y.Status == "Subtituted & Venue Change")
                                .FirstOrDefault() != null ? true : false;
                var IsSetScheduleRealization = getDataSubtituteTeacherByItem
                                                    .Where(y => y.Status == "Subtituted" || y.Status == "Venue Change" || y.Status == "Subtituted & Venue Change")
                                                    .FirstOrDefault() != null ? true : false;
                var IdLesson = x.IdLesson;
                var IdAcademicYear = x.IdAcademicYear;
                var IdLevel = x.IdLevel;
                var IdGrade = x.IdGrade;
                var IdDay = x.IdDay;
                var IdScheduleRealization = getDataSubtituteTeacherByItem.Select(x => x.Id).FirstOrDefault();
                var haveHistory = listHistory.Any(x => x.IdScheduleRealization2 == IdScheduleRealization);
                //item.CanModified = haveHistory;

                dataItems.Add(new GetListScheduleRealizationV2Result
                {
                    Ids = Ids,
                    Date = Date,
                    SessionID = SessionID,
                    IdSession = IdSession,
                    SessionStartTime = SessionStartTime,
                    SessionEndTime = SessionEndTime,
                    ClassID = ClassID,
                    IdVenue = IdVenue,
                    VenueName = VenueName,
                    ChangeVenue = ChangeVenue,
                    DataTeachers = DataTeachers,
                    DataSubtituteTeachers = DataSubtituteTeachers,
                    EntryStatusBy = EntryStatusBy,
                    EntryStatusDate = EntryStatusDate,
                    Status = Status,
                    CanEnableDisable = CanEnableDisable,
                    IsCancelClass = IsCancelClass,
                    IsSendEmail = IsSendEmail,
                    CanPrint = CanPrint,
                    IsSetScheduleRealization = IsSetScheduleRealization,
                    IdLesson = x.IdLesson,
                    IdAcademicYear = x.IdAcademicYear,
                    IdLevel = x.IdLevel,
                    IdGrade = x.IdGrade,
                    IdDay = x.IdDay,
                    IdScheduleRealization = IdScheduleRealization,
                    CanModified = haveHistory
                });
            }

            //dataItems = items
            //        .Select(x => new GetListScheduleRealizationV2Result
            //        {
            //            Ids = getScheduleLesson.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Id).ToList(),
            //            Date = x.Date,
            //            SessionID = x.SessionID,
            //            IdSession = x.IdSession,
            //            SessionStartTime = x.SessionStartTime,
            //            SessionEndTime = x.SessionEndTime,
            //            ClassID = x.ClassID,
            //            IdVenue = x.IdVenueOld != null ? x.IdVenueOld : x.IdVenue,
            //            VenueName = x.VenueNameOld != null ? x.VenueNameOld : x.VenueName,
            //            ChangeVenue = new ItemValueVm(getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.IdVenueChange).FirstOrDefault() == null ? x.IdVenue : getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.IdVenueChange).FirstOrDefault(), getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.IdVenueChange).FirstOrDefault() == null ? x.VenueNameOld : getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.VenueNameChange).FirstOrDefault()),
            //            DataTeachers =  getDataTeacher.Where(y => y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).Select(y => new ListTeacher
            //                            {
            //                                Id = y.IdUser,
            //                                Description = y.Staff.FirstName + " " + y.Staff.LastName
            //                            }).Distinct().OrderBy(x => x.Description).ToList(),
            //            DataSubtituteTeachers = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel == false).Any() ? 
            //                            getDataTeacher.Where(y => y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).Select(y => new ListSubtituteTeacher
            //                            {
            //                                Id = getDataSubtituteTeacher.Where(z => z.ScheduleDate == x.Date && z.SessionID == x.SessionID && z.ClassID == x.ClassID && z.IdBinusian == y.IdUser).FirstOrDefault().IdBinusianSubtitute,
            //                                Code = y.Staff.FirstName + " " + y.Staff.LastName,
            //                                Description = getDataSubtituteTeacher.Where(z => z.ScheduleDate == x.Date && z.SessionID == x.SessionID && z.ClassID == x.ClassID && z.IdBinusian == y.IdUser).Select(z => z.StaffSubtitute.FirstName + " " + z.StaffSubtitute.LastName).FirstOrDefault()
            //                            }).Distinct().OrderBy(x => x.Code).ToList() :
            //                            // getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new ListSubtituteTeacher
            //                            // {
            //                            //     Id = y.IdBinusianSubtitute,
            //                            //     Description = y.StaffSubtitute.FirstName + " " + y.StaffSubtitute.LastName
            //                            // }).Distinct().ToList() :
            //                            getDataTeacher.Where(y => y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).Select(y => new ListSubtituteTeacher
            //                            {
            //                                Id = y.IdUser,
            //                                Description = y.Staff.FirstName + " " + y.Staff.LastName
            //                            }).Distinct().OrderBy(x => x.Description).ToList(),
            //            EntryStatusBy = "System",
            //            EntryStatusDate = getScheduleLesson.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.ScheduleDate).FirstOrDefault(),
            //            Status = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).FirstOrDefault() != null ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Status).FirstOrDefault() : null,
            //            CanEnableDisable = x.Date.Date < _dateTime.ServerTime.Date ? false : true,
            //            IsCancelClass = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault() != null ? true : false,
            //            IsSendEmail = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsSendEmail == true).Count() > 0 ? true : false,
            //            CanPrint = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && (y.Status == "Subtituted" || y.Status == "Venue Change" || y.Status == "Subtituted & Venue Change")).FirstOrDefault() != null ? true : false,
            //            IsSetScheduleRealization = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && (y.Status == "Subtituted" || y.Status == "Venue Change" || y.Status == "Subtituted & Venue Change")).FirstOrDefault() != null ? true : false,
            //            IdLesson = x.IdLesson,
            //            IdAcademicYear = x.IdAcademicYear,
            //            IdLevel = x.IdLevel,
            //            IdGrade = x.IdGrade,
            //            IdDay = x.IdDay,
            //            IdScheduleRealization = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(x => x.Id).FirstOrDefault()
            //        }    
            //    ).ToList();

            //foreach (var item in dataItems)
            //{
            //    var haveHistory = _dbContext.Entity<HTrScheduleRealization2>().Any(x => x.IdScheduleRealization2 == item.IdScheduleRealization);
            //    item.CanModified = haveHistory;

            //    var idLesson = item.IdLesson;
            //    var listTecaherSchedule = listSchedule.Where(e => e.IdLesson == item.IdLesson && e.IdDay == item.IdDay && e.IdSession == item.IdSession).ToList();

            //    foreach(var itemTeacher in item.DataTeachers)
            //    {
            //        itemTeacher.IsShow= listTecaherSchedule.Where(e=>e.IdUser==itemTeacher.Id).Any();
            //    }
            //}

            //dataItems = dataItems.Where(e => e.DataTeachers.Any(f => f.IsShow)).ToList();

            var countAll = await query
                .Select(x => new GetListScheduleRealizationResult
                    {
                        Date = x.ScheduleDate,
                        SessionID = x.SessionID,
                        SessionStartTime = x.StartTime,
                        SessionEndTime = x.EndTime,
                        ClassID = x.ClassID,
                        IdVenue = x.IdVenue
                    }
                )
                .Distinct().CountAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : countAll;

            return Request.CreateApiResult2(dataItems as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
