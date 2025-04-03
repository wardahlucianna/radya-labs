using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleWithClassIDHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetGenerateScheduleWithClassIDHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGenerateScheduleWithClassIDRequest>(
              nameof(GetGenerateScheduleWithClassIDRequest.IdAscTimetable),
              nameof(GetGenerateScheduleWithClassIDRequest.IdGrade),
              nameof(GetGenerateScheduleWithClassIDRequest.ClassID));

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleStudent>(p => p.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable
                                                                                && p.GeneratedScheduleGrade.IdGrade == param.IdGrade
                                                                                && p.GeneratedScheduleLessons.Any(x => x.ClassID == param.ClassID));
            var result = new GetClassIDByGradeAndStudentResult();
            var getData = await _dbContext.Entity<TrGeneratedScheduleStudent>()
                                 .Include(p => p.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable)
                                 .Include(p => p.GeneratedScheduleLessons).ThenInclude(p => p.Week)
                                 .Where(predicate).ToListAsync();

            result.Asctimetable = new CodeWithIdVm
            {
                Id = getData.FirstOrDefault().GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Id,
                Code = getData.FirstOrDefault().GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Name,
                Description = ""
            };
            result.StartPeriod = getData.FirstOrDefault().GeneratedScheduleGrade.StartPeriod;
            result.EndPeriod = getData.FirstOrDefault().GeneratedScheduleGrade.EndPeriod;
            result.ClassId = param.ClassID;

            List<StudentVm> _students = new List<StudentVm>();
            foreach (var item in getData)
            {
                var _student = new StudentVm();
                _student.StudentName = item.IdStudent;
                _student.StudentId = item.IdStudent;
                _student.Grade = new GradeWithPeriodVm
                {
                    Id = item.GeneratedScheduleGrade.IdGrade,
                    Code = "",
                    Description = "",
                };
                var getClassId = item.GeneratedScheduleLessons.GroupBy(p => p.ClassID).ToList();
                var _classIds = new List<LessonClassIDVM>();
                foreach (var itemClassId in getClassId)
                {
                    var _classId = new LessonClassIDVM();
                    _classId.ClassIdFormat = itemClassId.Key;
                    var weeks = new List<WeekVm>();
                    foreach (var itemWeek in itemClassId)
                    {
                        var _week = new WeekVm();
                        _week.IdWeek = itemWeek.IdWeek;
                        _week.Teacher = new CodeWithIdVm
                        {
                            Id = itemWeek.IdUser,
                            Description = itemWeek.TeacherName,
                        };
                        _week.Venue = new CodeWithIdVm
                        {
                            Id = itemWeek.IdVenue,
                            Description = itemWeek.VenueName,
                        };
                        _week.Week = new CodeWithIdVm
                        {
                            Id = itemWeek.IdWeek,
                            Code = itemWeek.Week.Code,
                            Description = itemWeek.Week.Description,
                        };
                        _week.Session = new SessionVm
                        {
                            Id = itemWeek.IdSession,
                            SessionID = itemWeek.SessionID,
                            StartTime = itemWeek.StartTime,
                            EndTime = itemWeek.EndTime,
                        };
                        weeks.Add(_week);
                    }
                    _classId.Weeks = weeks;
                    _classIds.Add(_classId);
                }
                _student.ClassIds = _classIds;

                _students.Add(_student);
            }
            result.Students = _students;
            return Request.CreateApiResult2(result as object);
        }
    }
}
