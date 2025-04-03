using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.Schedule.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Schedule
{
    public class AddScheduleHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;

        public AddScheduleHandler(
            ISchedulingDbContext dbContext,
            IApiService<ISemester> semesterService)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddScheduleRequest, AddScheduleValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var getSemesters = await _dbContext.Entity<MsPeriod>()
               .Where(x => x.IdGrade == body.IdGrade)
               .Select(x => x.Semester)
               .ToListAsync(CancellationToken);

            //var semesters = body.Semester == 1 ? getSemesters.ToList() : new List<int> { body.Semester };
            var semesters = new List<int> { body.Semester };

            var listIdWeekVarianDetail = body.Schedules.Select(e => e.IdWeekVarianDetail).ToList();
            var listWeekVariantDetail = await _dbContext.Entity<MsWeekVariantDetail>().Where(e => listIdWeekVarianDetail.Contains(e.Id)).ToListAsync(CancellationToken);

            foreach (var semester in semesters.Distinct().OrderBy(x => x))
            {
                foreach (var schedule in body.Schedules)
                {

                    var scheduleId = Guid.NewGuid().ToString();
                    var idWeek = listWeekVariantDetail.Where(e => e.Id == schedule.IdWeekVarianDetail).Select(e=>e.IdWeek).FirstOrDefault();
                    if (idWeek == null)
                        throw new BadRequestException("idWeek is not found");

                    var getScheduleExist = await _dbContext.Entity<MsSchedule>().Where(x => x.IdLesson == schedule.IdLesson
                    && x.IdWeekVarianDetail == schedule.IdWeekVarianDetail
                    && x.IdWeek == idWeek
                    && x.IdSession == body.IdSession
                    && x.IdUser == schedule.IdUser
                    && x.Semester == semester
                    && x.IdDay == body.IdDay).FirstOrDefaultAsync(CancellationToken);

                    if (getScheduleExist != null)
                        throw new BadRequestException("Schedule already exist.");

                    _dbContext.Entity<MsSchedule>().Add(new MsSchedule
                    {
                        Id = scheduleId,
                        IdLesson = schedule.IdLesson,
                        IdVenue = schedule.IdVenue,
                        IdWeekVarianDetail = schedule.IdWeekVarianDetail,
                        IdWeek = idWeek,
                        IdSession = body.IdSession,
                        IdUser = schedule.IdUser,
                        Semester = semester,
                        IdDay = body.IdDay
                    });

                    _dbContext.Entity<TrAscTimetableSchedule>().Add(new TrAscTimetableSchedule
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSchedule = scheduleId,
                        IdAscTimetable = body.IdAscTimeTable,
                        IsFromMaster = true
                    });

                    if (!_dbContext.Entity<TrAscTimetableLesson>().Any(x => x.IdAscTimetable == body.IdAscTimeTable
                                                                          && x.IdLesson == schedule.IdLesson))
                        _dbContext.Entity<TrAscTimetableLesson>().Add(new TrAscTimetableLesson
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdLesson = schedule.IdLesson,
                            IdAscTimetable = body.IdAscTimeTable,
                            IsFromMaster = true
                        });

                    await _dbContext.SaveChangesAsync(CancellationToken);
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
