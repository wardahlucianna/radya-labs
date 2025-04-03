using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NPOI.XWPF.UserModel;

namespace BinusSchool.Attendance.FnAttendanceSummary.Services
{
    public class ReRunAttendanceSummary
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReRunAttendanceSummary> _logger;

#if DEBUG
        private const string _blobPath = "attendance-summary-debug/{name}.json";
        private const string _containerPath = "attendance-summary-debug";
        private const string _backupContainerPath = "attendance-summary-source-debug";
#else
        private const string _blobPath = "attendance-summary/{name}.json";
        private const string _containerPath = "attendance-summary";
        private const string _backupContainerPath = "attendance-summary-source";
#endif

        public ReRunAttendanceSummary(IAttendanceDbContext dbContext,
        IConfiguration configuration,
        ILogger<ReRunAttendanceSummary> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
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
            var blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExistsAsync();
        }

        public async Task RunAsync(List<SummaryDto> listbody, CancellationToken cancellationToken)
        {
            try
            {
                await ExecuteAsync(listbody, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurs");
            }
        }
        public async Task ExecuteAsync(List<SummaryDto> listbody, CancellationToken cancellationToken)
        {

            var sw = Stopwatch.StartNew();

            CreateContainerIfNotExists(_backupContainerPath);
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerPath);
            var backupContainer = blobClient.GetContainerReference(_backupContainerPath);

            //local variables
            var masterAttendances = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                .Include(e => e.Attendance)
                .Include(e => e.MappingAttendance)
                .Where(e => e.Attendance.IdAcademicYear == listbody.First().IdAcademicYear)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var mappingAttendances = await _dbContext.Entity<MsMappingAttendance>()
                .Include(e => e.Level)
                .ThenInclude(e => e.Formulas)
                .Where(e => e.Level.IdAcademicYear == listbody.First().IdAcademicYear)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var mappingAttendanceWorkhabits = await _dbContext.Entity<MsMappingAttendanceWorkhabit>()
                .Include(e => e.MappingAttendance)
                .Include(e => e.Workhabit)
                .Where(e => e.MappingAttendance.Level.IdAcademicYear == listbody.First().IdAcademicYear)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var body in listbody)
            {
                var fileName = $"{body.IdSchool}-{body.IdStudent}-{body.Term}.json";
                try
                {
                    var backupBlob = backupContainer.GetBlockBlobReference(fileName);
                    backupBlob.Properties.ContentType = "application/json";
                    var s = JsonConvert.SerializeObject(body,
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
                    await backupBlob.UploadFromStreamAsync(ms);

                    _logger.LogInformation("Successfully copy file to {Path}", _backupContainerPath);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error occurs when copying file");
                }
                var blob = container.GetBlockBlobReference(body.JsonName);
                try
                {
                    var summaryTerms = await _dbContext.Entity<TrAttendanceSummaryTerm>()
                        .Where(e =>
                            e.IdStudent == body.IdStudent &&
                            e.IdPeriod == body.IdPeriod &&
                            e.IdAcademicYear == body.IdAcademicYear &&
                            e.IdSchool == body.IdSchool &&
                            e.IdLevel == body.IdLevel &&
                            e.IdGrade == body.IdGrade &&
                            e.IdHomeroom == body.IdHomeroom)
                        .ToListAsync(cancellationToken);

                    var mappingAttendance = mappingAttendances.FirstOrDefault(e => e.IdLevel == body!.IdLevel);
                    if (mappingAttendance is null)
                    {
                        await blob.DeleteIfExistsAsync();

                        sw.Stop();

                        _logger.LogWarning(
                            "Update mapping student {Id}, done within {Total}s, because mapping attendance with IdLevel {IdLevel} is null",
                            body.IdStudent,
                            Math.Round(sw.Elapsed.TotalSeconds, 2),
                            body!.IdLevel);
                        return;
                    }

                    _logger.LogInformation("Update student blob trigger runs of {FileName}", body.JsonName);

                    var getAttendanceByDate = body.Items
                                  .GroupBy(e => new
                                  {
                                      e.ScheduleDt
                                  })
                                  .ToList();

                    #region Default total day

                    //get default total day
                    var defaultTotalDay = summaryTerms.FirstOrDefault(e =>
                        e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default
                        && e.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalDayName);
                    var defaultTotalDayIsNew = false;
                    if (defaultTotalDay == null)
                    {
                        defaultTotalDayIsNew = true;
                        defaultTotalDay = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = body.IdStudent,
                            IdPeriod = body.IdPeriod,
                            IdAcademicYear = body.IdAcademicYear,
                            IdSchool = body.IdSchool,
                            IdLevel = body.IdLevel,
                            IdGrade = body.IdGrade,
                            IdHomeroom = body.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.Default,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultTotalDayName,
                            IdAttendanceWorkhabit = null,
                            Semester = body.Semester,
                            Term = body.Term
                        };
                    }

                    defaultTotalDay.SourceFileName = fileName;
                    defaultTotalDay.Total = body.Items.GroupBy(e => e.ScheduleDt).Count();

                    if (defaultTotalDayIsNew)
                        _dbContext.Entity<TrAttendanceSummaryTerm>().Add(defaultTotalDay);

                    #endregion

                    #region Default total session

                    //get default total session
                    var defaultTotalSession = summaryTerms.FirstOrDefault(e =>
                        e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default
                        && e.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalSessionName);
                    var defaultTotalSessionIsNew = false;
                    if (defaultTotalSession == null)
                    {
                        defaultTotalSessionIsNew = true;
                        defaultTotalSession = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = body.IdStudent,
                            IdPeriod = body.IdPeriod,
                            IdAcademicYear = body.IdAcademicYear,
                            IdSchool = body.IdSchool,
                            IdLevel = body.IdLevel,
                            IdGrade = body.IdGrade,
                            IdHomeroom = body.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.Default,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultTotalSessionName,
                            IdAttendanceWorkhabit = null,
                            Semester = body.Semester,
                            Term = body.Term
                        };
                    }

                    defaultTotalSession.SourceFileName = fileName;
                    defaultTotalSession.Total =
                        body.Items.GroupBy(x => new { x.IdSession, x.IdSubject, x.IdHomeroom, x.ScheduleDt }).Count();

                    if (defaultTotalSessionIsNew)
                        _dbContext.Entity<TrAttendanceSummaryTerm>().Add(defaultTotalSession);

                    #endregion

                    #region Default rate absence

                    //get default rate absence
                    //var defaultRateAbsence = summaryTerms.FirstOrDefault(e =>
                    //    e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default
                    //    && e.AttendanceWorkhabitName == SummaryTermConstant.DefaultRateAbsenceName);
                    //var defaultRateAbsenceIsNew = false;
                    //if (defaultRateAbsence == null)
                    //{
                    //    defaultRateAbsenceIsNew = true;
                    //    defaultRateAbsence = new TrAttendanceSummaryTerm
                    //    {
                    //        Id = Guid.NewGuid().ToString(),
                    //        IdStudent = item.IdStudent,
                    //        IdPeriod = item.IdPeriod,
                    //        IdAcademicYear = item.IdAcademicYear,
                    //        IdSchool = item.IdSchool,
                    //        IdLevel = item.IdLevel,
                    //        IdGrade = item.IdGrade,
                    //        IdHomeroom = item.IdHomeroom,
                    //        AttendanceWorkhabitType = TrAttendanceSummaryTermType.Default,
                    //        AttendanceWorkhabitName = SummaryTermConstant.DefaultRateAbsenceName,
                    //        IdAttendanceWorkhabit = null,
                    //        Semester = item.Semester,
                    //        Term = item.Term
                    //    };
                    //}

                    ////waiting
                    //defaultRateAbsence.Total = 0;

                    //if (defaultRateAbsenceIsNew)
                    //    _dbContext.Entity<TrAttendanceSummaryTerm>().Add(defaultRateAbsence);

                    #endregion

                    #region Default rate presence

                    //get default rate presence
                    //var defaultRatePresence = summaryTerms.FirstOrDefault(e =>
                    //    e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default
                    //    && e.AttendanceWorkhabitName == SummaryTermConstant.DefaultRatePresenceName);
                    //var defaultRatePresenceIsNew = false;
                    //if (defaultRatePresence == null)
                    //{
                    //    defaultRatePresenceIsNew = true;
                    //    defaultRatePresence = new TrAttendanceSummaryTerm
                    //    {
                    //        Id = Guid.NewGuid().ToString(),
                    //        IdStudent = item.IdStudent,
                    //        IdPeriod = item.IdPeriod,
                    //        IdAcademicYear = item.IdAcademicYear,
                    //        IdSchool = item.IdSchool,
                    //        IdLevel = item.IdLevel,
                    //        IdGrade = item.IdGrade,
                    //        IdHomeroom = item.IdHomeroom,
                    //        AttendanceWorkhabitType = TrAttendanceSummaryTermType.Default,
                    //        AttendanceWorkhabitName = SummaryTermConstant.DefaultRatePresenceName,
                    //        IdAttendanceWorkhabit = null,
                    //        Semester = item.Semester,
                    //        Term = item.Term
                    //    };
                    //}

                    ////waiting
                    //defaultRatePresence.Total = 0;

                    //if (defaultRatePresenceIsNew)
                    //    _dbContext.Entity<TrAttendanceSummaryTerm>().Add(defaultRatePresence);

                    #endregion

                    #region Attendance status

                    //attendance status
                    var values = Enum.GetValues(typeof(AttendanceEntryStatus)).Cast<AttendanceEntryStatus>();
                    var totalStatus = 0;
                    foreach (var entryStatus in values)
                    {
                        var summaryTermAttendanceStatus = summaryTerms.FirstOrDefault(e =>
                            e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceStatus
                            && e.AttendanceWorkhabitName == entryStatus.ToString());
                        var summaryTermAttendanceStatusIsNew = false;
                        if (summaryTermAttendanceStatus == null)
                        {
                            summaryTermAttendanceStatusIsNew = true;
                            summaryTermAttendanceStatus = new TrAttendanceSummaryTerm
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdStudent = body.IdStudent,
                                IdPeriod = body.IdPeriod,
                                IdAcademicYear = body.IdAcademicYear,
                                IdSchool = body.IdSchool,
                                IdLevel = body.IdLevel,
                                IdGrade = body.IdGrade,
                                IdHomeroom = body.IdHomeroom,
                                AttendanceWorkhabitType = TrAttendanceSummaryTermType.AttendanceStatus,
                                AttendanceWorkhabitName = entryStatus.ToString(),
                                IdAttendanceWorkhabit = null,
                                Semester = body.Semester,
                                Term = body.Term
                            };
                        }

                        summaryTermAttendanceStatus.SourceFileName = fileName;

                        if (entryStatus != AttendanceEntryStatus.Unsubmitted)
                        {
                            summaryTermAttendanceStatus.Total = mappingAttendance.AbsentTerms == AbsentTerm.Day
                                ? body.Items.Where(e => e.EntryStatus == entryStatus)
                                    .GroupBy(e => e.ScheduleDt).Count()
                                : body.Items.Where(e => e.EntryStatus == entryStatus)
                                    .GroupBy(e => new { e.ScheduleDt, e.IdSession, e.IdSubject, e.IdHomeroom }).Count();
                            totalStatus += summaryTermAttendanceStatus.Total;
                        }
                        else
                        {
                            var totalUnsubmitted = mappingAttendance.AbsentTerms == AbsentTerm.Day
                                ? defaultTotalDay.Total - totalStatus
                                : defaultTotalSession.Total - totalStatus;
                            summaryTermAttendanceStatus.Total = totalUnsubmitted;
                        }


                        if (summaryTermAttendanceStatusIsNew)
                            _dbContext.Entity<TrAttendanceSummaryTerm>().Add(summaryTermAttendanceStatus);
                    }

