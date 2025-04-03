using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.Schedule.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Schedule
{
    public class UpdateScheduleHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;

        public UpdateScheduleHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateScheduleRequest, UpdateScheduleValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var schedules = new List<MsSchedule>();
            var schedule = await _dbContext.Entity<MsSchedule>()
                                           .SingleOrDefaultAsync(x => x.Id == body.IdSchedule);
            if (schedule is null)
                throw new NotFoundException("Schedule is not found");

            schedules.Add(schedule);

            //if (schedule.Semester == 1)
            //{
            //    var relatedSchedules = await _dbContext.Entity<MsSchedule>()
            //                                         .Where(x => x.IdDay == schedule.IdDay
            //                                                     && x.IdSession == schedule.IdSession
            //                                                     && x.IdLesson == schedule.IdLesson
            //                                                     && x.Semester != 1)
            //                                         .ToListAsync();
            //    if (relatedSchedules.Any())
            //        schedules.AddRange(relatedSchedules);
            //}

            var ids = schedules.Select(x => x.Id).ToList();

            var idWeek = await _dbContext.Entity<MsWeekVariantDetail>().Where(e => e.Id == body.IdWeekVarianDetail).Select(e => e.IdWeek).FirstAsync(CancellationToken);

            foreach (var item in schedules)
            {
                var getScheduleExist = await _dbContext.Entity<MsSchedule>().Where(x => x.IdLesson == schedule.IdLesson
                    && x.Id != schedule.Id
                    && x.IdWeekVarianDetail == schedule.IdWeekVarianDetail
                    && x.IdWeek == idWeek
                    && x.IdSession == body.IdSession
                    && x.IdUser == schedule.IdUser
                    && x.Semester == schedule.Semester
                    && x.IdDay == body.IdDay).FirstOrDefaultAsync(CancellationToken);

                if (getScheduleExist != null)
                    throw new BadRequestException("Schedule already exist.");

                item.IdLesson = body.IdLesson;
                item.IdVenue = body.IdVenue;
                item.IdWeekVarianDetail = body.IdWeekVarianDetail;
                item.IdWeek = idWeek;
                item.IdSession = body.IdSession;
                item.IdUser = body.IdUser;
                item.IdDay = body.IdDay;
                _dbContext.Entity<MsSchedule>().Update(item);
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
