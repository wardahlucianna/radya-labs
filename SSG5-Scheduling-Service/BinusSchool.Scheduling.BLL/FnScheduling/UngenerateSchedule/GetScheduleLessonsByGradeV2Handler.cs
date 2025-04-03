using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetScheduleLessonsByGradeV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetScheduleLessonsByGradeV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetScheduleLessonsByGradeRequest, GetScheduleLessonsByGradeValidator>();

            var listHomeroomLesson = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                        .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                                        .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                        .Include(e=>e.Lesson)
                                        .Where(e=>e.HomeroomStudent.Homeroom.IdGrade== body.IdGrade)
                                        .GroupBy(e => new
                                        {
                                            e.IdLesson,
                                            gradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                                            classroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                            classID = e.Lesson.ClassIdGenerated
                                        })
                                        .Select(e => new
                                        {
                                            e.Key.IdLesson,
                                            homeroom = e.Key.gradeCode + e.Key.classroomCode,
                                            classID = e.Key.classID
                                        })
                                        .ToListAsync(CancellationToken);

            var listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                            .Include(e=>e.Session).ThenInclude(e=>e.Day)
                            .Where(x => x.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable
                                        && x.IdGrade == body.IdGrade
                                        && (x.ScheduleDate.Date >= body.StartDate.Date && x.ScheduleDate.Date <= body.EndDate.Date)
                                        && body.IdDays.Contains(x.IdDay))
                            .GroupBy(e => new
                            {
                                ClassId = e.ClassID,
                                IdLesson = e.IdLesson,
                                IdSession = e.Session.Id,
                                SessionID = e.Session.SessionID,
                                Day = e.Session.Day.Description,
                                IdDay = e.IdDay
                            })
                            .Select(e => new
                            {
                                ClassId = e.Key.ClassId,
                                IdLesson = e.Key.IdLesson,
                                IdSession = e.Key.IdSession,
                                SessionID = e.Key.SessionID,
                                Day = e.Key.Day,
                                IdDay = e.Key.IdDay
                            })
                            .ToListAsync();

            var result = listScheduleLesson
                            .GroupBy(x => new { x.ClassId, x.IdLesson })
                            .Select(g => new GetScheduleLessonsByGradeV2Result
                            {
                                IdLesson = g.Key.IdLesson,
                                ClassId = g.Key.ClassId,
                                Homeroom = string.Join(", ", listHomeroomLesson.Where(e => e.classID == g.Key.ClassId).Select(e => e.homeroom).Distinct().ToList()),
                                Sessions = listScheduleLesson
                                            .Where(e=>e.ClassId==g.Key.ClassId && e.IdLesson == g.Key.IdLesson)
                                            .OrderBy(e=>e.SessionID).ThenBy(e=>e.IdDay)
                                            .Select(e => new GetScheduleLessonsBySession
                                            {
                                                Id = e.IdSession,
                                                Description = e.SessionID.ToString(),
                                                Day = e.Day
                                            }).ToList(),
                            })
                            .OrderBy(e => e.ClassId)
                            .ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
