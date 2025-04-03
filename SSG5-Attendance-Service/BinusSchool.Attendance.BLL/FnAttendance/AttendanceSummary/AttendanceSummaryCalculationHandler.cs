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
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class AttendanceSummaryCalculationHandler : FunctionsHttpSingleHandler, IDisposable
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<AttendanceSummaryCalculationHandler> _logger;

        public AttendanceSummaryCalculationHandler(
            DbContextOptions<AttendanceDbContext> options,
            IMachineDateTime dateTime,
            ILogger<AttendanceSummaryCalculationHandler> logger)
        {
            _dbContext = new AttendanceDbContext(options); 
            _dateTime = dateTime;
            _logger = logger;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<AttendanceSummaryCalculationRequest>(nameof(AttendanceSummaryCalculationRequest.IdSchool),
                                                                                    nameof(AttendanceSummaryCalculationRequest.Date));

            await Calculate(param);

            return Request.CreateApiResult2();

        }

        public async Task Calculate(AttendanceSummaryCalculationRequest param)
        {
            var currentAcademic = await _dbContext.Entity<MsPeriod>()
                                      .Include(x => x.Grade)
                                           .ThenInclude(x => x.Level)
                                               .ThenInclude(x => x.AcademicYear)
                                      .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool
                                                  && _dateTime.ServerTime.Date >= x.StartDate.Date
                                                  && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                      .Select(x => new CurrentAcademicYearResult
                                      {
                                          Id = x.Grade.Level.AcademicYear.Id,
                                          Code = x.Grade.Level.AcademicYear.Code,
                                          Description = x.Grade.Level.AcademicYear.Description,
                                          Semester = x.Semester
                                      }).FirstOrDefaultAsync();

            if (currentAcademic is null)
            {
                //throw new NotFoundException("Current academic year is not defined");
                _logger.LogInformation($"Current academic year is not defined for IdSchool {param.IdSchool}");
                return;
            }


            //  Get all level in school 
            var levels = await _dbContext.Entity<MsLevel>()
                                .Where(x => x.IdAcademicYear == currentAcademic.Id)
                                .Select(y => new { y.Id, y.Description} )
                                .ToListAsync();

            // Get student per level
            foreach (var level in levels)
            {
                var mapping = await _dbContext.Entity<MsMappingAttendance>()
                              .Where(x => x.IdLevel == level.Id)
                              .Select(x => new
                              {
                                  Attendances = x.AttendanceMappingAttendances.Select(y => new
                                  {
                                      y.IdAttendance,
                                      y.Attendance.Description
                                  }),
                                  x.AbsentTerms,
                                  x.IsUseWorkhabit,
                                  Workhabits = x.IsUseWorkhabit ?
                                               x.MappingAttendanceWorkhabits.Select(y => new
                                               {
                                                   y.IdWorkhabit,
                                                   y.Workhabit.Description
                                               }) : null,
                              })
                              .FirstOrDefaultAsync(CancellationToken);

                if (mapping is null)
                {
                    //throw new NotFoundException($"mapping is not found for level {currentStudentHomeroom.Grade.Level.Description}");
                    _logger.LogInformation($"Mapping is not found for level {level.Description} in school id {param.IdSchool}");
                    continue;
                }
  

                var studentHomerooms = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom).ThenInclude(y => y.Grade)
                .Where(x => x.Homeroom.Grade.IdLevel == level.Id
                       && x.Homeroom.Semester == currentAcademic.Semester)
                .Select(x => new
                {
                    idStudent = x.IdStudent,
                    idHomeroom = x.IdHomeroom
                })
                .ToListAsync();


                var students = studentHomerooms.GroupBy(x => x.idStudent);

                var idHomeRooms = studentHomerooms.Select(x => x.idHomeroom).Distinct();

                foreach (var idHomeroom in idHomeRooms)
                {
                    var studentSchedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                .Include(x => x.GeneratedScheduleStudent)
                                .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                                .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance)
                                .Where(x => x.IdHomeroom == idHomeroom //idHomeRooms.Contains(x.IdHomeroom)
                                       //&& x.ScheduleDate.Date <= param.Date.Date
                                       && x.IsGenerated)
                                .ToListAsync();

                    foreach (var student in students)
                    {
                        if (studentSchedules.Any(x=>x.GeneratedScheduleStudent.IdStudent == student.Key))
                        {
                            var summary = await _dbContext.Entity<TrAttendanceSummary>()
                                                    .Include(x => x.AttendanceSummaryMappingAtds)
                                                    .Include(x => x.AttendanceSummaryWorkhabits)
                                                    .Where(x => x.IdStudent == student.Key
                                                           && x.Date.Date == param.Date.Date)
                                                    .FirstOrDefaultAsync();

                            if (summary is null)
                            {

                                var newSummary = new TrAttendanceSummary
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdStudent = student.Key,
                                    Date = param.Date.Date,
                                    TotalDays = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key)
                                                                .GroupBy(x => x.ScheduleDate).Count(),
                                    TotalSession = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key).Count(),
                                };

                                _dbContext.Entity<TrAttendanceSummary>().Add(newSummary);

                                foreach (var attendance in mapping.Attendances)
                                {
                                    if (studentSchedules.Any(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                             && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.IdAttendance == attendance.IdAttendance)))
                                    {
                                        _dbContext.Entity<TrAttendanceSummaryMappingAtd>().Add(new TrAttendanceSummaryMappingAtd
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdAttendanceSummary = newSummary.Id,
                                            IdAttendance = attendance.IdAttendance,
                                            CountAsDay = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                  && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.IdAttendance == attendance.IdAttendance && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                                            CountAsSession = studentSchedules.Count(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                  && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.IdAttendance == attendance.IdAttendance && y.Status == AttendanceEntryStatus.Submitted)),
                                        });
                                    }
                                }

                                if (mapping.IsUseWorkhabit)
                                {
                                    foreach (var workhabit in mapping.Workhabits)
                                    {
                                        if (studentSchedules.Any(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                             && x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.IdWorkhabit == workhabit.IdWorkhabit))))
                                        {
                                            _dbContext.Entity<TrAttendanceSummaryWorkhabit>().Add(new TrAttendanceSummaryWorkhabit
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                IdAttendanceSummary = newSummary.Id,
                                                IdWorkHabit = workhabit.IdWorkhabit,
                                                CountAsDay = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                      && x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.IdWorkhabit == workhabit.IdWorkhabit) && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                                                CountAsSession = studentSchedules.Count(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                  && x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.IdWorkhabit == workhabit.IdWorkhabit) && y.Status == AttendanceEntryStatus.Submitted)),
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                summary.TotalDays = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key)
                                                                .GroupBy(x => x.ScheduleDate).Count();
                                summary.TotalSession = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key).Count();

                                _dbContext.Entity<TrAttendanceSummary>().Update(summary);

                                foreach (var attendance in mapping.Attendances)
                                {
                                    if (studentSchedules.Any(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                             && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.IdAttendance == attendance.IdAttendance)))
                                    {
                                        var extSummaryMappingAtd = summary.AttendanceSummaryMappingAtds.Where(x => x.IdAttendanceSummary == summary.Id
                                                                                                      && x.IdAttendance == attendance.IdAttendance).FirstOrDefault();

                                        if (extSummaryMappingAtd is null)
                                        {
                                            _dbContext.Entity<TrAttendanceSummaryMappingAtd>().Add(new TrAttendanceSummaryMappingAtd
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                IdAttendanceSummary = summary.Id,
                                                IdAttendance = attendance.IdAttendance,
                                                CountAsDay = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                      && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.IdAttendance == attendance.IdAttendance && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                                                CountAsSession = studentSchedules.Count(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                      && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.IdAttendance == attendance.IdAttendance && y.Status == AttendanceEntryStatus.Submitted)),
                                            });
                                        }
                                        else
                                        {
                                            extSummaryMappingAtd.CountAsDay = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                      && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.IdAttendance == attendance.IdAttendance && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count();
                                            extSummaryMappingAtd.CountAsSession = studentSchedules.Count(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                      && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.IdAttendance == attendance.IdAttendance && y.Status == AttendanceEntryStatus.Submitted));

                                            _dbContext.Entity<TrAttendanceSummaryMappingAtd>().Update(extSummaryMappingAtd);
                                        }
                                    }
                                }
                                if (mapping.IsUseWorkhabit)
                                {
                                    foreach (var workhabit in mapping.Workhabits)
                                    {
                                        if (studentSchedules.Any(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                             && x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.IdWorkhabit == workhabit.IdWorkhabit))))
                                        {
                                            var extSummaryWorkHabit = summary.AttendanceSummaryWorkhabits.Where(x => x.IdAttendanceSummary == summary.Id
                                                                      && x.IdWorkHabit == workhabit.IdWorkhabit).FirstOrDefault();

                                            if (extSummaryWorkHabit is null)
                                            {
                                                _dbContext.Entity<TrAttendanceSummaryWorkhabit>().Add(new TrAttendanceSummaryWorkhabit
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    IdAttendanceSummary = summary.Id,
                                                    IdWorkHabit = workhabit.IdWorkhabit,
                                                    CountAsDay = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                          && x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.IdWorkhabit == workhabit.IdWorkhabit) && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                                                    CountAsSession = studentSchedules.Count(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                      && x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.IdWorkhabit == workhabit.IdWorkhabit) && y.Status == AttendanceEntryStatus.Submitted)),
                                                });
                                            }
                                            else
                                            {
                                                extSummaryWorkHabit.CountAsDay = studentSchedules.Where(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                          && x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.IdWorkhabit == workhabit.IdWorkhabit) && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count();
                                                extSummaryWorkHabit.CountAsSession = studentSchedules.Count(x => x.GeneratedScheduleStudent.IdStudent == student.Key
                                                                                      && x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.IdWorkhabit == workhabit.IdWorkhabit) && y.Status == AttendanceEntryStatus.Submitted));
                                                _dbContext.Entity<TrAttendanceSummaryWorkhabit>().Update(extSummaryWorkHabit);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } 
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

        }

        public void Dispose()
        {
            (_dbContext as AttendanceDbContext)?.Dispose();
        }
    }
}
