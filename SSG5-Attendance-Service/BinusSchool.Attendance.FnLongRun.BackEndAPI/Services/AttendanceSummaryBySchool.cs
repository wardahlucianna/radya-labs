using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BinusSchool.Attendance.FnLongRun.Services
{
    public class AttendanceSummaryBySchool
    {
#if DEBUG
        private const string _containerPath = "attendance-summary-debug";
#else
        private const string _containerPath = "attendance-summary";
#endif

        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _machineDateTime;
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AttendanceSummaryBySchool> _logger;

        public AttendanceSummaryBySchool(IAttendanceDbContext dbContext,
            IMachineDateTime machineDateTime,
            IAttendanceSummaryService attendanceSummaryService,
            IConfiguration configuration,
            ILogger<AttendanceSummaryBySchool> logger)
        {
            _dbContext = dbContext;
            _machineDateTime = machineDateTime;
            _attendanceSummaryService = attendanceSummaryService;
            _configuration = configuration;
            _logger = logger;
        }

        public CloudStorageAccount GetCloudStorageAccount()
        {
            string s = string.Empty;
            s = _configuration["ConnectionStrings:Attendance:AccountStorage"];

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
            CreateContainerIfNotExists(_containerPath);
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerPath);

            //get all schools
            var school = await _dbContext.Entity<MsSchool>()
                .Where(e => e.Id == idSchool)
                .Select(e => new { IdSchool = e.Id, e.Name, e.Description })
                .FirstOrDefaultAsync(cancellationToken);
            if (school is null)
                throw new Exception("Data school is empty");

            _logger.LogInformation("Attendance summary cronjob for school {Name} has started..", school.Name);

            var isError = false;

            var log = new TrAttendanceSummaryLog
            {
                StartDate = _machineDateTime.ServerTime
            };
            await _dbContext.Entity<TrAttendanceSummaryLog>().AddAsync(log, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.DetachChanges();

            try
            {
                //local variables
                var mappingAttendances = await _dbContext.Entity<MsMappingAttendance>()
                    .Include(e => e.Level)
                    .ThenInclude(e => e.Formulas)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var mappingAttendanceWorkhabits = await _dbContext.Entity<MsMappingAttendanceWorkhabit>()
                    .Include(e => e.MappingAttendance)
                    .Include(e => e.Workhabit)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var mappingSchools = await _dbContext.Entity<MsSchoolMappingEA>()
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var mappingSchool = mappingSchools.FirstOrDefault(e => e.IdSchool == school.IdSchool);
                if (mappingSchool == null)
                    throw new Exception("Mapping school MsSchoolMappingEA is null");

                //local loop variables
                var sw = Stopwatch.StartNew();
                var listSummaryPerGrade = new List<Summary>();
                var i = 0;

                var logSch = new TrAttdSummaryLogSch
                {
                    IdAttendanceSummaryLog = log.Id,
                    IdSchool = school.IdSchool,
                    SchoolName = school.Description,
                    StartDate = _machineDateTime.ServerTime
                };

                _dbContext.Entity<TrAttdSummaryLogSch>().Add(logSch);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.DetachChanges();

                var idAcademicYear =
                    await _attendanceSummaryService.GetActiveAcademicYearAsync(school.IdSchool, cancellationToken);

                //added new
                await _attendanceSummaryService.DeleteTermByAcademicYearAsync(idAcademicYear, cancellationToken);

                var grades = await _attendanceSummaryService.GetGradesAsync(school.IdSchool,
                    idAcademicYear,
                    cancellationToken);

                _logger.LogInformation("Total grade {Total} within Id School {IdSchool}", grades.Count,
                    school.IdSchool);

                logSch.TotalGrade = grades.Count;

                var listStudent = new List<string>();

                var swAllGrade = Stopwatch.StartNew();

                foreach (var item in grades)
                {
                    var swPerGrade = Stopwatch.StartNew();

                    _logger.LogInformation("Get data {Grade}", item.GradeName);

                    //get list period by grade
                    var periods = await _attendanceSummaryService.GetPeriodsAsync(item.IdGrade, cancellationToken);

                    var students =
                        await _attendanceSummaryService.GetStudentsByGradeAsync(item.IdGrade, cancellationToken);
                    listStudent.AddRange(students);

                    var grd = new TrAttdSummaryLogSchGrd
                    {
                        IdAttdSummaryLogSch = logSch.Id,
                        IdGrade = item.IdGrade,
                        GradeName = item.GradeName,
                        TotalStudent = students.Count
                    };

                    foreach (var idStudent in students)
                        _dbContext.Entity<TrAttdSummaryLogSchGrdStu>().Add(new TrAttdSummaryLogSchGrdStu
                        {
                            IdAttdSummaryLogSchGrd = grd.Id,
                            IdStudent = idStudent
                        });

                    _dbContext.Entity<TrAttdSummaryLogSchGrd>().Add(grd);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _dbContext.DetachChanges();

                    //get data lesson per grade
                    var summaries =
                        await _attendanceSummaryService.GetSummaryPerGradeIncludeEntriesAsync(item.IdGrade,
                            school.IdSchool,
                            periods,
                            mappingAttendances,
                            mappingAttendanceWorkhabits,
                            cancellationToken);

                    listSummaryPerGrade.Clear();
                    listSummaryPerGrade.AddRange(summaries);
                    i += listSummaryPerGrade.Count;

                    _logger.LogInformation("Data {Grade}, total summary {Total}, takes {Seconds}s", item.GradeName,
                        summaries.Count, Math.Round(swPerGrade.Elapsed.TotalSeconds, 2));

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
                            Items = e.ToList()
                        })
                        .ToList();

                    foreach (var item2 in groupByStudent)
                    {
                        var filename =
                            $"{school.IdSchool}_{school.Description}_{item2.IdStudent}_{Guid.NewGuid()}.json";
                        var blob = container.GetBlockBlobReference(filename);
                        blob.Properties.ContentType = "application/json";

                        var s = JsonConvert.SerializeObject(item2,
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
                    }
                }

                _dbContext.AttachEntity(logSch);
                logSch.TotalStudent = listStudent.Count;
                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.DetachChanges();

                swAllGrade.Stop();

                _logger.LogInformation("Total summary {TotalSummary} takes {TotalSeconds}s",
                    i,
                    Math.Round(swAllGrade.Elapsed.TotalSeconds, 2));

                _dbContext.AttachEntity(logSch);
                logSch.EndDate = _machineDateTime.ServerTime;

                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.DetachChanges();

                sw.Stop();

                _logger.LogInformation("{SchoolName} summary is done within {TotalSeconds}",
                    school.Description,
                    Math.Round(sw.Elapsed.TotalSeconds, 2));

                _dbContext.AttachEntity(log);
                log.IsDone = true;
            }
            catch (Exception ex)
            {
                isError = true;
                _logger.LogError(ex, "Error has occured");

                _dbContext.AttachEntity(log);
                log.IsError = true;
                log.ErrorMessage = ex.Message;
                log.StackTrace = ex.StackTrace;
            }
            finally
            {
                log.EndDate = _machineDateTime.ServerTime;

                if (isError)
                    _logger.LogInformation("Attendance summary cronjob has stopped..");
                else
                    _logger.LogInformation("Attendance summary cronjob has done..");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.DetachChanges();
        }
    }
}
