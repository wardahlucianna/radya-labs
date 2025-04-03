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
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetListSubstitutionReportHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetListSubstitutionReportHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListSubstitutionReportRequest>(nameof(GetListSubstitutionReportRequest.IdAcademicYear));

            var predicate = PredicateBuilder.Create<TrScheduleRealization>(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            var predicateLesson = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);
            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization>(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            
            if(param.StartDate != null)
            {
                predicate = predicate.And(x => x.ScheduleDate >= param.StartDate);
                predicateSubtitute = predicateSubtitute.And(x => x.ScheduleDate >= param.StartDate);
            }

            if(param.EndDate != null)
            {
                predicate = predicate.And(x => x.ScheduleDate <= param.EndDate);
                predicateSubtitute = predicateSubtitute.And(x => x.ScheduleDate <= param.EndDate);
            }
                
            if(param.IdUserTeacher != null)
                predicate = predicate.And(x => param.IdUserTeacher.Contains(x.IdBinusian));

            if(param.IdUserSubstituteTeacher != null)
                predicate = predicate.And(x => param.IdUserSubstituteTeacher.Contains(x.IdBinusianSubtitute));
            
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

            var query = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Staff)
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataTeacher = await _dbContext.Entity<TrScheduleRealization>()
                                 .Where(predicate)
                                 .Select(x=> new
                                 {
                                     x.IdBinusian,
                                     x.TeacherName,
                                     x.IdBinusianSubtitute,
                                     x.TeacherNameSubtitute,
                                     x.ScheduleDate,
                                     x.SessionID,
                                     x.ClassID,
                                     x.DateIn
                                 })
                                 .Distinct()
                                 .ToListAsync(CancellationToken);

            var getDataSubtituteTeacher = await _dbContext.Entity<TrScheduleRealization>()
                                 .Where(predicateSubtitute)
                                  .Select(x => new
                                  {
                                      x.IdBinusian,
                                      x.TeacherName,
                                      x.IdBinusianSubtitute,
                                      x.TeacherNameSubtitute,
                                      x.ScheduleDate,
                                      x.SessionID,
                                      x.ClassID,
                                      x.Status
                                  })
                                 .Distinct()
                                 .ToListAsync(CancellationToken);

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

            IReadOnlyList<GetListSubstitutionReportResult> items;
            IReadOnlyList<GetListSubstitutionReportResult> dataItems;

            var query2 = await _dbContext.Entity<TrScheduleRealization>().Where(x => x.IsCancel == true || x.IdVenue != x.IdVenueChange || x.IdBinusian != x.IdBinusianSubtitute).OrderBy(x => x.ScheduleDate).ThenBy(x => x.StartTime).Distinct().ToListAsync();

            items = await query
                .Where(x => x.IsCancel == true || x.IdVenue != x.IdVenueChange || x.IdBinusian != x.IdBinusianSubtitute)
                .Select(x => new GetListSubstitutionReportResult
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
                        Status = x.Status,
                        // IdHomeroom = x.IdHomeroom
                    }
                )
                .Distinct()
                .OrderBy(x => x.Date).ThenBy(x => x.SessionStartTime).ThenBy(x => x.ClassID)
                .SetPagination(param)
                .ToListAsync(CancellationToken);

            dataItems = items
                    .Select(x => new GetListSubstitutionReportResult
                    {
                        // Ids = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Id).ToList(),
                        Date = x.Date,
                        ClassID = x.ClassID,
                        SessionID = x.SessionID,
                        SessionStartTime = x.SessionStartTime,
                        SessionEndTime = x.SessionEndTime,
                        DataTeachers =  getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new DataListTeacher
                                        {
                                            Id = y.IdBinusian,
                                            Description = y.TeacherName
                                        }).Distinct().ToList(),
                        DataSubtituteTeachers = x.IsCancelClass == false ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new DataListSubtituteTeacher
                                        {
                                            Id = y.IdBinusianSubtitute,
                                            Description = y.TeacherNameSubtitute
                                        }).Distinct().ToList() :
                                        getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new DataListSubtituteTeacher
                                        {
                                            Id = y.IdBinusian,
                                            Description = y.TeacherName
                                        }).Distinct().ToList(),
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
                .Select(x => new GetListSubstitutionReportResult
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
