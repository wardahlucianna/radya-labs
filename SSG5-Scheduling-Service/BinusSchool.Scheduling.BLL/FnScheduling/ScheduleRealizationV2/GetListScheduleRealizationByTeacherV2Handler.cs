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
using Microsoft.EntityFrameworkCore;
using NPOI.Util;
using Org.BouncyCastle.Crypto;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetListScheduleRealizationByTeacherV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetListScheduleRealizationByTeacherV2Handler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListScheduleRealizationByTeacherV2Request>(nameof(GetListScheduleRealizationByTeacherV2Request.IdAcademicYear),
                                                                                           nameof(GetListScheduleRealizationByTeacherV2Request.StartDate),
                                                                                           nameof(GetListScheduleRealizationByTeacherV2Request.EndDate),
                                                                                           nameof(GetListScheduleRealizationByTeacherV2Request.IdUserTeacher),
                                                                                           nameof(GetListScheduleRealizationByTeacherV2Request.ClassID));

            var predicate = PredicateBuilder.Create<MsScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear && param.ClassID.Contains(x.ClassID));
            var predicateLessonTeacher = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear && x.IdUser == param.IdUserTeacher && x.IsAttendance);
            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization2>(x => x.ScheduleDate >= param.StartDate
                                                                                        && x.ScheduleDate <= param.EndDate
                                                                                        && x.IdAcademicYear == param.IdAcademicYear
                                                                                        && x.IdBinusian == param.IdUserTeacher
                                                                                        && param.ClassID.Contains(x.ClassID));

            var predicateSchedule = PredicateBuilder.Create<MsSchedule>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear
                                                                                    && param.ClassID.Contains(x.Lesson.ClassIdGenerated));
            var predicateHistory = PredicateBuilder.Create<HTrScheduleRealization2>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);

            var listIdLesson = await _dbContext.Entity<MsLessonTeacher>()
                          .Include(e => e.Lesson)
                          .Where(predicateLessonTeacher)
                          .Select(e => e.IdLesson)
                          .ToListAsync(CancellationToken);

            predicate = predicate.And(x => listIdLesson.Contains(x.IdLesson));

            var query = _dbContext.Entity<MsScheduleLesson>()
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getScheduleLesson = _dbContext.Entity<MsScheduleLesson>()
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataTeacher = _dbContext.Entity<MsLessonTeacher>()
                                    .Include(e => e.Lesson).ThenInclude(e => e.LessonTeachers).ThenInclude(e => e.Staff)
                                    .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                            && listIdLesson.Contains(e.IdLesson)
                                            && e.Lesson.LessonTeachers.Any(y => y.IsAttendance));

            var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization2>()
                                 .Where(predicateSubtitute);

            var listSchedule = _dbContext.Entity<MsSchedule>()
                                 .Where(predicateSchedule);

            //ordering
            switch (param.OrderBy)
            {
                // case "Date":
                //     query = param.OrderType == OrderType.Desc
                //         ? query.OrderByDescending(x => x.ScheduleDate)
                //         : query.OrderBy(x => x.ScheduleDate);
                //     break;

                case "Session":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SessionID)
                        : query.OrderBy(x => x.SessionID);
                    break;
            };

            IReadOnlyList<GetListScheduleRealizationByTeacherV2Result> items;
            List<GetListScheduleRealizationByTeacherV2Result> dataItems = new List<GetListScheduleRealizationByTeacherV2Result>();

            items = await query
                .Select(x => new GetListScheduleRealizationByTeacherV2Result
                {
                    ClassID = x.ClassID,
                    DaysOfWeek = x.DaysOfWeek,
                    SessionID = x.SessionID,
                    SessionStartTime = x.StartTime,
                    SessionEndTime = x.EndTime,
                    IdVenue = x.IdVenue,
                    VenueName = x.VenueName,
                    IdLesson = x.IdLesson,
                    IdAcademicYear = x.IdAcademicYear,
                    IdLevel = x.IdLevel,
                    IdGrade = x.IdGrade,
                    IdDay = x.IdDay
                }
                )
                .Distinct()
                .OrderBy(x => x.SessionID)
                .SetPagination(param)
                .ToListAsync(CancellationToken);

            var listHistory = await _dbContext.Entity<HTrScheduleRealization2>()
                                               .Where(predicateHistory)
                                               .ToListAsync(CancellationToken);

            foreach (var x in items)
            {
                var getScheduleLessonByItem = getScheduleLesson
                                                .Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek)
                                                .ToList();
                var getDataSubtituteTeacherByItem = getDataSubtituteTeacher
                                                        .Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID)
                                                        .ToList();
                var getDataTeacherByItem = getDataTeacher.Where(y => y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).ToList();

                var Ids = getScheduleLessonByItem.Select(y => y.Id).ToList();
                var ClassID = x.ClassID;
                var DaysOfWeek = x.DaysOfWeek;
                var SessionID = x.SessionID;
                var SessionStartTime = x.SessionStartTime;
                var SessionEndTime = x.SessionEndTime;
                var IdVenue = x.IdVenueOld != null ? x.IdVenueOld : x.IdVenue;
                var VenueName = x.VenueNameOld != null ? x.VenueNameOld : x.VenueName;
                var ChangeVenue = new ItemValueVm(getDataSubtituteTeacherByItem.FirstOrDefault() == null
                                        ? x.IdVenue
                                        : getDataSubtituteTeacherByItem.Select(y => y.IdVenueChange).FirstOrDefault(),
                                            getDataSubtituteTeacherByItem.Select(y => y.IdVenueChange).FirstOrDefault() == null
                                            ? x.VenueNameOld
                                            : getDataSubtituteTeacherByItem.Select(y => y.VenueNameChange).FirstOrDefault());
                var DataSubtituteTeachers = getDataSubtituteTeacherByItem.Where(y => x.IsCancelClass == false).Any()
                                            ?
                                                getDataTeacherByItem.Select(y => new SubtituteTeacher
                                                {
                                                    Id = getDataSubtituteTeacherByItem
                                                        .Where(z => z.IdBinusian == y.IdUser).Select(e => e.IdBinusianSubtitute)
                                                        .FirstOrDefault(),
                                                    Description = getDataSubtituteTeacherByItem
                                                                    .Where(z => z.IdBinusian == y.IdUser)
                                                                    .Select(z => z.StaffSubtitute.FirstName + " " + z.StaffSubtitute.LastName)
                                                                    .FirstOrDefault()
                                                }).First()
                                            :
                                                getDataTeacherByItem.Where(y => y.IdUser == param.IdUserTeacher)
                                                .Select(y => new SubtituteTeacher
                                                {
                                                    Id = y.IdUser,
                                                    Description = y.Staff.FirstName + " " + y.Staff.LastName
                                                }).First();
                var EntryStatusBy = "System";
                var EntryStatusDate = getScheduleLessonByItem.Select(y => y.ScheduleDate).FirstOrDefault();
                var Status = getDataSubtituteTeacherByItem.FirstOrDefault() != null
                            ? getDataSubtituteTeacherByItem.Select(y => y.Status).FirstOrDefault()
                            : null;
                var CanEnableDisable = param.EndDate.Date < _dateTime.ServerTime.Date ? false : true;
                var IsCancelClass = getDataSubtituteTeacherByItem.Where(y => y.IsCancel).FirstOrDefault() != null ? true : false;
                var IsSendEmail = getDataSubtituteTeacherByItem.Where(y => y.IsSendEmail == true).Count() > 0 ? true : false;
                var CanPrint = getDataSubtituteTeacherByItem
                                .Where(y => y.Status == "Subtituted"
                                        || y.Status == "Venue Change"
                                        || y.Status == "Subtituted & Venue Change")
                                .FirstOrDefault() != null ? true : false;
                var IsSetScheduleRealization = getDataSubtituteTeacherByItem
                                                    .Where(y => y.Status == "Subtituted"
                                                                || y.Status == "Venue Change"
                                                                || y.Status == "Subtituted & Venue Change")
                                                    .FirstOrDefault() != null ? true : false;
                var IdLesson = x.IdLesson;
                var IdAcademicYear = x.IdAcademicYear;
                var IdLevel = x.IdLevel;
                var IdGrade = x.IdGrade;
                var IdDay = x.IdDay;
                var IdScheduleRealization = getDataSubtituteTeacherByItem.Select(x => x.Id).FirstOrDefault();
                var haveHistory = listHistory.Any(x => x.IdScheduleRealization2 == IdScheduleRealization);

                dataItems.Add(new GetListScheduleRealizationByTeacherV2Result
                {
                    Ids = Ids,
                    ClassID = ClassID,
                    DaysOfWeek = DaysOfWeek,
                    SessionID = SessionID,
                    SessionStartTime = SessionStartTime,
                    SessionEndTime = SessionEndTime,
                    IdVenue = IdVenue,
                    VenueName = VenueName,
                    ChangeVenue = ChangeVenue,
                    DataSubtituteTeachers = DataSubtituteTeachers,
                    EntryStatusBy = EntryStatusBy,
                    EntryStatusDate = EntryStatusDate,
                    Status = Status,
                    CanEnableDisable = CanEnableDisable,
                    IsCancelClass = IsCancelClass,
                    IsSendEmail = IsSendEmail,
                    CanPrint = CanPrint,
                    IsSetScheduleRealization = IsSetScheduleRealization,
                    IdLesson = IdLesson,
                    IdAcademicYear = IdAcademicYear,
                    IdLevel = IdLevel,
                    IdGrade = IdGrade,
                    IdDay = IdDay,
                    IdScheduleRealization = IdScheduleRealization,
                    CanModified = haveHistory
                });
            };

            //dataItems = items
            //        .Select(x => new GetListScheduleRealizationByTeacherV2Result
            //        {
            //            Ids = getScheduleLesson.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek).Select(y => y.Id).ToList(),
            //            ClassID = x.ClassID,
            //            DaysOfWeek = x.DaysOfWeek,
            //            SessionID = x.SessionID,
            //            SessionStartTime = x.SessionStartTime,
            //            SessionEndTime = x.SessionEndTime,
            //            IdVenue = x.IdVenueOld != null ? x.IdVenueOld : x.IdVenue,
            //            VenueName = x.VenueNameOld != null ? x.VenueNameOld : x.VenueName,
            //            ChangeVenue = new ItemValueVm(getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.IdVenueChange).FirstOrDefault() == null ? x.IdVenue : getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.IdVenueChange).FirstOrDefault(), getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.IdVenueChange).FirstOrDefault() == null ? x.VenueNameOld : getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.VenueNameChange).FirstOrDefault()),
            //            DataSubtituteTeachers = getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && x.IsCancelClass == false).Any() ? 
            //            getDataTeacher.Where(y => y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).Select(y => new SubtituteTeacher
            //            {
            //                Id = getDataSubtituteTeacher.Where(z => z.DaysOfWeek == x.DaysOfWeek && z.SessionID == x.SessionID && z.ClassID == x.ClassID && z.IdBinusian == y.IdUser).FirstOrDefault().IdBinusianSubtitute,
            //                Description = getDataSubtituteTeacher.Where(z => z.DaysOfWeek == x.DaysOfWeek && z.SessionID == x.SessionID && z.ClassID == x.ClassID && z.IdBinusian == y.IdUser).Select(z => z.StaffSubtitute.FirstName + " " + z.StaffSubtitute.LastName).FirstOrDefault()
            //            }).First() :
            //                            getDataTeacher.Where(y => y.IdUser == param.IdUserTeacher && y.IdLesson == x.IdLesson && y.Lesson.ClassIdGenerated == x.ClassID).Select(y => new SubtituteTeacher
            //                            {
            //                                Id = y.IdUser,
            //                                Description = y.Staff.FirstName + " " + y.Staff.LastName
            //                            }).First(),
            //            EntryStatusBy = "System",
            //            EntryStatusDate = getScheduleLesson.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.ScheduleDate).FirstOrDefault(),
            //            Status = getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID).FirstOrDefault() != null ? getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Status).FirstOrDefault() : null,
            //            CanEnableDisable = param.EndDate.Date < _dateTime.ServerTime.Date ? false : true,
            //            IsCancelClass = getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault() != null ? true : false,
            //            IsSendEmail = getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsSendEmail == true).Count() > 0 ? true : false,
            //            CanPrint = getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID && (y.Status == "Subtituted" || y.Status == "Venue Change" || y.Status == "Subtituted & Venue Change")).FirstOrDefault() != null ? true : false,
            //            IsSetScheduleRealization = getDataSubtituteTeacher.Where(y => y.DaysOfWeek == x.DaysOfWeek && y.SessionID == x.SessionID && y.ClassID == x.ClassID && (y.Status == "Subtituted" || y.Status == "Venue Change" || y.Status == "Subtituted & Venue Change")).FirstOrDefault() != null ? true : false,
            //            //IdHomeroom = x.IdHomeroom,
            //            IdLesson = x.IdLesson,
            //            IdAcademicYear = x.IdAcademicYear,
            //            IdLevel = x.IdLevel,
            //            IdGrade = x.IdGrade,
            //            IdDay = x.IdDay,
            //            IdScheduleRealization = getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek).Select(x => x.Id).FirstOrDefault()
            //        }
            //    ).ToList();

            //foreach (var item in dataItems)
            //{
            //    var haveHistory = _dbContext.Entity<HTrScheduleRealization2>().Any(x => x.IdScheduleRealization2 == item.IdScheduleRealization);
            //    item.CanModified = haveHistory;
            //}

            var countAll = await query
                .Select(x => new GetListScheduleRealizationByTeacherV2Result
                {
                    SessionID = x.SessionID,
                    SessionStartTime = x.StartTime,
                    SessionEndTime = x.EndTime,
                    ClassID = x.ClassID,
                    IdVenue = x.IdVenue,
                    DaysOfWeek = x.DaysOfWeek
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
