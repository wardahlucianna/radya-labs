using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetListSubstitutionReporV2tHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetListSubstitutionReporV2tHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListSubstitutionReportV2Request>(nameof(GetListSubstitutionReportV2Request.IdAcademicYear));

            var predicate = PredicateBuilder.Create<TrScheduleRealization2>(x => x.IdAcademicYear == param.IdAcademicYear);
            var predicateLesson = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);
            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization2>(x => x.IdAcademicYear == param.IdAcademicYear);
            
            if(param.StartDate != null)
            {
                predicate = predicate.And(x => x.ScheduleDate >= param.StartDate);
                predicateSubtitute = predicateSubtitute.And(x => x.ScheduleDate >= param.StartDate);
            }

            if(param.EndDate != null)
            {
                predicate = predicate.And(x => x.ScheduleDate <= param.EndDate);
                predicateSubtitute = predicateSubtitute.And(x => x.ScheduleDate <= param.StartDate);
            }
                
            if(param.IdUserTeacher != null)
                predicate = predicate.And(x => param.IdUserTeacher.Contains(x.IdBinusian));

            if(param.IdUserSubstituteTeacher != null)
                predicate = predicate.And(x => param.IdUserSubstituteTeacher.Contains(x.IdBinusianSubtitute));
            
            if(!string.IsNullOrWhiteSpace(param.IdLevel))
            {
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);
                predicateSubtitute = predicateSubtitute.And(x => x.IdLevel == param.IdLevel);
            }

            if(param.IdGrade != null)
                {
                    predicate = predicate.And(x => param.IdGrade.Contains(x.IdGrade));
                    predicateLesson = predicateLesson.And(x => param.IdGrade.Contains(x.Lesson.Subject.IdGrade));
                    predicateSubtitute = predicateSubtitute.And(x => param.IdGrade.Contains(x.IdGrade));
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

            var query = _dbContext.Entity<TrScheduleRealization2>()
                                 .Include(x => x.Staff)
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataTeacher = _dbContext.Entity<TrScheduleRealization2>()
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization2>()
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

            IReadOnlyList<GetListSubstitutionReportV2Result> items;
            IReadOnlyList<GetListSubstitutionReportV2Result> dataItems;

            items = query
                .Select(x => new GetListSubstitutionReportV2Result
                    {
                        
                        Date = x.ScheduleDate,
                        ClassID = x.ClassID,
                        SessionID = x.SessionID,
                        SessionStartTime = x.StartTime,
                        SessionEndTime = x.EndTime,
                        IdVenue = x.IdVenueChange != null ? x.IdVenueChange : x.IdVenue,
                        VenueName = x.IdVenueChange != null ? x.VenueNameChange : x.VenueName,
                        IdVenueOld = x.IdVenue,
                        VenueNameOld = x.VenueName,
                        IsCancelClass = x.IsCancel,
                        IsSendEmail = x.IsSendEmail,
                        NotesForSubtitutions = x.NotesForSubtitutions,
                        Status = x.Status
                    }
                )
                .Distinct()
                .OrderBy(x => x.SessionID)
                .SetPagination(param)
                .ToList();

            dataItems = items
                    .Select(x => new GetListSubstitutionReportV2Result
                    {
                        // Ids = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Id).ToList(),
                        Date = x.Date,
                        ClassID = x.ClassID,
                        SessionID = x.SessionID,
                        SessionStartTime = x.SessionStartTime,
                        SessionEndTime = x.SessionEndTime,
                        DataTeachers =  getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new DataListTeacherV2
                                        {
                                            Id = y.IdBinusian,
                                            Description = y.TeacherName
                                        }).Distinct().OrderBy(x => x.Description).ToList(),
                        DataSubtituteTeachers = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new DataListSubtituteTeacherV2
                                        {
                                            Id = y.IdBinusianSubtitute,
                                            Code = y.TeacherName,
                                            Description = y.TeacherNameSubtitute
                                        }).Distinct().OrderBy(x => x.Code).ToList(),
                        IdVenueOld = x.IdVenueOld,
                        VenueNameOld = x.VenueNameOld,
                        ChangeVenue = new ItemValueVm(x.IdVenue,x.VenueName),
                        IdVenue = x.IdVenueOld,
                        VenueName = x.VenueNameOld,
                        EntryStatusBy = "System",
                        EntryStatusDate = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.DateIn).First(),
                        Status = x.IsCancelClass == false ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Status).FirstOrDefault() : null,
                        IsCancelClass = x.IsCancelClass,
                        IsSendEmail = x.IsSendEmail,
                        NotesForSubtitutions = x.NotesForSubtitutions,
                        IdHomeroom = x.IdHomeroom
                    }    
                ).ToList();

            var countAll = await query
                .Select(x => new GetListSubstitutionReportV2Result
                    {
                        
                        Date = x.ScheduleDate,
                        ClassID = x.ClassID,
                        SessionID = x.SessionID,
                        SessionStartTime = x.StartTime,
                        SessionEndTime = x.EndTime,
                        IdVenue = x.IdVenue,
                        VenueName = x.VenueName,
                        IsCancelClass = x.IsCancel,
                        IsSendEmail = x.IsSendEmail,
                        NotesForSubtitutions = x.NotesForSubtitutions,
                        Status = x.Status
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
