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

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class UpdateGenerateScheduleLessonByStudentStatusHandler : FunctionsHttpSingleHandler, IDisposable
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<UpdateGenerateScheduleLessonByStudentStatusHandler> _logger;

        public UpdateGenerateScheduleLessonByStudentStatusHandler(
            DbContextOptions<SchedulingDbContext> options,
            IMachineDateTime dateTime,
            ILogger<UpdateGenerateScheduleLessonByStudentStatusHandler> logger)
        {
            _dbContext = new SchedulingDbContext(options); 
            _dateTime = dateTime;
            _logger = logger;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            await UpdateGenerate();

            return Request.CreateApiResult2();

        }

        public async Task UpdateGenerate()
        {
            var currentAY = await _dbContext.Entity<MsPeriod>()
               .Include(x => x.Grade)
                   .ThenInclude(x => x.Level)
                       .ThenInclude(x => x.AcademicYear)
               .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
               .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
               .Select(x => new
               {
                   Id = x.Grade.Level.AcademicYear.Id
               }).ToListAsync();

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear})
                .Where(x => x.StartDate <= _dateTime.ServerTime.Date && currentAY.Select(y => y.Id).ToList().Contains(x.IdAcademicYear) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

            if (listStudentStatus is null)
            {
                //throw new NotFoundException("Current academic year is not defined");
                _logger.LogInformation($"No Student Status Non Active");
                return;
            }

            foreach(var dataSS in listStudentStatus)
            {
                var getUpdate = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                   .Include(p => p.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule)
                                   .Where(p =>
                                   p.IsGenerated
                                   && p.ScheduleDate.Date >= dataSS.StartDate
                                   && p.GeneratedScheduleStudent.IdStudent == dataSS.IdStudent
                                   )
                                   .ToListAsync();

                if (getUpdate.Any())
                {
                    foreach (var setItemfalse in getUpdate)
                    {
                        setItemfalse.IsGenerated = false;
                        setItemfalse.IsActive = false;
                        _dbContext.Entity<TrGeneratedScheduleLesson>().Update(setItemfalse);
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
