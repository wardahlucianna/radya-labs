using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetDetailLoadMoreHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDetailLoadMoreHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new List<GetDetailGenerateScheduleResult>();
            var param = Request.ValidateParams<GetDetailGenerateScheduleRequest>(
               nameof(GetDetailGenerateScheduleRequest.IdAcademicYears),
               nameof(GetDetailGenerateScheduleRequest.IdGrade),
               nameof(GetDetailGenerateScheduleRequest.ScheduleDate)
               );

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x =>
                                            x.IsGenerated
                                            && x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.IdAcademicYear == param.IdAcademicYears
                                            && x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == param.IdGrade
                                            && x.ScheduleDate.Date == param.ScheduleDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(param.IdAscTimetable))
                predicate = predicate.And(p => p.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable);
            if (!string.IsNullOrWhiteSpace(param.IdClass))
                predicate = predicate.And(p => p.IdHomeroom == param.IdClass);
            if (!string.IsNullOrWhiteSpace(param.IdSubject))
                predicate = predicate.And(p => p.IdSubject == param.IdSubject);

            var getData = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                         .Include(p => p.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule)
                                         .Include(p => p.Week)
                                         .Where(predicate)
                                         .Select(p => new GetDetailGenerateScheduleResult
                                         {
                                             ClassId = p.ClassID,
                                             StartDate = p.ScheduleDate,
                                             EndDate = p.ScheduleDate,
                                             Summary = p.ClassID,
                                             Venue = new CodeWithIdVm
                                             {
                                                 Id = p.IdVenue,
                                                 Description = p.VenueName,
                                             },
                                             Teacher = new CodeWithIdVm
                                             {
                                                 Id = p.IdUser,
                                                 Description = p.TeacherName
                                             },
                                             Session = new SessionVm
                                             {
                                                 Id = p.IdSession,
                                                 Dayofweek = p.DaysOfWeek,
                                                 StartTime = p.StartTime,
                                                 EndTime = p.EndTime,
                                                 SessionID = p.SessionID,
                                             },
                                             Week = p.Week.Description,
                                             Subject = new CodeWithIdVm
                                             {
                                                 Id = p.IdSubject,
                                                 Description = p.SubjectName,
                                                 Code = p.SubjectName
                                             },
                                             Homeroom = new CodeWithIdVm
                                             {
                                                 Id = p.IdHomeroom,
                                                 Description = p.HomeroomName,
                                                 Code = p.HomeroomName
                                             }
                                         }).ToListAsync();


            return Request.CreateApiResult2(getData as object);
        }
    }
}
