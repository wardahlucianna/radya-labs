using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Schedule
{
    public class DeleteScheduleHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;

        public DeleteScheduleHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DeleteScheduleRequest>(nameof(DeleteScheduleRequest.IdSchedule));

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var schedules = new List<MsSchedule>();
            var schedule = await _dbContext.Entity<MsSchedule>()
                                           .Include(x => x.AscTimetableSchedules)
                                           .SingleOrDefaultAsync(x => x.Id == param.IdSchedule);
            if (schedule is null)
                throw new NotFoundException("Schedule is not found");

            schedules.Add(schedule);

            //if (schedule.Semester == 1)
            //{
            //    var relatedSchedules = await _dbContext.Entity<MsSchedule>()
            //                                           .Include(x => x.AscTimetableSchedules)
            //                                           .Where(x => x.IdDay == schedule.IdDay
            //                                                     && x.IdSession == schedule.IdSession
            //                                                     && x.IdLesson == schedule.IdLesson
            //                                                     && x.Semester != 1)
            //                                           .ToListAsync();
            //    if (relatedSchedules.Any())
            //        schedules.AddRange(relatedSchedules);
            //}

            foreach (var item in schedules)
            {
                item.IsActive = false;
                _dbContext.Entity<MsSchedule>().Update(item);

                if (item.AscTimetableSchedules.Any())
                {
                    foreach (var item2 in item.AscTimetableSchedules)
                    {
                        item2.IsActive = false;
                        _dbContext.Entity<TrAscTimetableSchedule>().Update(item2);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }

        protected override void OnFinally()
        {
            _transaction?.Dispose();
        }
    }
}