                    #endregion

                    #region Attendance
                    //attendance
                    var listAttendances = masterAttendances.Where(e =>
                            e.Attendance.IdAcademicYear == body.IdAcademicYear &&
                            e.MappingAttendance.IdLevel == body.IdLevel)
                        .ToList();

                    foreach (var attendanceMappingAttendance in listAttendances)
                    {
                        #region total in days
                        var totalInDays = 0;
                        if (attendanceMappingAttendance.Attendance.Code != "PR" && attendanceMappingAttendance.Attendance.Code != "LT")
                        {
                            foreach (var item in getAttendanceByDate)
                            {
                                var countSession = item.Count();
                                var getDataUaEa = item.Where(e => e.IdAttendanceMappingAttendance == attendanceMappingAttendance.Id).ToList();

                                if (countSession == getDataUaEa.Count())
                                {
                                    var groupByUaEa = getDataUaEa
                                        .GroupBy(e => new
                                        {
                                            e.IdAttendanceMappingAttendance
                                        }).Count();

                                    if (groupByUaEa == 1)
                                        totalInDays += 1;
                                }
                            }
                        }
                        #endregion


                        var summaryTermAttendance = summaryTerms.FirstOrDefault(e =>
                            e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance
                            && e.IdAttendanceWorkhabit == attendanceMappingAttendance.Attendance.Id);
                        var summaryTermAttendanceIsNew = false;
                        if (summaryTermAttendance == null)
                        {
                            summaryTermAttendanceIsNew = true;
                            summaryTermAttendance = new TrAttendanceSummaryTerm
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdStudent = body.IdStudent,
                                IdPeriod = body.IdPeriod,
                                IdAcademicYear = body.IdAcademicYear,
                                IdSchool = body.IdSchool,
                                IdLevel = body.IdLevel,
                                IdGrade = body.IdGrade,
                                IdHomeroom = body.IdHomeroom,
                                AttendanceWorkhabitType = TrAttendanceSummaryTermType.Attendance,
                                IdAttendanceWorkhabit = null,
                                Semester = body.Semester,
                                Term = body.Term
                            };
                        }

