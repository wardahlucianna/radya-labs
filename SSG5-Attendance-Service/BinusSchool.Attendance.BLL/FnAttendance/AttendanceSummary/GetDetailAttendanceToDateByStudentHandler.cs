using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailAttendanceToDateByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly GetMapAttendanceDetailHandler _mapAttendanceHandler;

        public GetDetailAttendanceToDateByStudentHandler(IAttendanceDbContext dbContext, GetMapAttendanceDetailHandler mapAttendanceHandler)
        {
            _dbContext = dbContext;
            _mapAttendanceHandler = mapAttendanceHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailAttendanceToDateByStudentRequest>(
                nameof(GetDetailAttendanceToDateByStudentRequest.IdStudent),
                nameof(GetDetailAttendanceToDateByStudentRequest.IdLevel)
                );

            var mapAttendance = await _mapAttendanceHandler.GetMapAttendanceDetail(param.IdLevel, CancellationToken);
            if (mapAttendance is null)
                throw new BadRequestException($"Mapping attendance for level {param.IdLevel} is not available.");

            var eaMapping = await _dbContext.Entity<MsSchoolMappingEA>()
                                            .Include(x => x.School).ThenInclude(x => x.AcademicYears).ThenInclude(x => x.Levels)
                                            .Where(x => x.School.AcademicYears.Any(y => y.Levels.Any(z => z.Id == param.IdLevel)))
                                            .FirstOrDefaultAsync(CancellationToken);
            if (eaMapping is null)
                throw new NotFoundException("EA mapping is not found for this school");

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(
                x => x.GeneratedScheduleStudent.IdStudent == param.IdStudent
                     && x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.Attendance.AbsenceCategory.HasValue && y.Status == AttendanceEntryStatus.Submitted));
            if (param.StartDate.HasValue && param.EndDate.HasValue)
            {
                predicate = predicate.And(x =>
                EF.Functions.DateDiffDay(x.ScheduleDate, param.StartDate.Value) <= 0
                    && EF.Functions.DateDiffDay(x.ScheduleDate, param.EndDate.Value) >= 0);
            }
            else if (param.Semester.HasValue)
            {
                predicate = predicate.And(x => x.Homeroom.Semester == param.Semester.Value);
            }

            var query = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.Homeroom)
                .Include(x => x.GeneratedScheduleStudent)
                .Include(x => x.AttendanceEntries)
                    .ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                .Where(predicate)
                .OrderBy(x => x.ScheduleDate)
                .ToListAsync(CancellationToken);

            var idHomeroom = query
                .GroupBy(x => (x.GeneratedScheduleStudent.IdStudent, x.ScheduleDate, x.IdHomeroom))
                .Select(x => x.Key.IdHomeroom)
                .FirstOrDefault();

            var homeroom = await _dbContext.Entity<MsHomeroomTeacher>()
                .Include(x => x.Homeroom)
                .Include(x => x.Staff)
                .Where(x => x.IdHomeroom == idHomeroom && x.IsAttendance)
                .Select(x => new NameValueVm
                {
                    Id = x.Staff.IdBinusian,
                    Name = x.Staff.FirstName
                }).FirstOrDefaultAsync(CancellationToken);


            var details = mapAttendance.Term == AbsentTerm.Day
                ? query
                    .GroupBy(x => (x.GeneratedScheduleStudent.IdStudent, x.ScheduleDate))
                    .Select(x => CreateAttendanceDetail(x.First()))
                : query
                    .Select(x => CreateAttendanceDetail(x));
            var _details = details.ToList();
            var summaryExcuse = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>();
            if (AbsentTerm.Day == mapAttendance.Term)
            {
                _details.ForEach(x => x.Teacher = homeroom);
            }
            if (eaMapping.IsGrouped)
            {
                if (details.Any(x => x.Attendance.AbsenceCategory.HasValue
                                     && x.Attendance.AbsenceCategory.Value == AbsenceCategory.Excused
                                     && !x.Attendance.ExcusedAbsenceCategory.HasValue))
                    summaryExcuse = _details
                        .Where(x => x.Attendance.AbsenceCategory.HasValue
                                    && x.Attendance.AbsenceCategory.Value == AbsenceCategory.Excused
                                    && !x.Attendance.ExcusedAbsenceCategory.HasValue)
                        .GroupBy(x => x.Attendance.ExcusedAbsenceCategory)
                        .ToDictionary(x => x.Key?.ToString() ?? "ExcusedAbsence", x => x
                            .GroupBy(y => y.Attendance.Attendance.Description)
                            .ToDictionary(y => y.Key, y => y.Count()));

                var availAttendances = await _dbContext.Entity<MsAttendance>()
                    .Where(x
                        => x.AttendanceMappingAttendances.Any(y => y.MappingAttendance.IdLevel == mapAttendance.Level.Id && y.MappingAttendance.AbsentTerms == mapAttendance.Term)
                        && x.AbsenceCategory.HasValue
                        && x.AbsenceCategory.Value == AbsenceCategory.Excused
                        && !x.ExcusedAbsenceCategory.HasValue)
                    .ToListAsync(CancellationToken);

                if (availAttendances.Any())
                {
                    var groupedExcused = availAttendances
                        .GroupBy(x => x.ExcusedAbsenceCategory)
                        .ToDictionary(x => x.Key?.ToString() ?? "ExcusedAbsence", x => x
                            .GroupBy(y => y.Description)
                            .ToDictionary(y => y.Key, _ => 0));
                    foreach (var item in groupedExcused)
                    {
                        if (summaryExcuse.ContainsKey(item.Key))
                        {
                            foreach (var data in item.Value)
                            {

                                summaryExcuse[item.Key].TryAdd(data.Key, data.Value);
                            }
                        }
                        else
                            summaryExcuse.Add(item.Key, item.Value);
                    }
                }
            }
            else
            {
                if (details.Any(x => x.Attendance.AbsenceCategory.HasValue
                                     && x.Attendance.AbsenceCategory.Value == AbsenceCategory.Excused
                                     && x.Attendance.ExcusedAbsenceCategory.HasValue))
                    summaryExcuse = _details
                    .Where(x => x.Attendance.AbsenceCategory.HasValue
                                && x.Attendance.AbsenceCategory.Value == AbsenceCategory.Excused
                                && x.Attendance.ExcusedAbsenceCategory.HasValue)
                    .GroupBy(x => x.Attendance.ExcusedAbsenceCategory.Value)
                    .ToDictionary(x => x.Key.ToString(), x => x
                        .GroupBy(y => y.Attendance.Attendance.Description)
                        .ToDictionary(y => y.Key, y => y.Count()));

                var availAttendances = await _dbContext.Entity<MsAttendance>()
                    .Where(x
                        => x.AttendanceMappingAttendances.Any(y => y.MappingAttendance.IdLevel == mapAttendance.Level.Id && y.MappingAttendance.AbsentTerms == mapAttendance.Term)
                        && x.AbsenceCategory.HasValue
                        && x.AbsenceCategory.Value == AbsenceCategory.Excused
                        && x.ExcusedAbsenceCategory.HasValue)
                    .ToListAsync(CancellationToken);

                if (availAttendances.Any())
                {
                    var groupedExcused = availAttendances
                        .GroupBy(x => x.ExcusedAbsenceCategory.Value)
                        .ToDictionary(x => x.Key.ToString(), x => x
                            .GroupBy(y => y.Description)
                            .ToDictionary(y => y.Key, _ => 0));
                    foreach (var item in groupedExcused)
                    {
                        if (summaryExcuse.ContainsKey(item.Key))
                        {
                            foreach (var data in item.Value)
                            {

                                summaryExcuse[item.Key].TryAdd(data.Key, data.Value);
                            }
                        }
                        else
                            summaryExcuse.Add(item.Key, item.Value);
                    }
                }
            }

            var student = await _dbContext.Entity<MsStudent>().FindAsync(new[] { param.IdStudent }, CancellationToken);
            var result = new GetDetailAttendanceToDateByStudentResult
            {
                Id = param.IdStudent,
                Name = NameUtil.GenerateFullName(student.FirstName, student.MiddleName, student.LastName),
                Details = _details,
                IsEAGrouped = eaMapping.IsGrouped,
                Summary = new AttendanceToDateSummary
                {
                    ExcusedAbsence = summaryExcuse.Count != 0 ? summaryExcuse : null,
                    //UnexcusedAbsence = mapAttendance.IsUseDueToLateness
                    //    ? details.Count(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused) + Math.Abs(details.Count(x => x.Attendance.Code == "LT") / 4)
                    //    : details.Count(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused)
                    UnexcusedAbsence = details.Count(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused)
                }
            };

            return Request.CreateApiResult2(result as object);
        }

        private AttendanceToDateDetail CreateAttendanceDetail(TrGeneratedScheduleLesson scheduleLesson)
        {
            return new AttendanceToDateDetail
            {
                IdGeneratedScheduleLesson = scheduleLesson.Id,
                Date = scheduleLesson.ScheduleDate,
                Session = new NameValueVm
                {
                    Id = scheduleLesson.IdSession,
                    Name = scheduleLesson.SessionID
                },
                Subject = new NameValueVm
                {
                    Id = scheduleLesson.IdSubject,
                    Name = scheduleLesson.SubjectName
                },
                Teacher = new NameValueVm
                {
                    Id = scheduleLesson.IdUser,
                    Name = scheduleLesson.TeacherName
                },
                Attendance = CreateAttendance(
                        scheduleLesson.AttendanceEntries.First().IdAttendanceMappingAttendance,
                        scheduleLesson.AttendanceEntries.First().AttendanceMappingAttendance.Attendance),
                Reason = scheduleLesson.AttendanceEntries.First().Notes
            };
        }

        private AttendanceToDateAttendance CreateAttendance(string idAttendanceMapAttendance, MsAttendance attendance)
        {
            return new AttendanceToDateAttendance
            {
                Id = attendance.Id,
                IdAttendanceMapAttendance = idAttendanceMapAttendance,
                Code = attendance.Code,
                Description = attendance.Description,
                AbsenceCategory = attendance.AbsenceCategory,
                ExcusedAbsenceCategory = attendance.ExcusedAbsenceCategory,
                Attendance = new CodeVm
                {
                    Code = attendance.Code,
                    Description = attendance.Description
                }
            };
        }
    }
}
