using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;
        public GetAttendanceSummaryHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryRequest>();

            var key = $"ListSummaryAttendanceTerm-AttendanceSummaryTerm-{param.IdAcademicYear}";

            var getAttendanceSummaryTerm = await _redisCache.GetAsync<List<GetAttendanceSummaryResult>>(key);

            IReadOnlyList<IItemValueVm> items = new List<IItemValueVm>();
            if (getAttendanceSummaryTerm == null)
            {
                string[] _columns = { "AcademicYear", "Level", "Submited", "Unsubmited", "Pending", "Total Student" };

                var filterIdHomerooms = new GetHomeroomByIdUserRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    SelectedPosition = param.SelectedPosition,
                    IdUser = param.IdUser,
                };

                var IdHomerooms = await GetHomeroomByUser(_dbContext, CancellationToken, filterIdHomerooms);

                #region GetRedis
                var paramRedis = new RedisAttendanceSummaryRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                };

                var redisStudentStatus = await AttendanceSummaryRedisCacheHandler.GetStudentStatus(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
                var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
                var redisPeriod = await AttendanceSummaryRedisCacheHandler.GetPeriod(paramRedis, _redisCache, _dbContext, CancellationToken);
                var redisAttendanceSummaryTerm = await AttendanceSummaryRedisCacheHandler.GetAttendanceSummaryTerm(paramRedis, _redisCache, _dbContext, CancellationToken);
                #endregion

                var CurrentPeriod = await _dbContext.Entity<MsPeriod>()
                               .Include(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                               .Where(x => x.Grade.Level.AcademicYear.IdSchool == redisPeriod.Select(x=> x.IdSchool).First() && x.StartDate.Date <= _datetime.ServerTime.Date
                                                    && x.EndDate.Date >= _datetime.ServerTime.Date)
                               .OrderByDescending(x => x.AttendanceEndDate)
                               .FirstOrDefaultAsync(CancellationToken);

                var endDate = _datetime.ServerTime.Date;

                if (CurrentPeriod != null)
                    if (param.IdAcademicYear != CurrentPeriod.Grade.Level.IdAcademicYear)
                        endDate = redisPeriod.Max(e => e.AttendanceEndDate);

                var listIdStudentActive = redisStudentStatus
                                             .Where(x => x.StartDate.Date <= _datetime.ServerTime.Date
                                                    && x.EndDate.Date >= endDate)
                                             .Select(e => e.IdStudent)
                                             .ToList();

                var queryAttendanceSummaryTerm = redisAttendanceSummaryTerm
                                                .Where(x => IdHomerooms.Contains(x.Homeroom.Id));


                var DataAttendanceSummaryTerm = queryAttendanceSummaryTerm.ToList();
                var DataAttendanceSummaryTermStudentActive = queryAttendanceSummaryTerm.Where(x => listIdStudentActive.Contains(x.Student.IdStudent)).ToList();

                var GetLevel = DataAttendanceSummaryTerm
                    .Select(e => new
                    {
                        IdLevel = e.Level.Id,
                        Level = $"{e.Level.Description}({e.Level.Code})",
                        IdAcademicYear = e.AcademicYear.Id,
                        AcademicYear = e.AcademicYear.Description
                    })
                    .Distinct()
                    .ToList();

                var GetAttendanceSummaryTerm = GetLevel.Select(e => new
                {
                    AcademicYear = new NameValueVm
                    {
                        Id = e.IdAcademicYear,
                        Name = e.AcademicYear
                    },
                    Level = new NameValueVm
                    {
                        Id = e.IdLevel,
                        Name = e.Level
                    },
                    Submited = DataAttendanceSummaryTermStudentActive
                                .Where(x => x.Level.Id == e.IdLevel && x.AttendanceWorkhabitName == SummaryTermConstant.DefaultSubmittedName && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceStatus)
                                .Select(e => e.Total)
                                .Sum(),
                    Unsubmited = DataAttendanceSummaryTermStudentActive
                                .Where(x => x.Level.Id == e.IdLevel && x.AttendanceWorkhabitName == SummaryTermConstant.DefaultUnsubmittedName && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceStatus)
                                .Select(e => e.Total)
                                .Sum(),
                    Pending = DataAttendanceSummaryTermStudentActive
                                .Where(x => x.Level.Id == e.IdLevel && x.AttendanceWorkhabitName == SummaryTermConstant.DefaultPendingName && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceStatus)
                                .Select(e => e.Total)
                                .Sum(),
                    TotalDay = DataAttendanceSummaryTerm
                                .Where(x => x.Level.Id == e.IdLevel && x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalDayName && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default)
                                .Select(e => e.Total)
                                .Sum(),
                    TotalSeason = DataAttendanceSummaryTerm
                                .Where(x => x.Level.Id == e.IdLevel && x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalSessionName && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default)
                                .Select(e => e.Total)
                                .Sum(),
                    MappingAttendance = redisMappingAttendance
                                .Where(x => x.IdLevel == e.IdLevel)
                                .Select(e => e.AbsentTerms)
                                .FirstOrDefault(),
                    IsNeedValidation = redisMappingAttendance
                                .Where(x => x.IdLevel == e.IdLevel)
                                .Select(e => e.IsNeedValidation)
                                .FirstOrDefault(),
                    IsUseWorkhabit = redisMappingAttendance
                                .Where(x => x.IdLevel == e.IdLevel)
                                .Select(e => e.IsUseWorkhabit)
                                .FirstOrDefault(),
                    TotalStudent = DataAttendanceSummaryTerm
                                .Where(x => x.Level.Id == e.IdLevel)
                                .Select(e => e.Student.IdStudent)
                                .Distinct().Count(),
                    StartDate = redisPeriod.Where(x => x.IdLevel == e.IdLevel).Min(e => e.StartDate),
                    EndDate = redisPeriod.Where(x => x.IdLevel == e.IdLevel).Max(e => e.EndDate),
                }).ToList();

                var result = GetAttendanceSummaryTerm
                            .Select(e => new GetAttendanceSummaryResult
                            {
                                AcademicYear = e.AcademicYear,
                                Level = e.Level,
                                Submited = e.Submited,
                                Pending = e.IsNeedValidation ? e.Pending : 0,
                                Unsubmitted = e.Unsubmited,
                                TotalStudent = e.TotalStudent,
                                IsNeedValidation = e.IsNeedValidation,
                                IsUseWorkhabit = e.IsUseWorkhabit,
                                Startdate = e.StartDate,
                                Enddate = e.EndDate,
                                AbsentTerm = e.MappingAttendance
                            })
                            .ToList();

                items = result
                        .ToList();

                await _redisCache.SetAsync(key, result, TimeSpan.FromMinutes(5));
            }
            else
            {
                items = getAttendanceSummaryTerm.ToList();
            }

            return Request.CreateApiResult2(items as object);
        }

        public static async Task<List<string>> GetHomeroomByUser(IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken, GetHomeroomByIdUserRequest param)
        {
            List<string> IdHomerooms = new List<string>();

            var queryHomeroom = _dbContext.Entity<MsHomeroom>()
                                      .Include(e => e.Grade).ThenInclude(e => e.Level)
                                      .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdClassroom))
                queryHomeroom = queryHomeroom.Where(e => e.GradePathwayClassroom.IdClassroom == param.IdClassroom);
            if (!string.IsNullOrEmpty(param.IdLevel))
                queryHomeroom = queryHomeroom.Where(e => e.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                queryHomeroom = queryHomeroom.Where(e => e.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                queryHomeroom = queryHomeroom.Where(e => e.Semester == param.Semester);

            var GetHomeroom = await queryHomeroom.ToListAsync(CancellationToken);

            var GetGrade = await _dbContext.Entity<MsGrade>()
                                      .Include(e => e.Level)
                                      .Where(e => e.Level.IdAcademicYear == param.IdAcademicYear)
                                      .ToListAsync(CancellationToken);

            //HomeroomTeacher
            if (param.SelectedPosition == null || param.SelectedPosition.ToLower()== "all")
            {
                IdHomerooms.AddRange(GetHomeroom.Select(e => e.Id).ToList());

            }
            else if (param.SelectedPosition == PositionConstant.ClassAdvisor || param.SelectedPosition == PositionConstant.CoTeacher)
            {
                var HomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                .Include(x => x.TeacherPosition).ThenInclude(e => e.LtPosition)
                .Include(x => x.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                .Where(x => x.IdBinusian == param.IdUser && x.TeacherPosition.LtPosition.Code == param.SelectedPosition && x.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                .Select(e => e.Homeroom.Id)
                .ToListAsync(CancellationToken);
                IdHomerooms.AddRange(HomeroomTeacher);
            }
            else if (param.SelectedPosition == PositionConstant.SubjectTeacher)
            {
                var GetIdLessonBySubjetTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                    .Include(e => e.Lesson)
                                    .Where(x => x.IdUser == param.IdUser && x.Lesson.IdAcademicYear == param.IdAcademicYear)
                                    .Select(e => e.IdLesson)
                                    .ToListAsync(CancellationToken);

                var IdHomeroomBySubjectTeacher = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(e=>e.HomeroomStudent)
                                                    .Where(e => GetIdLessonBySubjetTeacher.Contains(e.IdLesson)).Select(e=>e.HomeroomStudent.IdHomeroom)
                                                    .Distinct()
                                                    .ToListAsync(CancellationToken);

                IdHomerooms.AddRange(IdHomeroomBySubjectTeacher);
            }
            else
            {
                var GetTeacherNonTeaching = await _dbContext.Entity<TrNonTeachingLoad>()
                                 .Include(e => e.NonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                                 .Where(x => x.IdUser == param.IdUser && x.NonTeachingLoad.TeacherPosition.LtPosition.Code == param.SelectedPosition && x.Data != null && x.NonTeachingLoad.IdAcademicYear == param.IdAcademicYear)
                                 .ToListAsync(CancellationToken);

                var GetDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                         .Include(e => e.Department)
                                         .Where(e => e.Department.IdAcademicYear == param.IdAcademicYear)
                                         .ToListAsync(CancellationToken);

                foreach (var item in GetTeacherNonTeaching)
                {
                    var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                    _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                    if (_DepartemenPosition != null)
                    {
                        var getDepartmentLevelbyIdLevel = GetDepartmentLevel.Any(e => e.IdDepartment == _DepartemenPosition.Id);
                        if (getDepartmentLevelbyIdLevel)
                        {
                            var GetIdGrades = GetDepartmentLevel.Where(e => e.IdDepartment == _DepartemenPosition.Id)
                                .Select(e => e.Level.Grades.Select(e => e.Id).ToList())
                                .FirstOrDefault();
                            var IdHomeroomByGrade = GetHomeroom.Where(e => GetIdGrades.Contains(e.IdGrade)).Select(e => e.Id).ToList();
                            IdHomerooms.AddRange(IdHomeroomByGrade);
                        }
                    }


                    //ByGrade or Level
                    _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                    _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                    if (_GradePosition == null && _LevelPosition != null)
                    {
                        var GetIdGrades = GetGrade.Where(e => e.IdLevel == _LevelPosition.Id)
                                .Select(e => e.Id)
                                .ToList();
                        var IdHomeroomByGrade = GetHomeroom.Where(e => GetIdGrades.Contains(e.IdGrade)).Select(e => e.Id).ToList();
                        IdHomerooms.AddRange(IdHomeroomByGrade);
                    }
                    else if (_GradePosition != null)
                    {
                        var GetIdGrades = GetGrade.Where(e => e.Id == _GradePosition.Id)
                           .Select(e => e.Id)
                           .ToList();
                        var IdHomeroomByGrade = GetHomeroom.Where(e => GetIdGrades.Contains(e.IdGrade)).Select(e => e.Id).ToList();
                        IdHomerooms.AddRange(IdHomeroomByGrade);
                    }
                }
            }

            IdHomerooms = IdHomerooms.Distinct().ToList();

            return IdHomerooms;
        }

        

    }
}
