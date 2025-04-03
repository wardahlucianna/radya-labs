using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnLongRun.Extensions;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnLongRun.Services
{
    public class AttendanceSummaryService : IAttendanceSummaryService
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<AttendanceSummaryService> _logger;
        private readonly Dictionary<string, string> _dictSchool;
        private readonly Dictionary<string, string> _dictAcademicYearBySchool;

        public AttendanceSummaryService(IAttendanceDbContext dbContext, IMachineDateTime dateTime,
            ILogger<AttendanceSummaryService> logger)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _logger = logger;
            _dictSchool = new Dictionary<string, string>();
            _dictAcademicYearBySchool = new Dictionary<string, string>();
        }

        public async Task<string> GetSchoolNameAsync(string idSchool, CancellationToken cancellationToken)
        {
            if (_dictSchool.ContainsKey(idSchool))
                return _dictSchool[idSchool];

            var result = await _dbContext.Entity<MsSchool>().Where(e => e.Id == idSchool)
                .Select(e => e.Description)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                return null;

            _dictSchool.Add(idSchool, result);

            return _dictSchool[idSchool];
        }

        public async Task<string> GetActiveAcademicYearAsync(string idSchool, CancellationToken cancellationToken)
        {
            if (_dictAcademicYearBySchool.ContainsKey(idSchool))
                return _dictAcademicYearBySchool[idSchool];

            var id = await _dbContext.Entity<MsAcademicYear>()
                .Where(e => e.IdSchool == idSchool)
                .OrderByDescending(e => e.DateIn)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(id))
                return null;

            _dictAcademicYearBySchool.Add(idSchool, id);

            return _dictAcademicYearBySchool[idSchool];
        }

        public async Task DeleteTermByAcademicYearAsync(string academicYear, CancellationToken cancellationToken)
        {
            await using (var command = _dbContext.DbFacade.GetDbConnection().CreateCommand())
            {
                command.CommandText = @$"
DELETE FROM dbo.TrAttendanceSummaryTerm
WHERE IdAcademicYear = '{academicYear}';
";
                command.CommandType = CommandType.Text;

                await _dbContext.DbFacade.OpenConnectionAsync(cancellationToken);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public async Task<List<GradeDto>> GetGradesAsync(string idSchool, string idAcademicYear,
            CancellationToken cancellationToken)
        {
            //get list levels by academic year
            var listLevel = new List<LevelDto>();
            var levels = await _dbContext.Entity<MsLevel>()
                .Where(e => e.IdAcademicYear == idAcademicYear)
                .Select(e => new
                {
                    IdLevel = e.Id,
                    e.IdAcademicYear,
                    Name = e.Description
                })
                .ToListAsync(cancellationToken);
            if (levels == null)
                throw new Exception($"levels in empty with id academic year {idAcademicYear}");

            listLevel.AddRange(levels.Select(level => new LevelDto
            {
                IdLevel = level.IdLevel,
                LevelName = level.Name,
                IdAcademicYear = level.IdAcademicYear,
                IdSchool = idSchool
            }));

            //get list grades by level
            var listGrade = new List<GradeDto>();
            foreach (var level in listLevel)
            {
                var grades = await _dbContext.Entity<MsGrade>()
                    .Where(e => e.IdLevel == level.IdLevel)
                    .Select(e => new { e.Id, e.Code, e.Description, e.OrderNumber })
                    .ToListAsync(cancellationToken);

                if (!grades.Any())
                    throw new Exception(
                        $"Data grade by level {level.IdLevel}/{level.LevelName} is empty so it will be skipped");

                listGrade.AddRange(grades.Select(grade => new GradeDto
                {
                    IdLevel = level.IdLevel,
                    LevelName = level.LevelName,
                    IdGrade = grade.Id,
                    IdAcademicYear = level.IdAcademicYear,
                    IdSchool = level.IdSchool,
                    GradeName = grade.Description,
                    GradeOrder = grade.OrderNumber
                }));
            }

            return listGrade;
        }

        public Task<List<string>> GetStudentsByGradeAsync(string idGrade, CancellationToken cancellationToken)
            => _dbContext.Entity<MsHomeroomStudent>()
                .Include(e => e.Homeroom)
                .Where(e => e.Homeroom.IdGrade == idGrade)
                .Select(e => e.IdStudent)
                .GroupBy(e => e)
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

        public Task<int> GetTotalStudentByGradeAsync(string idGrade, CancellationToken cancellationToken)
            => _dbContext.Entity<MsHomeroomStudent>()
                .Include(e => e.Homeroom)
                .Where(e => e.Homeroom.IdGrade == idGrade)
                .Select(e => e.IdStudent)
                .GroupBy(e => e)
                .Select(e => e.Key)
                .CountAsync(cancellationToken);

        public Task<List<PeriodDto>> GetPeriodsAsync(string idGrade, CancellationToken cancellationToken)
            => _dbContext.Entity<MsPeriod>()
                .Where(e => e.IdGrade == idGrade)
                .OrderBy(e => e.Code)
                .Select(e => new PeriodDto
                {
                    IdPeriod = e.Id,
                    PeriodCode = e.Code,
                    PeriodName = e.Description,
                    PeriodOrder = e.OrderNumber,
                    PeriodStartDt = e.StartDate,
                    PeriodEndDt = e.EndDate,
                    PeriodAttendanceStartDt = e.AttendanceStartDate,
                    PeriodAttendanceEndDt = e.AttendanceEndDate,
                    PeriodSemester = e.Semester
                })
                .ToListAsync(cancellationToken);

        public Task<List<Summary>> GetSummaryPerGradeAsync(string idGrade, CancellationToken cancellationToken)
            => _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Where(e => e.IdGrade == idGrade && e.IsGenerated && e.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                .Select(e => new Summary
                {
                    Id = e.Id,
                    IdGrade = e.IdGrade,
                    IdLesson = e.IdLesson,
                    IdStudent = e.IdStudent,
                    IdHomeroom = e.IdHomeroom,
                    IdAcademicYear = e.IdAcademicYear,
                    IdLevel = e.IdLevel,
                    IdSubject = e.IdSubject,
                    IdSession = e.IdSession,
                    ScheduleDt = e.ScheduleDate,
                    StartPeriod = e.StartPeriod,
                    EndPeriod = e.EndPeriod
                })
                .ToListAsync(cancellationToken);

        public async Task<List<Summary>> GetSummaryPerGradeIncludeEntriesAsync(string idGrade, string idSchool,
            List<PeriodDto> periods, List<MsMappingAttendance> mappingAttendances,
            List<MsMappingAttendanceWorkhabit> mappingAttendanceWorkhabits, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var summaries = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(e => e.AttendanceEntries)
                .ThenInclude(e => e.AttendanceEntryWorkhabits)
                .Where(e => e.IdGrade == idGrade && e.IsGenerated && e.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                .Select(e => new
                {
                    e.Id,
                    e.IdGrade,
                    e.IdLesson,
                    e.IdStudent,
                    e.IdHomeroom,
                    e.IdAcademicYear,
                    e.IdLevel,
                    e.IdSubject,
                    e.IdSession,
                    e.ScheduleDate,
                    e.StartPeriod,
                    e.EndPeriod,
                    e.AttendanceEntries
                })
                .ToListAsync(cancellationToken);

            sw.Stop();

            _logger.LogInformation("Total data summary by grade id {Total}, within {Seconds}s", summaries.Count,
                Math.Round(sw.Elapsed.TotalSeconds, 2));

            var list = new List<Summary>();

            foreach (var e in summaries)
            {
                var summary = new Summary
                {
                    Id = e.Id,
                    IdGrade = e.IdGrade,
                    IdLesson = e.IdLesson,
                    IdStudent = e.IdStudent,
                    IdHomeroom = e.IdHomeroom,
                    IdAcademicYear = e.IdAcademicYear,
                    IdLevel = e.IdLevel,
                    IdSubject = e.IdSubject,
                    IdSession = e.IdSession,
                    ScheduleDt = e.ScheduleDate,
                    StartPeriod = e.StartPeriod,
                    EndPeriod = e.EndPeriod,
                    IdSchool = idSchool
                };
                summary.Mapping(periods);

                if (string.IsNullOrWhiteSpace(summary.IdPeriod))
                    continue;

                if (e.AttendanceEntries.Any())
                {
                    var entry = e.AttendanceEntries.First();
                    summary.IdAttendanceMappingAttendance = entry.IdAttendanceMappingAttendance;
                    summary.EntryStatus = entry.Status;

                    if (entry.AttendanceEntryWorkhabits.Any())
                        foreach (var item2 in entry.AttendanceEntryWorkhabits)
                            summary.Workhabits.Add(new WorkHabit
                            {
                                IdEntryWorkHabit = item2.Id,
                                IdMappingAttendanceWorkHabit = item2.IdMappingAttendanceWorkhabit
                            });
                }

                list.Add(summary);
            }

            _logger.LogInformation("End mapping data summary of grade id {Id}", idGrade);

            return list;
        }

        public async Task<Entry> GetEntryAsync(string id, CancellationToken cancellationToken)
        {
            var entry = await _dbContext.Entity<TrAttendanceEntry>()
                .Where(e => e.IsActive && e.IdGeneratedScheduleLesson == id)
                .Select(e => new Entry
                {
                    IdEntry = e.Id,
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    LateTime = e.LateTime,
                    PositionIn = e.PositionIn,
                    EntryStatus = e.Status
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (entry == null) return null;

            entry.Workhabits.AddRange(await _dbContext.Entity<TrAttendanceEntryWorkhabit>()
                .Where(e => e.IdAttendanceEntry == entry.IdEntry)
                .Select(e => new WorkHabit
                {
                    IdEntryWorkHabit = e.Id,
                    IdMappingAttendanceWorkHabit = e.IdMappingAttendanceWorkhabit,
                }).ToListAsync(cancellationToken));

            return entry;
        }

        public async Task<List<Entry>> GetEntriesAsync(List<string> listId, CancellationToken cancellationToken)
        {
            var list = new List<Entry>();

            var listChunked = new List<List<string>>();

            listChunked.AddRange(listId.ChunkBy2(1500));

            const int maxRetry = 3;

            foreach (var item in listChunked)
            {
                var @do = true;
                var retries = 0;
                while (@do)
                    try
                    {
                        var entries = await _dbContext.Entity<TrAttendanceEntry>()
                            .Include(e => e.AttendanceMappingAttendance)
                            .Where(e => e.IsActive && item.Contains(e.IdGeneratedScheduleLesson))
                            .Select(e => new Entry
                            {
                                IdScheduleLesson = e.IdGeneratedScheduleLesson,
                                IdEntry = e.Id,
                                IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                                LateTime = e.LateTime,
                                PositionIn = e.PositionIn,
                                EntryStatus = e.Status,
                                IdAttendance = e.AttendanceMappingAttendance.IdAttendance
                            })
                            .ToListAsync(cancellationToken);

                        if (!entries.Any())
                            break;

                        var entryIds = entries.Select(e => e.IdEntry).ToList();

                        var workhabits = await _dbContext.Entity<TrAttendanceEntryWorkhabit>()
                            .Where(e => entryIds.Contains(e.IdAttendanceEntry))
                            .Select(e => new WorkHabit
                            {
                                IdEntry = e.IdAttendanceEntry,
                                IdEntryWorkHabit = e.Id,
                                IdMappingAttendanceWorkHabit = e.IdMappingAttendanceWorkhabit,
                            }).ToListAsync(cancellationToken);

                        foreach (var entry in entries)
                        {
                            var internalWorkhabits = workhabits.Where(e => e.IdEntry == entry.IdEntry).ToList();
                            if (internalWorkhabits.Any())
                                entry.Workhabits.AddRange(internalWorkhabits);
                        }

                        list.AddRange(entries);

                        @do = false;
                        Thread.Sleep(1000);
                    }
                    catch (Exception)
                    {
                        retries++;

                        if (retries < maxRetry)
                            throw;
                    }
            }

            return list;
        }

        public List<Entry> GetEntries(List<string> listId)
        {
            _logger.LogInformation("Total data id {Total}", listId.Count);

            var list = new List<Entry>();

            var listChunked = new List<List<string>>();

            listChunked.AddRange(listId.ChunkBy2(2000));

            _logger.LogInformation("Chunked total with maximum iteration {Total}, start from 0", listChunked.Count - 1);

            var iteration = 0;

            foreach (var item in listChunked)
            {
                _logger.LogInformation("Iteration {Iteration} started", iteration);

                var entries = _dbContext.Entity<TrAttendanceEntry>()
                    .Include(e => e.AttendanceMappingAttendance)
                    .Where(e => e.IsActive && item.Contains(e.IdGeneratedScheduleLesson))
                    .Select(e => new Entry
                    {
                        IdScheduleLesson = e.IdGeneratedScheduleLesson,
                        IdEntry = e.Id,
                        IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                        LateTime = e.LateTime,
                        PositionIn = e.PositionIn,
                        EntryStatus = e.Status,
                        IdAttendance = e.AttendanceMappingAttendance.IdAttendance
                    })
                    .ToList();

                if (!entries.Any())
                    continue;

                var entryIds = entries.Select(e => e.IdEntry).ToList();

                var workhabits = _dbContext.Entity<TrAttendanceEntryWorkhabit>()
                    .Where(e => entryIds.Contains(e.IdAttendanceEntry))
                    .Select(e => new WorkHabit
                    {
                        IdEntry = e.IdAttendanceEntry,
                        IdEntryWorkHabit = e.Id,
                        IdMappingAttendanceWorkHabit = e.IdMappingAttendanceWorkhabit,
                    }).ToList();

                foreach (var entry in entries)
                {
                    var internalWorkhabits = workhabits.Where(e => e.IdEntry == entry.IdEntry).ToList();
                    if (internalWorkhabits.Any())
                        entry.Workhabits.AddRange(internalWorkhabits);
                }

                list.AddRange(entries);

                iteration++;
            }

            _logger.LogInformation("Get data entry completed with total {Total}", list.Count);

            return list;
        }

        public Task<List<MsAttendanceMappingAttendance>> GetAttendanceMappingAttendanceAsync(
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MsMappingAttendance>> GetMappingAttendanceAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MsMappingAttendanceWorkhabit>> GetMappingAttendanceWorkhabitAsync(
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MsSchoolMappingEA>> GetSchoolMappingEaAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MsAttendance>> GetMasterAttendanceAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
