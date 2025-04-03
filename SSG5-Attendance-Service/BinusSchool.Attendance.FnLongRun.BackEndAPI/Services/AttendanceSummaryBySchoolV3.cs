using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Exception = System.Exception;

namespace BinusSchool.Attendance.FnLongRun.Services
{
    public class AttendanceSummaryBySchoolV3
    {
#if DEBUG
        private const string _containerPath = "attendance-summary-debug";
#else
        private const string _containerPath = "attendance-summary";
#endif

        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _machineDateTime;
        private readonly IConfiguration _configuration;
        private readonly IAttendanceSummaryV3Service _attendanceSummaryV3Service;
        private readonly ILogger<AttendanceSummaryBySchoolV3> _logger;

        public AttendanceSummaryBySchoolV3(IAttendanceDbContext dbContext,
            IMachineDateTime machineDateTime,
            IConfiguration configuration,
            IAttendanceSummaryV3Service attendanceSummaryV3Service,
            ILogger<AttendanceSummaryBySchoolV3> logger)
        {
            _dbContext = dbContext;
            _machineDateTime = machineDateTime;
            _configuration = configuration;
            _attendanceSummaryV3Service = attendanceSummaryV3Service;
            _logger = logger;
        }

        public CloudStorageAccount GetCloudStorageAccount()
        {
            var s = _configuration["ConnectionStrings:Attendance:AccountStorage"];

#if DEBUG
            s = "UseDevelopmentStorage=true";
#endif

            var storageAccount = CloudStorageAccount.Parse(s);
            return storageAccount;
        }

        public void CreateContainerIfNotExists(string containerName)
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var containers = new[] { containerName };

            foreach (var item in containers)
            {
                var blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }

