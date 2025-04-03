using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnLongRun.Services
{
    public class AttendanceSummaryV3Service : IAttendanceSummaryV3Service
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<AttendanceSummaryV3Service> _logger;

        private readonly Dictionary<string, string> _dictSchool;
        private readonly Dictionary<string, string> _dictAcademicYearBySchool;

        public AttendanceSummaryV3Service(IAttendanceDbContext dbContext,
            IMachineDateTime dateTime,
            ILogger<AttendanceSummaryV3Service> logger)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _logger = logger;

            _dictSchool = new Dictionary<string, string>();
            _dictAcademicYearBySchool = new Dictionary<string, string>();
        }

        public async Task<string> GetSchoolNameAsync(string idSchool, CancellationToken cancellationToken)
        {
            if (_dictSchool.TryGetValue(idSchool, out var s))
                return s;

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
            if (_dictAcademicYearBySchool.TryGetValue(idSchool, out var s))
                return s;

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

        public Task<List<PeriodDto>> GetPeriodsAsync(string idGrade, CancellationToken cancellationToken)
            => _dbContext.Entity<MsPeriod>()
                .Where(e => e.IdGrade == idGrade)
                .OrderBy(e => e.StartDate)
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

        public Task<List<StudentDto>> GetStudentEnrolledBy(string idHomeroom, CancellationToken cancellationToken)
            => _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(e => e.HomeroomStudent)
                .Where(e => e.HomeroomStudent.IdHomeroom == idHomeroom)
                .Select(e => new StudentDto
                {
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    LessonId = e.IdLesson,
                })
                .ToListAsync(cancellationToken);

        public async Task<Dictionary<string, List<StudentDto>>> GetStudentEnrolledGroupByIdLesson(string idHomeroom,
            CancellationToken cancellationToken)
        {
            var results = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(e => e.HomeroomStudent)
                .Where(e => e.HomeroomStudent.IdHomeroom == idHomeroom)
                .Select(e => new StudentDto
                {
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    IdStudent = e.HomeroomStudent.IdStudent,
                    LessonId = e.IdLesson,
                }).ToListAsync(cancellationToken);

            return results.GroupBy(e => e.LessonId).ToDictionary(e => e.Key, g => g.ToList());
        }

        public async Task<List<StudentEnrollmentDto>> GetStudentEnrolled(string idHomeroom,
            DateTime startAttendanceDt,
            CancellationToken cancellationToken)
        {
            var students = await _dbContext.Entity<MsHomeroomStudent>()
                .Where(e => e.IdHomeroom == idHomeroom)
                .Select(e => new MsHomeroomStudent
                {
                    Id = e.Id,
                    IdStudent = e.IdStudent,
                })
                .ToListAsync(cancellationToken);

            var idHomeroomStudents = students.Select(e => e.Id).ToList();

            var allHomeroomStudentEnrollments = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Where(e => idHomeroomStudents.Contains(e.IdHomeroomStudent))
                .Select(e => new HomeroomStudentEnrollDto
                {
                    IdHomeroomStudentEnrollment = e.Id,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    IdLesson = e.IdLesson,
                    IdLessonOld = null,
                    Date = startAttendanceDt,
                    DateIn = e.DateIn.Value,
                    IsDeleted = false,
                    IsFromHistory = false,
                })
                .ToListAsync(cancellationToken);

            var allTrHomeroomStudentEnrollments = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                .Where(e => idHomeroomStudents.Contains(e.IdHomeroomStudent))
                .Select(e => new HomeroomStudentEnrollDto
                {
                    IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    IdLessonOld = e.IdLessonOld,
                    IdLesson = e.IdLessonNew,
                    Date = e.StartDate,
                    DateIn = e.DateIn.Value,
                    IsDeleted = e.IsDelete,
                    Flag = 1,
                    IsFromHistory = e.IsShowHistory
                })
                .ToListAsync(cancellationToken);

            var allUnionList = allHomeroomStudentEnrollments.Union(allTrHomeroomStudentEnrollments)
                .GroupBy(e => e.IdHomeroomStudent)
                .ToDictionary(g => g.Key, g => g.ToList());

            var list = new List<StudentEnrollmentDto>();

            foreach (var item in students)
            {
                if (!allUnionList.ContainsKey(item.Id))
                    continue;

                var vmItem = new StudentEnrollmentDto
                {
                    IdHomeroomStudent = item.Id,
                    IdStudent = item.IdStudent
                };

                var fixedList = allUnionList[item.Id]
                    .GroupBy(e => e.IdHomeroomStudentEnrollment)
                    .Select(e => new
                    {
                        e.Key, Items =
                            e.OrderBy(y => y.Flag)
                                .ThenBy(y => y.Date)
                                .ThenBy(y => y.IsFromHistory)
                                .ThenBy(y => y.DateIn)
                                .ToList()
                    })
                    .ToList();

                foreach (var item2 in fixedList)
                {
                    //tidak pernah di moving
                    if (item2.Items.Count == 1)
                    {
                        vmItem.Items.Add(new StudentEnrollmentItemDto
                        {
                            IdLesson = item2.Items[0].IdLesson,
                            StartDt = startAttendanceDt,
                            Ignored = false
                        });
                        continue;
                    }

                    //logic moving
                    var fixedItems = RecalculateHomeroomStudentEnroll(item2.Items);

                    for (var i = 0; i < fixedItems.Count; i++)
                    {
                        var vmChildItem = new StudentEnrollmentItemDto
                        {
                            IdLesson = fixedItems[i].IdLesson,
                            StartDt = fixedItems[i].Date,
                        };

                        if (i + 1 < fixedItems.Count)
                            vmChildItem.EndDt = fixedItems[i + 1].Date;

                        vmItem.Items.Add(vmChildItem);
                    }
                }

                list.Add(vmItem);
            }

            return list;
        }

        public async Task<List<StudentStatusDto>> GetStudentStatusesAsync(string[] studentIds,
            string idAcademicYear,
            DateTime lastPeriodDt,
            CancellationToken cancellationToken)
        {
            var list = new List<StudentStatusDto>();

            var queryable = _dbContext.Entity<TrStudentStatus>()
                .AsQueryable();

            var studentStatuses = await queryable
                .Select(e => new TrStudentStatus
                {
                    IdTrStudentStatus = e.IdTrStudentStatus,
                    IdAcademicYear = e.IdAcademicYear,
                    IdStudent = e.IdStudent,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    ActiveStatus = e.ActiveStatus,
                    DateIn = e.DateIn
                })
                .Where(e => studentIds.Contains(e.IdStudent) && e.IdAcademicYear == idAcademicYear && e.ActiveStatus == true)
                .ToListAsync(cancellationToken);

            foreach (var item in studentStatuses)
            {
                var vmItem = new StudentStatusDto
                {
                    IdStudent = item.IdStudent,
                    IsActive = item.ActiveStatus,
                    StartDt = item.StartDate,
                    EndDt = lastPeriodDt
                };

                if (item.EndDate.HasValue)
                    vmItem.EndDt = item.EndDate.Value;

                list.Add(vmItem);
            }

            return list;
        }

        public List<HomeroomStudentEnrollDto> RecalculateHomeroomStudentEnroll(List<HomeroomStudentEnrollDto> data)
        {
            var list = new List<HomeroomStudentEnrollDto>();
            var fixedList = new List<HomeroomStudentEnrollDto>();
            //construct data to a single row
            for (var i = 1; i < data.Count; i++)
            {
                if (i == 1)
                {
                    list.Add(new HomeroomStudentEnrollDto
                    {
                        IdHomeroomStudentEnrollment = data[0].IdHomeroomStudentEnrollment,
                        IdHomeroomStudent = data[0].IdHomeroomStudent,
                        IdLesson = data[i].IdLessonOld,
                        Date = data[0].Date,
                        DateIn = data[0].DateIn,
                        IsDeleted = data[0].IsDeleted
                    });

                    list.Add(new HomeroomStudentEnrollDto
                    {
                        IdHomeroomStudentEnrollment = data[i].IdHomeroomStudentEnrollment,
                        IdHomeroomStudent = data[i].IdHomeroomStudent,
                        IdLesson = data[i].IdLesson,
                        Date = data[i].Date,
                        DateIn = data[i].DateIn,
                        IsDeleted = data[i].IsDeleted
                    });

                    continue;
                }

                list.Add(new HomeroomStudentEnrollDto
                {
                    IdHomeroomStudentEnrollment = data[i].IdHomeroomStudentEnrollment,
                    IdHomeroomStudent = data[i].IdHomeroomStudent,
                    IdLesson = data[i].IdLesson,
                    Date = data[i].Date,
                    DateIn = data[i].DateIn,
                    IsDeleted = data[i].IsDeleted
                });
            }

            var index = 0;
            foreach (var item in list)
            {
                if (index == 0)
                {
                    fixedList.Add(item);
                    index++;
                    continue;
                }

                if (fixedList.Last().IdLesson == item.IdLesson)
                {
                    fixedList.Last().Date = item.Date;
                    fixedList.Last().DateIn = item.DateIn;
                    index++;
                    continue;
                }

                fixedList.Add(item);
                index++;
            }

            fixedList.Reverse();
            list.Clear();

            foreach (var t in fixedList)
                if (t.Date.Date <= _dateTime.ServerTime.Date)
                {
                    if (list.Any())
                    {
                        if (t.Date.Date < list.Last().Date.Date)
                            list.Add(t);
                    }
                    else
                        list.Add(t);
                }

            list.Reverse();

            return list;
        }

        public Task<List<HomeroomDto>> GetHomeroomsAsync(string idGrade, CancellationToken cancellationToken)
            => _dbContext.Entity<MsHomeroom>()
                .Where(e => e.IdGrade == idGrade)
                .OrderBy(e => e.DateIn)
                .Select(e => new HomeroomDto
                {
                    IdHomeroom = e.Id,
                    Semester = e.Semester
                }).ToListAsync(cancellationToken);

        public async Task<Dictionary<int, List<HomeroomDto>>> GetHomeroomsGroupedBySemester(string idGrade,
            CancellationToken cancellationToken)
        {
            var results = await _dbContext.Entity<MsHomeroom>()
                .Where(e => e.IdGrade == idGrade)
                .OrderBy(e => e.DateIn)
                .Select(e => new HomeroomDto
                {
                    IdHomeroom = e.Id,
                    Semester = e.Semester
                }).ToListAsync(cancellationToken);

            return results.GroupBy(e => e.Semester).ToDictionary(e => e.Key, g => g.ToList());
        }

        public async Task DeleteTermByAcademicYearAsync(string idAcademicYear, CancellationToken cancellationToken)
        {
            await using (var command = _dbContext.DbFacade.GetDbConnection().CreateCommand())
            {
                command.CommandText = $@"
DELETE FROM dbo.TrAttendanceSummaryTerm
WHERE IdAcademicYear = '{idAcademicYear}';
";
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 0;
                await _dbContext.DbFacade.OpenConnectionAsync(cancellationToken);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public Task<List<ScheduleDto>> GetScheduleAsync(string idAcademicYear, string idGrade,
            CancellationToken cancellationToken)
            => _dbContext.Entity<MsScheduleLesson>()
                .Where(e => e.ScheduleDate.Date <= _dateTime.ServerTime.Date
                            && e.IdAcademicYear == idAcademicYear
                            && e.IdGrade == idGrade)
                .Select(e => new ScheduleDto
                {
                    IdSchedule = e.Id,
                    ScheduleDate = e.ScheduleDate,
                    IdLesson = e.IdLesson,
                    IdLevel = e.IdLevel,
                    IdGrade = e.IdGrade,
                    IdSubject = e.IdSubject,
                    IdSession = e.IdSession
                })
                .ToListAsync(cancellationToken);

        public Task<List<ScheduleDto>> GetScheduleAsync(string idAcademicYear, string idGrade, DateTime start,
            DateTime end,
            CancellationToken cancellationToken)
            => _dbContext.Entity<MsScheduleLesson>()
                .Include(x=> x.Lesson)
                .Where(e =>
                    e.ScheduleDate.Date >= start.Date
                    && e.ScheduleDate.Date <= end.Date
                    && e.IdAcademicYear == idAcademicYear
                    && e.IdGrade == idGrade)
                .Select(e => new ScheduleDto
                {
                    IdSchedule = e.Id,
                    ScheduleDate = e.ScheduleDate,
                    IdLesson = e.IdLesson,
                    IdLevel = e.IdLevel,
                    IdGrade = e.IdGrade,
                    IdSubject = e.IdSubject,
                    IdSession = e.IdSession,
                    SessionID = e.SessionID,
                    IdDay = e.IdDay
                })
                .ToListAsync(cancellationToken);

        public Task<List<AttendanceDto>> GetAttendanceEntriesAsync(string[] idSchedules,
            CancellationToken cancellationToken)
            => _dbContext.Entity<TrAttendanceEntryV2>()
                .Where(e => idSchedules.Contains(e.IdScheduleLesson))
                .Select(e => new AttendanceDto
                {
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    Status = e.Status,
                    IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                    PositionIn = e.PositionIn,
                    IdHomeroomStudent = e.IdHomeroomStudent
                })
                .ToListAsync(cancellationToken);

        public async Task<Dictionary<string, List<AttendanceDto>>> GetAttendanceEntriesGroupedAsync(
            string[] idSchedules,
            CancellationToken cancellationToken)
        {
            var results = await _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(e => e.AttendanceEntryWorkhabitV2s)
                .Where(e => idSchedules.Contains(e.IdScheduleLesson))
                .Select(e => new AttendanceDto
                {
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    Status = e.Status,
                    IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                    PositionIn = e.PositionIn,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    DateIn = e.DateIn,
                    Workhabits = e.AttendanceEntryWorkhabitV2s.Select(f => new AttendanceWorkhabitDto
                    {
                        IdEntryWorkhabit = f.Id,
                        IdMappingAttendanceWorkHabit = f.IdMappingAttendanceWorkhabit
                    }).ToList()
                }).ToListAsync(cancellationToken);

            return results.GroupBy(e => e.IdScheduleLesson).ToDictionary(g => g.Key, g => g.ToList());
        }

        public Task<List<ScheduleDto>> GetScheduleCancelAsync(string idAcademicYear, string idGrade, DateTime start, DateTime end, CancellationToken cancellationToken)
            => _dbContext.Entity<TrScheduleRealization2>()
                .Where(e =>
                    e.ScheduleDate.Date >= start.Date
                    && e.ScheduleDate.Date <= end.Date
                    && e.IdAcademicYear == idAcademicYear
                    && e.IdGrade == idGrade)
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

        // => Task.FromResult(_dbContext.Entity<TrAttendanceEntryV2>()
        //     .Include(e => e.AttendanceEntryWorkhabitV2s)
        //     .Where(e => idSchedules.Contains(e.IdScheduleLesson))
        //     .Select(e => new AttendanceDto
        //     {
        //         IdScheduleLesson = e.IdScheduleLesson,
        //         IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
        //         Status = e.Status,
        //         IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
        //         PositionIn = e.PositionIn,
        //         IdHomeroomStudent = e.IdHomeroomStudent,
        //         DateIn = e.DateIn,
        //         Workhabits = e.AttendanceEntryWorkhabitV2s.Select(f => new AttendanceWorkhabitDto
        //         {
        //             IdEntryWorkhabit = f.Id,
        //             IdMappingAttendanceWorkHabit = f.IdMappingAttendanceWorkhabit
        //         }).ToList()
        //     }).GroupBy(e => e.IdScheduleLesson)
        //     .ToDictionary(g => g.Key, g => g.ToList()));
    }
}
