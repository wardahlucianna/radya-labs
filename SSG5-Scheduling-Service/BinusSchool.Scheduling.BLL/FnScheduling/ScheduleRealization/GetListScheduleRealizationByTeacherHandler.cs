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
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetListScheduleRealizationByTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetListScheduleRealizationByTeacherHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListScheduleRealizationByTeacherRequest>(nameof(GetListScheduleRealizationByTeacherRequest.IdAcademicYear),
                                                                                           nameof(GetListScheduleRealizationByTeacherRequest.StartDate),
                                                                                           nameof(GetListScheduleRealizationByTeacherRequest.EndDate),
                                                                                           nameof(GetListScheduleRealizationByTeacherRequest.IdUserTeacher),
                                                                                           nameof(GetListScheduleRealizationByTeacherRequest.ClassID));

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.Homeroom.IdAcademicYear == param.IdAcademicYear && (x.IdUser == param.IdUserTeacher || x.IdBinusianOld == param.IdUserTeacher) && param.ClassID.Contains(x.ClassID));
            var predicateLesson = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);
            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization>(x => x.ScheduleDate >= param.StartDate 
                                                                                        && x.ScheduleDate <= param.EndDate 
                                                                                        && x.Homeroom.IdAcademicYear == param.IdAcademicYear
                                                                                        && (x.IdBinusian == param.IdUserTeacher || x.IdBinusianSubtitute == param.IdUserTeacher)
                                                                                        && param.ClassID.Contains(x.ClassID));
            
            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                 .Include(x => x.User)
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataTeacher = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Where(predicateSubtitute);

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

            IReadOnlyList<GetListScheduleRealizationByTeacherResult> items;
            IReadOnlyList<GetListScheduleRealizationByTeacherResult> dataItems;

            items = await query
                .Select(x => new GetListScheduleRealizationByTeacherResult
                    {
                        ClassID = x.ClassID,
                        DaysOfWeek = x.DaysOfWeek,
                        SessionID = x.SessionID,
                        SessionStartTime = x.StartTime,
                        SessionEndTime = x.EndTime,
                        IdVenue = x.IdVenue,
                        VenueName = x.VenueName,
                        // IdVenueOld = x.IdVenueOld,
                        // VenueNameOld = x.VenueNameOld,
                        IsSetScheduleRealization = x.IsSetScheduleRealization,
                        IsCancelClass = x.IsCancelScheduleRealization,
                        // IdHomeroom = x.IdHomeroom
                    }
                )
                .Distinct()
                .OrderBy(x => x.ClassID).ThenBy(x => x.SessionStartTime)
                .SetPagination(param)
                .ToListAsync(CancellationToken);

            dataItems = items
                    .Select(x => new GetListScheduleRealizationByTeacherResult
                    {
                        Ids = getDataTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek).Select(y => y.Id).ToList(),
                        ClassID = x.ClassID,
                        DaysOfWeek = x.DaysOfWeek,
                        SessionID = x.SessionID,
                        SessionStartTime = x.SessionStartTime,
                        SessionEndTime = x.SessionEndTime,
                        IdVenue = getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault() == null ? x.IdVenue : getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault().IdVenueChange,
                        VenueName = getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault() == null ? x.VenueName : getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault().VenueNameChange,
                        ChangeVenue = new ItemValueVm(x.IdVenue,x.VenueName),
                        DataSubtituteTeachers = x.IsSetScheduleRealization == true && getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher && x.IsCancelClass == false).Count() > 0 ? getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher && y.IsCancel == false).Select(y => new SubtituteTeacher
                                        {
                                            Id = y.IdBinusianSubtitute,
                                            Description = y.TeacherNameSubtitute
                                        }).FirstOrDefault() :
                                        getDataTeacher.Where(y => y.IdUser == param.IdUserTeacher).Select(y => new SubtituteTeacher
                                        {
                                            Id = y.IdUser,
                                            Description = y.TeacherName
                                        }).FirstOrDefault(),
                        EntryStatusBy = "System",
                        EntryStatusDate = getDataTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek).Select(y => y.DateIn).FirstOrDefault() != null ? getDataTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek).Select(y => y.DateIn).FirstOrDefault() : null,
                        Status = x.IsSetScheduleRealization == true && getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher).FirstOrDefault() != null ? getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher).Select(y => y.Status).FirstOrDefault() : null,
                        CanEnableDisable = param.EndDate.Date < _dateTime.ServerTime.Date ? false : true,
                        IsCancelClass = getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault() == null ? false : true,
                        IsSendEmail = getDataSubtituteTeacher.Where(y => y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.DaysOfWeek == x.DaysOfWeek && y.IdBinusian == param.IdUserTeacher && y.IsSendEmail == true).Count() > 0 ? true : false,
                        CanPrint = x.IsSetScheduleRealization,
                        // IdHomeroom = x.IdHomeroom
                    }    
                ).ToList();

            var countAll = await query
                .Select(x => new GetListScheduleRealizationByTeacherResult
                    {
                        SessionID = x.SessionID,
                        SessionStartTime = x.StartTime,
                        SessionEndTime = x.EndTime,
                        ClassID = x.ClassID,
                        IdVenue = x.IdVenue,
                        DaysOfWeek = x.DaysOfWeek,
                        // IdHomeroom = x.IdHomeroom
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
