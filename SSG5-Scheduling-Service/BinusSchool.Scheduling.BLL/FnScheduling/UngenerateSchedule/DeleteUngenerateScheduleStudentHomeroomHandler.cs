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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class DeleteUngenerateScheduleStudentHomeroomHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public DeleteUngenerateScheduleStudentHomeroomHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteUngenerateScheduleStudentHomeroomRequest, DeleteUngenerateScheduleStudentHomeroomValidator>();

            foreach (var student in body.Students)
            {
                var dataLesson = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                  .Include(x => x.GeneratedScheduleStudent)
                                                        .ThenInclude(x => x.GeneratedScheduleGrade)
                                                            .ThenInclude(x => x.GeneratedSchedule)
                                                  .Where(x => x.IdHomeroom == body.IdHomeroom)
                                                  .Where(x => x.ScheduleDate >= body.Start)
                                                  .Where(x => x.ScheduleDate <= body.End)
                                                  .Where(x => x.GeneratedScheduleStudent.IdStudent == student.IdStudent)
                                                  .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == body.IdGrade)
                                                  .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable)
                                                  .ToListAsync(CancellationToken);
                if (dataLesson.Any())
                {
                    // set inactive generated schedule lesson
                    foreach (var dl in dataLesson)
                    {
                        dl.IsActive = false;
                        dl.IsGenerated = false;
                    }
                    _dbContext.Entity<TrGeneratedScheduleLesson>().UpdateRange(dataLesson);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                var dataStudentEndrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                    .Include(x=> x.HomeroomStudent)
                    .Where(x => x.HomeroomStudent.IdHomeroom == body.IdHomeroom)
                    .Where(x => x.HomeroomStudent.IdStudent == student.IdStudent);
                var countdataStudentEndrollment = await dataStudentEndrollment.CountAsync();
                if (countdataStudentEndrollment != 0)
                {
                    var dataEndrollment = await dataStudentEndrollment.ToListAsync();
                    List<MsHomeroomStudentEnrollment> Enrollments = new List<MsHomeroomStudentEnrollment>();
                    foreach (var item in dataEndrollment)
                    {
                        item.IsActive = false;
                        Enrollments.Add(item);
                    }
                    _dbContext.Entity<MsHomeroomStudentEnrollment>().UpdateRange(Enrollments);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                }

                var dataHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                    .Where(x => x.IdStudent == student.IdStudent)
                    .Where(x => x.IdHomeroom == body.IdHomeroom);
                var countdataHomeroomStudent = await dataHomeroomStudent.CountAsync();
                if (countdataHomeroomStudent != 0)
                {
                    var dataHomeroom = await dataHomeroomStudent.ToListAsync();
                    List<MsHomeroomStudent> Homerooms = new List<MsHomeroomStudent>();
                    foreach (var item in dataHomeroom)
                    {
                        item.IsActive = false;
                        Homerooms.Add(item);
                    }
                    _dbContext.Entity<MsHomeroomStudent>().UpdateRange(Homerooms);
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
