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
using NPOI.Util;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class DeleteUngenerateScheduleGradeV2Handler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public DeleteUngenerateScheduleGradeV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteUngenerateScheduleGradeV2Request, DeleteUngenerateScheduleGradeV2Validator>();
            foreach (var period in body.Periods)
            {
                var listClassId = period.UngenerateScheduleClass.Select(e => e.ClassId).ToList();

                var listScheduleLessonByClassId = await _dbContext.Entity<MsScheduleLesson>()
                            .IgnoreQueryFilters() // untuk ambil data yang tidak aktif
                            .Where(x => listClassId.Contains(x.ClassID))
                            .Where(x => x.ScheduleDate.Date >= period.Start.Date)
                            .Where(x => x.ScheduleDate.Date <= period.End.Date)
                            .Where(x => x.GeneratedScheduleGrade.IdGrade == period.IdGrade)
                            .Where(x => x.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable)
                            .ToListAsync();

                foreach (var itemUngenerateScheduleClass in period.UngenerateScheduleClass)
                {
                    var generatedLessons = listScheduleLessonByClassId
                                            .Where(e => e.ClassID == itemUngenerateScheduleClass.ClassId
                                                    && (e.IsGenerated || e.IsActive))
                                            .ToList();

                    if(itemUngenerateScheduleClass.IdSessions.Any())
                        generatedLessons = generatedLessons.Where(e => itemUngenerateScheduleClass.IdSessions.Contains(e.IdSession)).ToList();

                    if (itemUngenerateScheduleClass.IdDays.Any())
                        generatedLessons = generatedLessons.Where(e => itemUngenerateScheduleClass.IdDays.Contains(e.IdDay)).ToList();

                    if (generatedLessons.Any())
                    {
                        foreach (var item in generatedLessons)
                        {
                            // set inactive generated schedule lesson
                            item.IsActive = false;
                            item.IsGenerated = false;
                            _dbContext.Entity<MsScheduleLesson>().Update(item);
                        }
                        await _dbContext.SaveChangesAsync(CancellationToken);
                    }

                    //get data schedule lesson for reset other table
                    var listGeneratedScheduleGrade = _dbContext.Entity<TrGeneratedScheduleGrade>()
                                                    .Where(x => x.IdGrade == period.IdGrade 
                                                            && x.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable
                                                            && !x.ScheduleLessons.Any())
                                                    .Where(x => x.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable)
                                                    .ToList();
                    _dbContext.Entity<TrGeneratedScheduleGrade>().UpdateRange(listGeneratedScheduleGrade);
                    await _dbContext.SaveChangesAsync(CancellationToken);
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
