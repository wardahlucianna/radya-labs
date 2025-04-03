using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.SchedulingDb;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using System.Transactions;
using BinusSchool.Common.Utils;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class UpdateGenerateScheduleLessonByEventHandler : FunctionsHttpSingleHandler, IDisposable
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<UpdateGenerateScheduleLessonByEventHandler> _logger;

        public UpdateGenerateScheduleLessonByEventHandler(
            DbContextOptions<SchedulingDbContext> options,
            IMachineDateTime dateTime,
            ILogger<UpdateGenerateScheduleLessonByEventHandler> logger)
        {
            _dbContext = new SchedulingDbContext(options); 
            _dateTime = dateTime;
            _logger = logger;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            // string IdEvent = null;
            // await UpdateGenerateScheduleLessonByEvent(IdEvent);

            return Request.CreateApiResult2();

        }

        public async Task UpdateGenerateScheduleLessonByEvent(string IdEvent, string Action)
        {
            // var currentAY = await _dbContext.Entity<MsPeriod>()
            //    .Include(x => x.Grade)
            //        .ThenInclude(x => x.Level)
            //            .ThenInclude(x => x.AcademicYear)
            //    .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
            //    .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
            //    .Select(x => new
            //    {
            //        Id = x.Grade.Level.AcademicYear.Id
            //    }).ToListAsync();

            var predicate = PredicateBuilder.Create<TrEvent>(x => true);

            var query = _dbContext.Entity<TrEvent>()
                .Include(x => x.EventDetails)
                .Include(x => x.EventIntendedFor)
                    .ThenInclude(x => x.EventIntendedForLevelStudents)
                .Include(x => x.EventIntendedFor)
                    .ThenInclude(x => x.EventIntendedForGradeStudents)
                .Include(x => x.EventIntendedFor)
                    .ThenInclude(x => x.EventIntendedForPersonalStudents)
                .Include(x => x.EventIntendedFor)
                    .ThenInclude(x => x.EventIntendedForAttendanceStudents)
                .Where(predicate).Where(x => x.Id == IdEvent && x.EventIntendedFor.Any(y => y.EventIntendedForAttendanceStudents.Any(z => z.Type == EventIntendedForAttendanceStudent.NoAtdClass)));

            var trEvent = await query
                    .SingleOrDefaultAsync(CancellationToken);

            bool IsGenerated = true;

            var predicateGenerate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => true);
            
            if(Action == "Approved")
            {
                IsGenerated = false;
                predicateGenerate = predicateGenerate.And(x => x.IsGenerated == true);
            }
            else
            {
                IsGenerated = true;
                predicateGenerate = predicateGenerate.And(x => x.IsGenerated == false);
            }

            foreach (var eventDetail in trEvent.EventDetails)
            {
                foreach (var intendedFor in trEvent.EventIntendedFor)
                {
                    if(intendedFor.IntendedFor == "ALL" && intendedFor.Option == "None")
                    {
                        var getUpdateGenerate = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                    .Include(p => p.Homeroom.AcademicYear)
                                    .Where(predicateGenerate)
                                    .Where(p =>
                                    p.Homeroom.AcademicYear.Id == trEvent.IdAcademicYear
                                    && p.ScheduleDate.Date >= eventDetail.StartDate.Date
                                    && p.ScheduleDate.Date <= eventDetail.EndDate.Date
                                    && p.StartTime >= eventDetail.StartDate.TimeOfDay
                                    && p.EndTime <= eventDetail.EndDate.TimeOfDay
                                    )
                                    .ToListAsync();

                        if (getUpdateGenerate.Any())
                        {
                            foreach (var setItemfalse in getUpdateGenerate)
                            {
                                setItemfalse.IsGenerated = IsGenerated;
                                _dbContext.Entity<TrGeneratedScheduleLesson>().Update(setItemfalse);
                            }
                        }
                    }
                    else if(intendedFor.IntendedFor == "STUDENT" && intendedFor.Option == "ALL")
                    {
                        var getUpdateGenerate = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                    .Include(p => p.Homeroom.AcademicYear)
                                    .Where(predicateGenerate)
                                    .Where(p =>
                                    p.Homeroom.AcademicYear.Id == trEvent.IdAcademicYear
                                    && p.ScheduleDate.Date >= eventDetail.StartDate.Date
                                    && p.ScheduleDate.Date <= eventDetail.EndDate.Date
                                    && p.StartTime >= eventDetail.StartDate.TimeOfDay
                                    && p.EndTime <= eventDetail.EndDate.TimeOfDay
                                    )
                                    .ToListAsync();

                        if (getUpdateGenerate.Any())
                        {
                            foreach (var setItemfalse in getUpdateGenerate)
                            {
                                setItemfalse.IsGenerated = IsGenerated;
                                _dbContext.Entity<TrGeneratedScheduleLesson>().Update(setItemfalse);
                            }
                        }
                    }
                    else if(intendedFor.IntendedFor == "STUDENT" && intendedFor.Option == "Level")
                    {
                        var getUpdateGenerate = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                    .Include(p => p.Homeroom.AcademicYear)
                                    .Where(predicateGenerate)
                                    .Where(p =>
                                    p.Homeroom.AcademicYear.Id == trEvent.IdAcademicYear
                                    && intendedFor.EventIntendedForLevelStudents.Select(y => y.IdLevel).ToList().Contains(p.IdLevel) 
                                    && p.ScheduleDate.Date >= eventDetail.StartDate.Date
                                    && p.ScheduleDate.Date <= eventDetail.EndDate.Date
                                    && p.StartTime >= eventDetail.StartDate.TimeOfDay
                                    && p.EndTime <= eventDetail.EndDate.TimeOfDay
                                    )
                                    .ToListAsync();

                        if (getUpdateGenerate.Any())
                        {
                            foreach (var setItemfalse in getUpdateGenerate)
                            {
                                setItemfalse.IsGenerated = IsGenerated;
                                _dbContext.Entity<TrGeneratedScheduleLesson>().Update(setItemfalse);
                            }
                        }
                    }
                    else if(intendedFor.IntendedFor == "STUDENT" && intendedFor.Option == "Grade")
                    {
                        var getUpdateGenerate = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                    .Include(p => p.Homeroom.AcademicYear)
                                    .Where(predicateGenerate)
                                    .Where(p =>
                                    p.Homeroom.AcademicYear.Id == trEvent.IdAcademicYear
                                    && intendedFor.EventIntendedForGradeStudents.Select(y => y.IdHomeroom).ToList().Contains(p.IdHomeroom)
                                    && p.ScheduleDate.Date >= eventDetail.StartDate.Date
                                    && p.ScheduleDate.Date <= eventDetail.EndDate.Date
                                    && p.StartTime >= eventDetail.StartDate.TimeOfDay
                                    && p.EndTime <= eventDetail.EndDate.TimeOfDay
                                    )
                                    .ToListAsync();

                        if (getUpdateGenerate.Any())
                        {
                            foreach (var setItemfalse in getUpdateGenerate)
                            {
                                setItemfalse.IsGenerated = IsGenerated;
                                _dbContext.Entity<TrGeneratedScheduleLesson>().Update(setItemfalse);
                            }
                        }
                    }
                    else if(intendedFor.IntendedFor == "STUDENT" && intendedFor.Option == "Personal")
                    {
                        var getUpdateGenerate = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                    .Include(p => p.Homeroom.AcademicYear)
                                    .Where(predicateGenerate)
                                    .Where(p =>
                                    p.Homeroom.AcademicYear.Id == trEvent.IdAcademicYear
                                    && intendedFor.EventIntendedForPersonalStudents.Select(y => y.IdStudent).ToList().Contains(p.IdStudent)
                                    && p.ScheduleDate.Date >= eventDetail.StartDate.Date
                                    && p.ScheduleDate.Date <= eventDetail.EndDate.Date
                                    && p.StartTime >= eventDetail.StartDate.TimeOfDay
                                    && p.EndTime <= eventDetail.EndDate.TimeOfDay
                                    )
                                    .ToListAsync();

                        if (getUpdateGenerate.Any())
                        {
                            foreach (var setItemfalse in getUpdateGenerate)
                            {
                                setItemfalse.IsGenerated = IsGenerated;
                                _dbContext.Entity<TrGeneratedScheduleLesson>().Update(setItemfalse);
                            }
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

        }

        public void Dispose()
        {
            (_dbContext as SchedulingDbContext)?.Dispose();
        }
    }
}
