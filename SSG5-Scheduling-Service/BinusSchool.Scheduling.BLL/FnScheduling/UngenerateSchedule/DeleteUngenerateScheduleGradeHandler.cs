using System;
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
    public class DeleteUngenerateScheduleGradeHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public DeleteUngenerateScheduleGradeHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteUngenerateScheduleGradeRequest, DeleteUngenerateScheduleGradeValidator>();
            foreach (var period in body.Periods)
            {
                foreach (var classId in period.ClassIds)
                {
                    var generatedLessons = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                   .Where(x => x.ClassID == classId)
                   .Where(x => x.ScheduleDate >= period.Start)
                   .Where(x => x.ScheduleDate <= period.End)
                   .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == period.IdGrade)
                   .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable).ToListAsync();
                    if (generatedLessons.Any())
                    {
                        foreach (var item in generatedLessons)
                        {
                            // set inactive generated schedule lesson
                            item.IsActive = false;
                            item.IsGenerated = false;
                            _dbContext.Entity<TrGeneratedScheduleLesson>().Update(item);
                        }
                        await _dbContext.SaveChangesAsync(CancellationToken);
                        //get data schedule lesson for reset other table
                        var generatedDataByClassId = _dbContext.Entity<TrGeneratedScheduleLesson>()
                            .Include(x => x.GeneratedScheduleStudent)
                                .ThenInclude(x => x.GeneratedScheduleGrade)
                                    .ThenInclude(x => x.GeneratedSchedule)
                            .Where(x => x.ClassID == classId)
                            .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == period.IdGrade)
                            .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable)
                            .Where(x => !x.IsGenerated);
                        var totalData = await generatedDataByClassId.CountAsync();
                        if (totalData == 0)
                        {
                            var dataStudent = await generatedDataByClassId.ToListAsync();
                            foreach (var item in dataStudent)
                            {
                                item.GeneratedScheduleStudent.IsActive = false;
                                _dbContext.Entity<TrGeneratedScheduleStudent>().Update(item.GeneratedScheduleStudent);
                            }
                            await _dbContext.SaveChangesAsync(CancellationToken);

                            var generateStudent = _dbContext.Entity<TrGeneratedScheduleStudent>()
                                .Include(x => x.GeneratedScheduleGrade)
                                    .ThenInclude(x => x.GeneratedSchedule)
                            .Where(x => x.GeneratedScheduleGrade.IdGrade == period.IdGrade)
                            .Where(x => x.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable);

                            var totalDataStudent = await generateStudent.CountAsync();

                            if (totalDataStudent == 0)
                            {
                                var dataGrade = await generateStudent.ToListAsync();
                                foreach (var item in dataGrade)
                                {
                                    item.GeneratedScheduleGrade.IsActive = false;
                                    _dbContext.Entity<TrGeneratedScheduleGrade>().Update(item.GeneratedScheduleGrade);
                                }
                                await _dbContext.SaveChangesAsync(CancellationToken);
                            }
                        }
                    }
                }
            }
            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }
    }
}
