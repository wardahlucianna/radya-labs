using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
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
    public class GetDetailGenerateScheduleV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDetailGenerateScheduleV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new List<GetDetailGenerateScheduleResult>();
            var param = Request.ValidateParams<GetDetailGenerateScheduleRequest>(
               nameof(GetDetailGenerateScheduleRequest.IdAcademicYears),
               nameof(GetDetailGenerateScheduleRequest.IdGrade),
               nameof(GetDetailGenerateScheduleRequest.DateMountYers)
               );

            var queryHomeroomStudentEnrollmen = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(e=>e.HomeroomStudent.Homeroom.Grade)
                                                    .Include(e=>e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom)
                                                    .Where(e => e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYears
                                                            && e.HomeroomStudent.Homeroom.IdGrade == param.IdGrade);

            if (!string.IsNullOrWhiteSpace(param.IdClass))
                queryHomeroomStudentEnrollmen = queryHomeroomStudentEnrollmen.Where(p => p.HomeroomStudent.IdHomeroom == param.IdClass);
            if (!string.IsNullOrWhiteSpace(param.IdSubject))
                queryHomeroomStudentEnrollmen = queryHomeroomStudentEnrollmen.Where(p => p.IdSubject == param.IdSubject);

            var listHomeroomStudentEnrollmen = await queryHomeroomStudentEnrollmen
                                                .GroupBy(e => new
                                                {
                                                    e.HomeroomStudent.IdHomeroom,
                                                    gradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                                                    classroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                                    e.IdLesson,
                                                    e.IdSubject
                                                })
                                                .Select(e=>e.Key)
                                                .ToListAsync(CancellationToken);

            var listIdSubject = listHomeroomStudentEnrollmen.Select(e => e.IdSubject).ToList();
            var listIdLesson = listHomeroomStudentEnrollmen.Select(e => e.IdLesson).ToList();

            var listSchedule = await _dbContext.Entity<MsSchedule>()
                                    .Include(e=>e.User)
                                    .Include(e=>e.Lesson)
                                    .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYears
                                            && e.Lesson.IdGrade == param.IdGrade
                                            && listIdLesson.Contains(e.IdLesson)
                                            && listIdSubject.Contains(e.Lesson.IdSubject)
                                            ).ToListAsync(CancellationToken);

            var predicate = PredicateBuilder.Create<MsScheduleLesson>(x =>
                                            x.IdAcademicYear == param.IdAcademicYears
                                            && x.IdGrade == param.IdGrade
                                            && x.ScheduleDate.Month == param.DateMountYers.Value.Month
                                            && x.ScheduleDate.Year == param.DateMountYers.Value.Year);

            if (!string.IsNullOrWhiteSpace(param.IdAscTimetable))
                predicate = predicate.And(p => p.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable);
            if (!string.IsNullOrWhiteSpace(param.IdSubject))
                predicate = predicate.And(p => listIdSubject.Contains(param.IdSubject));

            var getData = _dbContext.Entity<MsScheduleLesson>()
                                         .Include(p => p.GeneratedScheduleGrade.GeneratedSchedule)
                                         .Include(p => p.Week)
                                         .Where(predicate);

            var coba = await getData.ToListAsync(CancellationToken);
            if (getData.Count() == 0)
            {
                return Request.CreateApiResult2(result as object);
            }

            var group = getData.AsEnumerable().GroupBy(p => new
            {
                p.ScheduleDate,
                p.StartTime,
                p.EndTime
            });

            foreach (var item in group)
            {
                var getTriItem = item.Take(3);
                foreach (var dataItem in getTriItem)
                {
                    var _data = new GetDetailGenerateScheduleResult();
                    _data.ClassId = dataItem.ClassID;
                    _data.EndDate = dataItem.ScheduleDate;
                    _data.StartDate = dataItem.ScheduleDate;
                    _data.Week = dataItem.Week.Description;
                    _data.Venue = new CodeWithIdVm
                    {
                        Id = dataItem.IdVenue,
                        Description = dataItem.VenueName,

                    };
                    _data.Teacher = listSchedule
                                    .Where(e => e.IdLesson == dataItem.IdLesson
                                        && e.Lesson.IdSubject == dataItem.IdSubject
                                        && e.IdDay==dataItem.IdDay
                                        && e.IdWeek==dataItem.IdWeek)
                                    .Select(e => new CodeWithIdVm
                                    {
                                        Id = e.IdUser,
                                        Description = NameUtil.GenerateFullName(e.User.FirstName, e.User.LastName),
                                    }).FirstOrDefault();
                    _data.Summary = dataItem.ClassID;
                    _data.Session = new SessionVm
                    {
                        Id = dataItem.IdSession,
                        StartTime = dataItem.StartTime,
                        EndTime = dataItem.EndTime,
                        SessionID = dataItem.SessionID,
                    };
                    _data.Subject = new CodeWithIdVm
                    {
                        Id = dataItem.IdSubject,
                        Description = dataItem.SubjectName,
                        Code = dataItem.SubjectName
                    };
                    _data.Homeroom = listHomeroomStudentEnrollmen
                                        .Where(e => e.IdLesson == dataItem.IdLesson
                                            && e.IdSubject == dataItem.IdSubject)
                                        .Select(e => new CodeWithIdVm
                                        {
                                            Id = e.IdHomeroom,
                                            Description = e.gradeCode+e.classroomCode,
                                            Code = e.gradeCode + e.classroomCode
                                        }).FirstOrDefault();
                    result.Add(_data);
                }
            }
            return Request.CreateApiResult2(result as object);
        }
    }
}
