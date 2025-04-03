using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class DeleteUngenerateScheduleStudentHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public DeleteUngenerateScheduleStudentHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request
                .ValidateBody<DeleteUngenerateScheduleStudentRequest, DeleteUngenerateScheduleStudentValidator>();
            foreach (var student in body.Students)
            {
                foreach (var classId in student.ClassIds)
                {
                    var dataLesson = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                        .Include(x => x.GeneratedScheduleStudent)
                        .ThenInclude(x => x.GeneratedScheduleGrade)
                        .ThenInclude(x => x.GeneratedSchedule)
                        .Where(x => x.ClassID == classId)
                        .Where(x => x.ScheduleDate >= body.Start)
                        .Where(x => x.ScheduleDate <= body.End)
                        .Where(x => x.GeneratedScheduleStudent.IdStudent == student.IdStudent)
                        .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == body.IdGrade)
                        .Where(x =>
                            x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable ==
                            body.IdAscTimetable)
                        .ToListAsync(CancellationToken);
                    if (!dataLesson.Any()) continue;

                    // set inactive generated schedule lesson
                    foreach (var dl in dataLesson)
                    {
                        dl.IsActive = false;
                        dl.IsGenerated = false;
                    }

                    _dbContext.Entity<TrGeneratedScheduleLesson>().UpdateRange(dataLesson);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                var dataLessonStudent = _dbContext.Entity<TrGeneratedScheduleLesson>()
                    .Include(x => x.GeneratedScheduleStudent)
                    .ThenInclude(x => x.GeneratedScheduleGrade)
                    .ThenInclude(x => x.GeneratedSchedule)
                    .Where(x => student.ClassIds.Contains(x.ClassID))
                    .Where(x => x.GeneratedScheduleStudent.IdStudent == student.IdStudent)
                    .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == body.IdGrade)
                    .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable ==
                                body.IdAscTimetable);
                var countDataLessonStudent = await dataLessonStudent.CountAsync();
                if (countDataLessonStudent == 0)
                {
                    var dataStudent = await dataLessonStudent.ToListAsync();
                    var students = new List<TrGeneratedScheduleStudent>();
                    foreach (var item in dataStudent)
                    {
                        item.GeneratedScheduleStudent.IsActive = false;
                        students.Add(item.GeneratedScheduleStudent);
                    }

                    _dbContext.Entity<TrGeneratedScheduleStudent>().UpdateRange(students);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                }

                var dataGradeStudent = _dbContext.Entity<TrGeneratedScheduleStudent>()
                    .Include(x => x.GeneratedScheduleGrade)
                    .ThenInclude(x => x.GeneratedSchedule)
                    .Where(x => x.GeneratedScheduleGrade.IdGrade == body.IdGrade)
                    .Where(x => x.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable);
                var countDataGradeStudent = await dataGradeStudent.CountAsync();
                if (countDataGradeStudent == 0)
                {
                    var dataGrade = await dataGradeStudent.ToListAsync();
                    var grades = new List<TrGeneratedScheduleGrade>();
                    foreach (var item in dataGrade)
                    {
                        item.GeneratedScheduleGrade.IsActive = false;
                        grades.Add(item.GeneratedScheduleGrade);
                    }

                    _dbContext.Entity<TrGeneratedScheduleGrade>().UpdateRange(grades);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                }
            }

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Dispose();
            return base.OnException(ex);
        }
    }
}