                        summaryTermAttendance.TotalInDays = totalInDays;
                        summaryTermAttendance.SourceFileName = fileName;
                        summaryTermAttendance.AttendanceWorkhabitName =
                            attendanceMappingAttendance.Attendance.Description;
                        summaryTermAttendance.IdAttendanceWorkhabit = attendanceMappingAttendance.Attendance.Id;

                        //waiting
                        summaryTermAttendance.Total = 0;

                        if (mappingAttendance?.Level == null)
                            summaryTermAttendance.Message =
                                "Calculation skipped cause ms mapping by level not found";

                        if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                            summaryTermAttendance.Total = body.Items
                                .Where(e => e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                            e.IdAttendanceMappingAttendance == attendanceMappingAttendance.Id)
                                .GroupBy(e => e.ScheduleDt).Count();
                        else
                            summaryTermAttendance.Total = body.Items.Where(e =>
                                    e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                    e.IdAttendanceMappingAttendance == attendanceMappingAttendance.Id)
                                .GroupBy(e => new { e.ScheduleDt, e.IdSession ,e.IdSubject, e.IdHomeroom }).Count();

                        if (summaryTermAttendanceIsNew)
                            _dbContext.Entity<TrAttendanceSummaryTerm>().Add(summaryTermAttendance);
                    }

                    //attendance category excused
                    var summaryTermAttendanceExc = summaryTerms.FirstOrDefault(e =>
                        e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceCategory
                        && e.AttendanceWorkhabitName == AbsenceCategory.Excused.ToString());
                    var summaryTermAttendanceIsExcNew = false;
                    if (summaryTermAttendanceExc == null)
                    {
                        summaryTermAttendanceIsExcNew = true;
                        summaryTermAttendanceExc = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = body.IdStudent,
                            IdPeriod = body.IdPeriod,
                            IdAcademicYear = body.IdAcademicYear,
                            IdSchool = body.IdSchool,
                            IdLevel = body.IdLevel,
                            IdGrade = body.IdGrade,
                            IdHomeroom = body.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.AttendanceCategory,
                            IdAttendanceWorkhabit = null,
                            Semester = body.Semester,
                            Term = body.Term
                        };
                    }

                    summaryTermAttendanceExc.SourceFileName = fileName;
                    summaryTermAttendanceExc.AttendanceWorkhabitName = AbsenceCategory.Excused.ToString();
                    summaryTermAttendanceExc.IdAttendanceWorkhabit = null;
                    summaryTermAttendanceExc.Total = 0;

                    var getExcused = listAttendances
                        .Where(e => e.Attendance.AbsenceCategory == AbsenceCategory.Excused).ToList();

                    if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                        summaryTermAttendanceExc.Total += body.Items
                            .Where(e => e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                        getExcused.Select(x => x.Id).ToList()
                                            .Contains(e.IdAttendanceMappingAttendance))
                            .GroupBy(e => e.ScheduleDt).Count();
                    else
                        summaryTermAttendanceExc.Total += body.Items.Where(e =>
                                e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                getExcused.Select(x => x.Id).ToList().Contains(e.IdAttendanceMappingAttendance))
                            .GroupBy(e => new { e.ScheduleDt, e.IdSession, e.IdSubject, e.IdHomeroom }).Count();

                    if (mappingAttendance?.Level == null)
                        summaryTermAttendanceExc.Message =
                            "Calculation skipped cause ms mapping by level not found";

                    if (summaryTermAttendanceIsExcNew)
                        _dbContext.Entity<TrAttendanceSummaryTerm>().Add(summaryTermAttendanceExc);


                    //attendance category unexcused
                    var summaryTermAttendanceUnexcused = summaryTerms.FirstOrDefault(e =>
                        e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceCategory
                        && e.AttendanceWorkhabitName == AbsenceCategory.Unexcused.ToString());
                    var summaryTermAttendanceUnexcusedIsNew = false;
                    if (summaryTermAttendanceUnexcused == null)
                    {
                        summaryTermAttendanceUnexcusedIsNew = true;
                        summaryTermAttendanceUnexcused = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = body.IdStudent,
                            IdPeriod = body.IdPeriod,
                            IdAcademicYear = body.IdAcademicYear,
                            IdSchool = body.IdSchool,
                            IdLevel = body.IdLevel,
                            IdGrade = body.IdGrade,
                            IdHomeroom = body.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.AttendanceCategory,
                            IdAttendanceWorkhabit = null,
                            Semester = body.Semester,
                            Term = body.Term
                        };
                    }

                    summaryTermAttendanceUnexcused.SourceFileName = fileName;
                    summaryTermAttendanceUnexcused.AttendanceWorkhabitName = AbsenceCategory.Unexcused.ToString();
                    summaryTermAttendanceUnexcused.IdAttendanceWorkhabit = null;
                    summaryTermAttendanceUnexcused.Total = 0;

                    var getUnexcused = listAttendances
                        .Where(e => e.Attendance.AbsenceCategory == AbsenceCategory.Unexcused).ToList();

                    if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                        summaryTermAttendanceUnexcused.Total += body.Items
                            .Where(e => e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                        getUnexcused.Select(x => x.Id).ToList()
                                            .Contains(e.IdAttendanceMappingAttendance))
                            .GroupBy(e => e.ScheduleDt).Count();
                    else
                        summaryTermAttendanceUnexcused.Total += body.Items.Where(e =>
                                e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                getUnexcused.Select(x => x.Id).ToList().Contains(e.IdAttendanceMappingAttendance))
                            .GroupBy(e => new { e.ScheduleDt, e.IdSession, e.IdSubject, e.IdHomeroom }).Count();

                    if (mappingAttendance?.Level == null)
                        summaryTermAttendanceUnexcused.Message =
                            "Calculation skipped cause ms mapping by level not found";

                    if (summaryTermAttendanceUnexcusedIsNew)
                        _dbContext.Entity<TrAttendanceSummaryTerm>().Add(summaryTermAttendanceUnexcused);

                    //excused absence category assign by school
                    var summaryTermAttendanceSch = summaryTerms.FirstOrDefault(e =>
                        e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.ExcusedAbsenceCategory
                        && e.AttendanceWorkhabitName == SummaryTermConstant.DefaultAssignBySchoolName);
                    var summaryTermAttendanceIsScNew = false;
                    if (summaryTermAttendanceSch == null)
                    {
                        summaryTermAttendanceIsScNew = true;
                        summaryTermAttendanceSch = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = body.IdStudent,
                            IdPeriod = body.IdPeriod,
                            IdAcademicYear = body.IdAcademicYear,
                            IdSchool = body.IdSchool,
                            IdLevel = body.IdLevel,
                            IdGrade = body.IdGrade,
                            IdHomeroom = body.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.ExcusedAbsenceCategory,
                            IdAttendanceWorkhabit = null,
                            Semester = body.Semester,
                            Term = body.Term
                        };
                    }

                    summaryTermAttendanceSch.SourceFileName = fileName;
                    summaryTermAttendanceSch.AttendanceWorkhabitName =
                        SummaryTermConstant.DefaultAssignBySchoolName;
                    summaryTermAttendanceSch.IdAttendanceWorkhabit = null;
                    summaryTermAttendanceSch.Total = 0;

                    var getAssignBySchool = listAttendances.Where(e =>
                        e.Attendance.ExcusedAbsenceCategory == ExcusedAbsenceCategory.AssignBySchool).ToList();

                    if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                        summaryTermAttendanceSch.Total += body.Items
                            .Where(e => e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                        getAssignBySchool.Select(x => x.Id).ToList()
                                            .Contains(e.IdAttendanceMappingAttendance))
                            .GroupBy(e => e.ScheduleDt).Count();
                    else
                        summaryTermAttendanceSch.Total += body.Items.Where(e =>
                                e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                getAssignBySchool.Select(x => x.Id).ToList()
                                    .Contains(e.IdAttendanceMappingAttendance))
                            .GroupBy(e => new { e.ScheduleDt, e.IdSession, e.IdSubject, e.IdHomeroom }).Count();

                    if (mappingAttendance?.Level == null)
                        summaryTermAttendanceSch.Message =
                            "Calculation skipped cause ms mapping by level not found";

                    if (summaryTermAttendanceIsScNew)
                        _dbContext.Entity<TrAttendanceSummaryTerm>().Add(summaryTermAttendanceSch);

                    //excused absence category personal
                    var summaryTermAttendancePersonal = summaryTerms.FirstOrDefault(e =>
                        e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.ExcusedAbsenceCategory
                        && e.AttendanceWorkhabitName == SummaryTermConstant.DefaultPersonalName);
                    var summaryTermAttendanceIsPersonal = false;
                    if (summaryTermAttendancePersonal == null)
                    {
                        summaryTermAttendanceIsPersonal = true;
                        summaryTermAttendancePersonal = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = body.IdStudent,
                            IdPeriod = body.IdPeriod,
                            IdAcademicYear = body.IdAcademicYear,
                            IdSchool = body.IdSchool,
                            IdLevel = body.IdLevel,
                            IdGrade = body.IdGrade,
                            IdHomeroom = body.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.ExcusedAbsenceCategory,
                            IdAttendanceWorkhabit = null,
                            Semester = body.Semester,
                            Term = body.Term
                        };
                    }

                    summaryTermAttendancePersonal.SourceFileName = fileName;
                    summaryTermAttendancePersonal.AttendanceWorkhabitName = SummaryTermConstant.DefaultPersonalName;
                    summaryTermAttendancePersonal.IdAttendanceWorkhabit = null;
                    summaryTermAttendancePersonal.Total = 0;

                    var getPersonal = listAttendances.Where(e =>
                        e.Attendance.ExcusedAbsenceCategory == ExcusedAbsenceCategory.Personal).ToList();
                    if (!getPersonal.Any())
                        summaryTermAttendancePersonal.Message = "Master data list attendance personal is null";

                    if (getPersonal.Any())
                    {
                        if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                            summaryTermAttendancePersonal.Total += body.Items
                                .Where(e => e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                            getPersonal.Select(x => x.Id).ToList()
                                                .Contains(e.IdAttendanceMappingAttendance))
                                .GroupBy(e => e.ScheduleDt).Count();
                        else
                            summaryTermAttendancePersonal.Total += body.Items.Where(e =>
                                    e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                    getPersonal.Select(x => x.Id).ToList()
                                        .Contains(e.IdAttendanceMappingAttendance))
                                .GroupBy(e => new { e.ScheduleDt, e.IdSession, e.IdSubject, e.IdHomeroom }).Count();
                    }

                    if (summaryTermAttendanceIsPersonal)
                        _dbContext.Entity<TrAttendanceSummaryTerm>().Add(summaryTermAttendancePersonal);

                    #endregion

                    #region Work Habit

                    //workhabit
                    var listWorkhabit = mappingAttendanceWorkhabits
                        .Where(e => e.MappingAttendance.IdLevel == body.IdLevel)
                        .ToList();

                    foreach (var workhabit in listWorkhabit)
                    {
                        var summaryTermWorkhabit = summaryTerms.FirstOrDefault(e =>
                            e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Workhabit
                            && e.IdAttendanceWorkhabit == workhabit.Id);
                        var summaryTermWorkhabitIsNew = false;
                        if (summaryTermWorkhabit == null)
                        {
                            summaryTermWorkhabitIsNew = true;
                            summaryTermWorkhabit = new TrAttendanceSummaryTerm
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdStudent = body.IdStudent,
                                IdPeriod = body.IdPeriod,
                                IdAcademicYear = body.IdAcademicYear,
                                IdSchool = body.IdSchool,
                                IdLevel = body.IdLevel,
                                IdGrade = body.IdGrade,
                                IdHomeroom = body.IdHomeroom,
                                AttendanceWorkhabitType = TrAttendanceSummaryTermType.Workhabit,
                                IdAttendanceWorkhabit = null,
                                Semester = body.Semester,
                                Term = body.Term
                            };
                        }

                        summaryTermWorkhabit.SourceFileName = fileName;
                        summaryTermWorkhabit.AttendanceWorkhabitName = workhabit.Workhabit.Description;
                        summaryTermWorkhabit.IdAttendanceWorkhabit = workhabit.Id;
                        summaryTermWorkhabit.Total = 0;

                        if (mappingAttendance?.Level == null)
                            summaryTermWorkhabit.Message =
                                "Calculation skipped cause ms mapping by level not found";

                        if (!mappingAttendance.Level.Formulas.Any(e => e.IsActive))
                            summaryTermWorkhabit.Message =
                                "Calculation skipped cause ms mapping by level, formula is not found";

                        //calculate
                        if (mappingAttendance != null && mappingAttendance.Level.Formulas.Any(e => e.IsActive))
                        {
                            if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                                summaryTermWorkhabit.Total = body.Items
                                    .Where(e => e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                                e.Workhabits.Select(x => x.IdMappingAttendanceWorkHabit).ToList()
                                                    .Contains(workhabit.Id))
                                    .GroupBy(e => e.ScheduleDt)
                                    .Count();

                            else
                                summaryTermWorkhabit.Total = body.Items
                                    .Where(e => e.EntryStatus == AttendanceEntryStatus.Submitted &&
                                                e.Workhabits.Select(x => x.IdMappingAttendanceWorkHabit).ToList()
                                                    .Contains(workhabit.Id))
                                    .GroupBy(e => new { e.ScheduleDt, e.IdSession, e.IdSubject, e.IdHomeroom })
                                    .Count();
                        }

                        if (summaryTermWorkhabitIsNew)
                            _dbContext.Entity<TrAttendanceSummaryTerm>().Add(summaryTermWorkhabit);
                    }

                    #endregion

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    await blob.DeleteIfExistsAsync();

                    _logger.LogInformation("Update mapping student {Id}, done within {Total}s",
                    body.IdStudent,
                    Math.Round(sw.Elapsed.TotalSeconds, 2));
                }
                catch (Exception)
                {
                    await blob.DeleteIfExistsAsync();
                }
            }
        }
    }
}