        public async Task RunAsync(string idSchool, CancellationToken cancellationToken)
        {
            try
            {
                await ExecuteAsync(idSchool, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurs");
            }
        }

        private string _idAcademicYear;

        public async Task RunAsync(string idSchool, string idAcademicYear, CancellationToken cancellationToken)
        {
            try
            {
                _idAcademicYear = idAcademicYear;
                await ExecuteAsync(idSchool, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurs");
            }
        }

        public async Task ExecuteAsync(string idSchool, CancellationToken cancellationToken)
        {
            CreateContainerIfNotExists(_containerPath);
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerPath);

            var schoolName = await _attendanceSummaryV3Service.GetSchoolNameAsync(idSchool, cancellationToken);
            if (string.IsNullOrWhiteSpace(schoolName))
                throw new Exception("Data school is empty");

            _logger.LogInformation("Attendance summary cronjob v3 for {Name}", schoolName);

            var log = new TrAttendanceSummaryLog
            {
                StartDate = _machineDateTime.ServerTime
            };
            await _dbContext.Entity<TrAttendanceSummaryLog>().AddAsync(log, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            //local variables
            var mappingSchools = await _dbContext.Entity<MsSchoolMappingEA>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var mappingSchool = mappingSchools.FirstOrDefault(e => e.IdSchool == idSchool);
            if (mappingSchool == null)
                throw new Exception($"Mapping school MsSchoolMappingEA of {schoolName} is null");

            //log school
            var logSch = new TrAttdSummaryLogSch
            {
                IdAttendanceSummaryLog = log.Id,
                IdSchool = idSchool,
                SchoolName = schoolName,
                StartDate = _machineDateTime.ServerTime
            };

            await _dbContext.Entity<TrAttdSummaryLogSch>().AddAsync(logSch, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            string idAcademicYear;
            if (string.IsNullOrWhiteSpace(_idAcademicYear))
                idAcademicYear =
                    await _attendanceSummaryV3Service.GetActiveAcademicYearAsync(idSchool, cancellationToken);
            else
                idAcademicYear = _idAcademicYear;

            await _attendanceSummaryV3Service.DeleteTermByAcademicYearAsync(idAcademicYear, cancellationToken);

            var grades = await _attendanceSummaryV3Service.GetGradesAsync(idSchool,
                idAcademicYear,
                cancellationToken);

            _logger.LogInformation("Total Grade of {Name} is {Count}", schoolName, grades.Count);

            logSch.TotalGrade = grades.Count;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var sw = Stopwatch.StartNew();

            foreach (var grade in grades)
            {
                var logGrade = new TrAttdSummaryLogSchGrd
                {
                    IdAttdSummaryLogSch = logSch.Id,
                    IdGrade = grade.IdGrade,
                    GradeName = grade.GradeName,
                    //TotalStudent = students.Count
                };
                await _dbContext.Entity<TrAttdSummaryLogSchGrd>().AddAsync(logGrade, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var listSummaryPerGrade = new List<Summary>();

                _logger.LogInformation("Get data {Grade}", grade.GradeName);

                //get list period by grade
                var periods = await _attendanceSummaryV3Service.GetPeriodsAsync(grade.IdGrade, cancellationToken);

                if (!periods.Any())
                    continue;

                var lastPeriod = periods.Last();

                var startAcademicYearDt = periods.First().PeriodStartDt;

                var homerooms =
                    await _attendanceSummaryV3Service.GetHomeroomsGroupedBySemester(grade.IdGrade, cancellationToken);

                if (!homerooms.Any())
                    continue;

                var scheduleCancels = await _dbContext.Entity<TrScheduleRealization2>()
                .Where(e =>e.IdAcademicYear == idAcademicYear
                    && e.IdGrade == grade.IdGrade
                    && e.IsCancel == true)
                .Select(e => new ScheduleDto
                {
                    IdSchedule = e.Id,
                    ScheduleDate = e.ScheduleDate,
                    IdLesson = e.IdLesson,
                    IdLevel = e.IdLevel,
                    IdGrade = e.IdGrade,
                    SessionID = e.SessionID
                })
                .ToListAsync(cancellationToken);

                var scheduleData = await _dbContext.Entity<MsSchedule>()
                    .Where(x => x.Lesson.IdAcademicYear == idAcademicYear && x.Lesson.IdGrade == grade.IdGrade)
                    .ToListAsync(cancellationToken);                    

                foreach (var period in periods)
                {
                    //skip if period
                    if (period.PeriodStartDt.Date >= _machineDateTime.ServerTime.Date)
                        continue;

                    DateTime endDt;
                    if (period.PeriodEndDt.Date >= _machineDateTime.ServerTime.Date)
                        endDt = _machineDateTime.ServerTime.Date;
                    else
                        endDt = period.PeriodEndDt.Date;

                    var schedules =
                        await _attendanceSummaryV3Service.GetScheduleAsync(idAcademicYear,
                            grade.IdGrade,
                            period.PeriodStartDt,
                            endDt,
                            cancellationToken);

                    var scheduleGroupedByLesson = schedules
                        .GroupBy(e => e.IdLesson)
                        .ToDictionary(g => g.Key, t => t.ToList());

                    var idSchedules = schedules.Select(e => e.IdSchedule).ToArray();

                    var dictAttendanceEntry =
                        await _attendanceSummaryV3Service.GetAttendanceEntriesGroupedAsync(
                            idSchedules,
                            cancellationToken);

                    foreach (var homeroom in homerooms[period.PeriodSemester])
                    {
                        var students = await _attendanceSummaryV3Service.GetStudentEnrolled(homeroom.IdHomeroom,
                            startAcademicYearDt, cancellationToken);

                        var studentIds = students.Select(e => e.IdStudent)
                            .Distinct()
                            .ToArray();

                        var studentStatuses = await _attendanceSummaryV3Service
                            .GetStudentStatusesAsync(
                                studentIds,
                                idAcademicYear,
                                lastPeriod.PeriodAttendanceEndDt,
                                cancellationToken);

                        foreach (var studentEnrollment in students)
                        {
                            var studentStatus = studentStatuses
                                .Where(e => e.IdStudent == studentEnrollment.IdStudent).ToList();

                            //student status is null or not active, skipped
                            if (studentStatus.Any() == false)
                                continue;

                            for (var i = 0; i < studentEnrollment.Items.Count; i++)
                            {
                                var current = studentEnrollment.Items[i];

                                if (string.IsNullOrWhiteSpace(current.IdLesson))
                                    continue;

                                if (current.Ignored || !scheduleGroupedByLesson.ContainsKey(current.IdLesson))
                                    continue;

                                scheduleGroupedByLesson.TryGetValue(studentEnrollment.Items[i].IdLesson,
                                    out var value);

                                if (!value.Any())
                                    continue;

                                value = value
                                    .Where(e => current.StartDt.Date <= e.ScheduleDate.Date &&
                                                e.ScheduleDate.Date < current.EndDt.Date)
                                    .OrderBy(e => e.ScheduleDate)
                                    .ToList();

                                //no schedule, then skip to next iterations
                                if (!value.Any())
                                    continue;

                                foreach (var schedule in value)
                                {
                                    // check if student status start date, and end date both are satisfied the schedule date
                                    //if (!(studentStatus.StartDt.Date <= schedule.ScheduleDate.Date &&
                                    //      schedule.ScheduleDate.Date <= studentStatus.EndDt.Value.Date))
                                    //    continue;
                                    if (studentStatus.Any(x => schedule.ScheduleDate.Date >= x.StartDt &&
                                    schedule.ScheduleDate.Date <= x.EndDt.Value.Date &&
                                    x.IdStudent == studentEnrollment.IdStudent) == false)
                                        continue;

                                    if (scheduleCancels.Any(x => x.IdLesson == schedule.IdLesson && x.ScheduleDate == schedule.ScheduleDate
                                        && x.SessionID == schedule.SessionID))
                                        continue;

                                    if (!scheduleData.Any(x => x.IdLesson == schedule.IdLesson && x.IdSession == schedule.IdSession
                                        && x.IdDay == schedule.IdDay))
                                        continue;

                                    dictAttendanceEntry.TryGetValue(schedule.IdSchedule,
                                        out var attendanceEntries);

                                    var summary = new Summary
                                    {
                                        Id = schedule.IdSchedule,
                                        IdGrade = grade.IdGrade,
                                        IdLesson = schedule.IdLesson,
                                        IdStudent = studentEnrollment.IdStudent,
                                        IdHomeroom = homeroom.IdHomeroom,
                                        IdAcademicYear = idAcademicYear,
                                        IdLevel = schedule.IdLevel,
                                        IdSubject = schedule.IdSubject,
                                        IdSession = schedule.IdSession,
                                        ScheduleDt = schedule.ScheduleDate,
                                        IdSchool = idSchool,
                                        IdPeriod = period.IdPeriod,
                                        Term = Convert.ToInt32(Regex.Match(period.PeriodCode, @"\d+").Value),
                                        Semester = period.PeriodSemester
                                    };

                                    if (attendanceEntries != null && attendanceEntries.Any())
                                    {
                                        var entry = attendanceEntries.Where(e =>
                                                e.IdHomeroomStudent == studentEnrollment.IdHomeroomStudent)
                                            .OrderByDescending(e => e.DateIn)
                                            .FirstOrDefault();
                                        if (entry != null)
                                        {
                                            summary.IdAttendanceMappingAttendance =
                                                entry.IdAttendanceMappingAttendance;
                                            summary.EntryStatus = entry.Status;

                                            if (entry.Workhabits != null && entry.Workhabits.Any())
                                                foreach (var workHabit in entry.Workhabits)
                                                    summary.Workhabits.Add(new WorkHabit
                                                    {
                                                        IdEntryWorkHabit = workHabit.IdEntryWorkhabit,
                                                        IdMappingAttendanceWorkHabit =
                                                            workHabit.IdMappingAttendanceWorkHabit
                                                    });
                                        }
                                    }

                                    listSummaryPerGrade.Add(summary);
                                }
                            }
                        }
                    }
                }

                _logger.LogInformation("Total data summary per grade {Total}", listSummaryPerGrade.Count);

                //calculation
                var groupByStudent = listSummaryPerGrade
                    .GroupBy(e => new
                    {
                        e.IdStudent,
                        e.IdPeriod,
                        e.IdAcademicYear,
                        e.IdSchool,
                        e.IdLevel,
                        e.IdGrade,
                        e.IdHomeroom,
                        e.Term,
                        e.Semester
                    })
                    .Select(e => new SummaryDto
                    {
                        IdStudent = e.Key.IdStudent,
                        IdPeriod = e.Key.IdPeriod,
                        IdAcademicYear = e.Key.IdAcademicYear,
                        IdSchool = e.Key.IdSchool,
                        IdLevel = e.Key.IdLevel,
                        IdGrade = e.Key.IdGrade,
                        IdHomeroom = e.Key.IdHomeroom,
                        Term = e.Key.Term,
                        Semester = e.Key.Semester,
                        Items = e.OrderBy(f => f.ScheduleDt).ToList()
                    })
                    .ToList();

                var counter = 0;
                foreach (var item in groupByStudent)
                {
                    var filename = $"{idSchool}_{schoolName}_{item.IdStudent}_{Guid.NewGuid()}.json";
                    var blob = container.GetBlockBlobReference(filename);
                    blob.Properties.ContentType = "application/json";

                    var s = JsonConvert.SerializeObject(item,
                        new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });

                    using var ms = new MemoryStream();
                    await using var streamWriter = new StreamWriter(ms);
                    await streamWriter.WriteAsync(s);
                    await streamWriter.FlushAsync();
                    ms.Position = 0;

                    //upload file
                    await blob.UploadFromStreamAsync(ms);

                    if (counter == 200)
                    {
                        counter = 0;
                        await Task.Delay(2000);
                    }
                    else
                    {
                        counter += 1;
                    }
                }
            }

            sw.Stop();

            _logger.LogInformation("{SchoolName} summary is done within {TotalSeconds}",
                schoolName,
                Math.Round(sw.Elapsed.TotalSeconds, 2));

            logSch.EndDate = _machineDateTime.ServerTime;
            log.IsDone = true;
            log.EndDate = _machineDateTime.ServerTime;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
