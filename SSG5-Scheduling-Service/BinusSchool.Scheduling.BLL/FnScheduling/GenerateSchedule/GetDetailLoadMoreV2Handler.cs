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
    public class GetDetailLoadMoreV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDetailLoadMoreV2Handler(ISchedulingDbContext dbContext)
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

            #region Homeroom Student Enrollment
            var queryHomeroomStudentEnrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                         .Include(p => p.HomeroomStudent).ThenInclude(p => p.Homeroom).ThenInclude(p => p.GradePathwayClassroom).ThenInclude(p => p.Classroom)
                                         .Include(p => p.HomeroomStudent).ThenInclude(p => p.Homeroom).ThenInclude(p => p.Grade)
                                         .Where(e => e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYears
                                                    && e.HomeroomStudent.Homeroom.IdGrade == param.IdGrade
                                                )
                                         .Select(p => new
                                         {
                                             p.IdLesson,
                                             p.IdSubject,
                                             p.HomeroomStudent.IdHomeroom,
                                             GradeCode = p.HomeroomStudent.Homeroom.Grade.Code,
                                             ClassroomCode = p.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                                         });

            if (!string.IsNullOrWhiteSpace(param.IdSubject))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(p => p.IdSubject == param.IdSubject);
            if (!string.IsNullOrWhiteSpace(param.IdClass))
            {
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(p => p.IdHomeroom == param.IdClass);
            }

            var getHomeroomStudentEnrollment = await queryHomeroomStudentEnrollment.ToListAsync(CancellationToken);
            #endregion
            var getIdLesson = getHomeroomStudentEnrollment.Select(e => e.IdLesson).ToList();

            #region teacher
            var querySchedule = _dbContext.Entity<MsSchedule>()
                                .Include(p => p.Lesson)
                                .Include(p => p.User)
                                .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYears
                                        && e.Lesson.IdGrade == param.IdGrade
                                    )
                                .Select(p => new
                                {
                                    p.IdLesson,
                                    p.Lesson.IdSubject,
                                    p.IdUser,
                                    p.IdSession,
                                    p.IdWeek,
                                    p.IdDay,
                                    NameUser = NameUtil.GenerateFullName(p.User.FirstName, p.User.LastName)
                                });

            if (!string.IsNullOrWhiteSpace(param.IdSubject))
                querySchedule = querySchedule.Where(p => p.IdSubject == param.IdSubject);
            if (!string.IsNullOrWhiteSpace(param.IdClass))
            {
                querySchedule = querySchedule.Where(p => getIdLesson.Contains(p.IdLesson));
            }

            var getSchedule = await querySchedule.ToListAsync(CancellationToken);
            #endregion

            var predicate = PredicateBuilder.Create<MsScheduleLesson>(x =>
                                            x.IsGenerated
                                            && x.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.IdAcademicYear == param.IdAcademicYears
                                            && x.GeneratedScheduleGrade.IdGrade == param.IdGrade
                                            && x.ScheduleDate.Date == param.ScheduleDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(param.IdAscTimetable))
                predicate = predicate.And(p => p.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable);
            if (!string.IsNullOrWhiteSpace(param.IdSubject))
                predicate = predicate.And(p => p.IdSubject == param.IdSubject);
            if (!string.IsNullOrWhiteSpace(param.IdClass))
            {
                predicate = predicate.And(p => getIdLesson.Contains(p.IdLesson));
            }

            var listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                    .Include(e => e.Week)
                                    .Where(predicate)
                                    .Select(x => new
                                    {
                                        x.ClassID,
                                        x.ScheduleDate,
                                        x.IdVenue,
                                        x.VenueName,
                                        x.IdSession,
                                        x.SessionID,
                                        x.IdSubject,
                                        x.SubjectName,
                                        x.DaysOfWeek,
                                        x.StartTime,
                                        x.EndTime,
                                        x.IdLesson,
                                        Week = x.Week.Description,
                                        x.IdWeek,
                                        x.IdDay
                                    })
                                    .ToListAsync(CancellationToken);

            var getData = listScheduleLesson
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
                                Teacher = getSchedule
                                           .Where(e => e.IdLesson == p.IdLesson && e.IdSession == p.IdSession && e.IdWeek == p.IdWeek && e.IdDay == p.IdDay)
                                           .Select(e => new CodeWithIdVm
                                           {
                                               Id = e.IdUser,
                                               Description = e.NameUser
                                           }).FirstOrDefault(),
                                Session = new SessionVm
                                {
                                    Id = p.IdSession,
                                    Dayofweek = p.DaysOfWeek,
                                    StartTime = p.StartTime,
                                    EndTime = p.EndTime,
                                    SessionID = p.SessionID,
                                },
                                Week = p.Week,
                                Subject = new CodeWithIdVm
                                {
                                    Id = p.IdSubject,
                                    Description = p.SubjectName,
                                    Code = p.SubjectName
                                },
                                Homeroom = getHomeroomStudentEnrollment
                                        .Where(e => e.IdLesson == p.IdLesson)
                                        .Select(e => new CodeWithIdVm
                                        {
                                            Id = e.IdHomeroom,
                                            Code = e.GradeCode + e.ClassroomCode,
                                            Description = e.GradeCode + e.ClassroomCode,
                                        })
                                        .FirstOrDefault()
                            }).ToList();

            return Request.CreateApiResult2(getData as object);
        }
    }
}
