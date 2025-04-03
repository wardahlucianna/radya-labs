using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetSessionVenueForScheduleRealizationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetSessionVenueForScheduleRealizationHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSessionVenueForScheduleRealizationRequest>(nameof(GetSessionByTeacherDateReq.IdAcademicYear), nameof(GetSessionVenueForScheduleRealizationRequest.StartDate), nameof(GetSessionVenueForScheduleRealizationRequest.EndDate));

            var predicate = PredicateBuilder.Create<MsScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear);

            var predicateLessonTeacher = PredicateBuilder.Create<MsLessonTeacher>(e => true);

            if(param.IdUser != null)
                predicateLessonTeacher = predicateLessonTeacher.And(e => param.IdUser.Contains(e.IdUser));

            var listIdLesson = await _dbContext.Entity<MsLessonTeacher>()
                          .Include(e => e.Lesson)
                          .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                  && param.IdGrade.Contains(e.Lesson.IdGrade)
                                  && e.IsAttendance)
                          .Where(predicateLessonTeacher)
                          .Select(e => e.IdLesson)
                          .ToListAsync(CancellationToken);

            if(param.IdUser != null)
                predicate = predicate.And(x => listIdLesson.Contains(x.IdLesson));
            
            if(!string.IsNullOrWhiteSpace(param.IdLevel))
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);

            if(param.IdGrade != null)
                predicate = predicate.And(x => param.IdGrade.Contains(x.IdGrade));
            
            if(param.IsSession)
            {
                var query = _dbContext.Entity<MsScheduleLesson>()
                                 .Where(predicate)
                                 .Select(x => new { x.Session.SessionID,  x.Session.StartTime, x.EndTime });

                IReadOnlyList<GetSessionByTeacherDateResult> items;
                items = await query
                    .Select(x => new GetSessionByTeacherDateResult
                        {
                            SessionID = x.SessionID,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime
                        }
                    )
                    .Distinct()
                    .OrderBy(x => x.SessionID)
                    .ToListAsync(CancellationToken);

                items = items
                        .Select(x => new GetSessionByTeacherDateResult
                        {
                            SessionID = x.SessionID,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime
                        }    
                    ).ToList();

                List<GetSessionByTeacherDateResult> itemSession = new List<GetSessionByTeacherDateResult>();

                //var noSession = 1;
                foreach(var dataItems in items)
                {
                    var dataSession = new GetSessionByTeacherDateResult
                    {
                        Id = dataItems.SessionID.ToString(),
                        Code = dataItems.SessionID.ToString(),
                        Description = $"Session {dataItems.SessionID}",
                        SessionID = dataItems.SessionID,
                        StartTime = dataItems.StartTime,
                        EndTime = dataItems.EndTime
                    };

                    itemSession.Add(dataSession);

                    //noSession++;
                }
                
            
                return Request.CreateApiResult2(itemSession as object);
            }
            else
            {
                var query = _dbContext.Entity<MsScheduleLesson>()
                                                .Where(predicate)
                                                .Select(x => new { x.Venue.Id,  x.Venue.Code, x.Venue.Description });

                IReadOnlyList<GetVenueByTeacherDateResult> items;
                items = await query
                    .Select(x => new GetVenueByTeacherDateResult
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description
                        }
                    )
                    .Distinct()
                    .OrderBy(x => x.Code)
                    .ToListAsync(CancellationToken);

                items = items
                        .Select(x => new GetVenueByTeacherDateResult
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description
                        }    
                    )
                    .OrderBy(x => x.Code)
                    .ToList();
            
                return Request.CreateApiResult2(items as object);
            }
        }
    }
}
