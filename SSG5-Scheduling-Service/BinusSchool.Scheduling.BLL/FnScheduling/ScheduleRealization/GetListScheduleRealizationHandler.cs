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
    public class GetListScheduleRealizationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetListScheduleRealizationHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListScheduleRealizationRequest>(nameof(GetListScheduleRealizationRequest.IdAcademicYear), nameof(GetListScheduleRealizationRequest.IdLevel), nameof(GetSessionByTeacherDateReq.StartDate), nameof(GetSessionByTeacherDateReq.EndDate));

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            var predicateLesson = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);
            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            
            if(param.IdUserTeacher != null)
                predicate = predicate.And(x => param.IdUserTeacher.Contains(x.IdUser) || param.IdUserTeacher.Contains(x.IdBinusianOld));
            
            if(!string.IsNullOrWhiteSpace(param.IdLevel))
            {
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
                predicateSubtitute = predicateSubtitute.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            }

            if(param.IdGrade != null)
                {
                    predicate = predicate.And(x => param.IdGrade.Contains(x.Homeroom.IdGrade));
                    predicateLesson = predicateLesson.And(x => param.IdGrade.Contains(x.Lesson.Subject.IdGrade));
                    predicateSubtitute = predicateSubtitute.And(x => param.IdGrade.Contains(x.Homeroom.IdGrade));
                }

            if(param.SessionID != null)
                predicate = predicate.And(x => param.SessionID.Contains(x.SessionID));

            if(param.IdVenue != null)
            {
                predicate = predicate.And(x => param.IdVenue.Contains(x.IdVenue));
                predicateSubtitute = predicateSubtitute.And(x => param.IdVenue.Contains(x.IdVenue));
            }

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.TeacherName.ToUpper(), $"%{param.Search.ToUpper()}%")
                    || EF.Functions.Like(x.VenueName.ToUpper(), $"%{param.Search.ToUpper()}%")
                    );

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

            IReadOnlyList<GetListScheduleRealizationResult> items;
            IReadOnlyList<GetListScheduleRealizationResult> dataItems;

            items = await query
                .Select(x => new GetListScheduleRealizationResult
                    {
                        
                        Date = x.ScheduleDate,
                        SessionID = x.SessionID,
                        SessionStartTime = x.StartTime,
                        SessionEndTime = x.EndTime,
                        ClassID = x.ClassID,
                        IdVenue = x.IdVenue,
                        VenueName = x.VenueName,
                        IdVenueOld = x.IdVenueOld,
                        VenueNameOld = x.VenueNameOld,
                        IsSetScheduleRealization = x.IsSetScheduleRealization,
                        IsCancelClass = x.IsCancelScheduleRealization,
                        // IdHomeroom = x.IdHomeroom
                    }
                )
                .Distinct()
                .OrderBy(x => x.Date).ThenBy(x => x.SessionStartTime).ThenBy(x => x.ClassID)
                .SetPagination(param)
                .ToListAsync(CancellationToken);

            dataItems = items
                    .Select(x => new GetListScheduleRealizationResult
                    {
                        Ids = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Id).ToList(),
                        Date = x.Date,
                        SessionID = x.SessionID,
                        SessionStartTime = x.SessionStartTime,
                        SessionEndTime = x.SessionEndTime,
                        ClassID = x.ClassID,
                        IdVenue = x.IdVenueOld != null ? x.IdVenueOld : x.IdVenue,
                        VenueName = x.VenueNameOld != null ? x.VenueNameOld : x.VenueName,
                        ChangeVenue = new ItemValueVm(x.IdVenue,x.VenueName),
                        DataTeachers =  getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new ListTeacher
                                        {
                                            Id = y.IsSetScheduleRealization == true ? y.IdBinusianOld : y.IdUser,
                                            Description = y.IsSetScheduleRealization == true ? y.TeacherNameOld : y.TeacherName
                                        }).Distinct().ToList(),
                        DataSubtituteTeachers = x.IsSetScheduleRealization == true ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new ListSubtituteTeacher
                                        {
                                            Id = y.IdBinusianSubtitute,
                                            Description = y.TeacherNameSubtitute
                                        }).Distinct().ToList() :
                                        getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new ListSubtituteTeacher
                                        {
                                            Id = y.IdUser,
                                            Description = y.TeacherName
                                        }).Distinct().ToList(),
                        EntryStatusBy = "System",
                        EntryStatusDate = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.DateIn).First(),
                        Status = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).FirstOrDefault() != null ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Status).FirstOrDefault() : null,
                        CanEnableDisable = x.Date.Date < _dateTime.ServerTime.Date ? false : true,
                        IsCancelClass = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault() == null ? false : true,
                        IsSendEmail = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsSendEmail == true).Count() == 0 ? false : true,
                        CanPrint = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID && (y.Status == "Subtituted" || y.Status == "Venue Change")).FirstOrDefault() == null ? false : true,
                        IdHomeroom = x.IdHomeroom
                    }    
                ).ToList();

            var countAll = await query
                .Select(x => new GetListScheduleRealizationResult
                    {
                        
                        Date = x.ScheduleDate,
                        SessionID = x.SessionID,
                        SessionStartTime = x.StartTime,
                        SessionEndTime = x.EndTime,
                        ClassID = x.ClassID,
                        IdVenue = x.IdVenue,
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
