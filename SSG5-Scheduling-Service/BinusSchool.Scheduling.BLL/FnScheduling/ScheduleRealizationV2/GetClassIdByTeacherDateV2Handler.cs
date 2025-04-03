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
    public class GetClassIdBYTeacherDateV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetClassIdBYTeacherDateV2Handler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassIdByTeacherDateV2Request>(nameof(GetClassIdByTeacherDateV2Request.IdAcademicYear), nameof(GetClassIdByTeacherDateV2Request.StartDate), nameof(GetClassIdByTeacherDateV2Request.EndDate), nameof(GetClassIdByTeacherDateV2Request.IdUser));

            var predicate = PredicateBuilder.Create<MsScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.IdAcademicYear == param.IdAcademicYear);

            var predicateLessonTeacher = PredicateBuilder.Create<MsLessonTeacher>(e => true);

            if(param.IdUser != null)
                predicateLessonTeacher = predicateLessonTeacher.And(e => param.IdUser.Contains(e.IdUser));

            var listIdLesson = await _dbContext.Entity<MsLessonTeacher>()
                          .Include(e => e.Lesson)
                          .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                  && e.IsAttendance)
                          .Where(predicateLessonTeacher)
                          .Select(e => e.IdLesson)
                          .ToListAsync(CancellationToken);

            if(param.IdUser != null)
                predicate = predicate.And(x => listIdLesson.Contains(x.IdLesson));

            var query = await _dbContext.Entity<MsScheduleLesson>()
                                 .Where(predicate)
                                 .Select(x => new { x.ClassID}).Distinct().ToListAsync(CancellationToken);

            IReadOnlyList<GetClassIdByTeacherDateResult> items;

            items = query
                    .Select(x => new GetClassIdByTeacherDateResult
                    {
                        Id = x.ClassID,
                        Code = x.ClassID,
                        Description = x.ClassID
                    }    
                ).ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
