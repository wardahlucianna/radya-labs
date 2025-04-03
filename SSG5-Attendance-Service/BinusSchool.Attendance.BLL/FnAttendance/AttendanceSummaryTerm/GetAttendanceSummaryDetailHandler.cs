using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDetailHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailRequest>();
            string[] _columns = { "StudentName", "Class", "AttendanceRate", "ClassSession", "UnexcusedAbsent", "ExcusedAbsent", "PresenceRate", "AbsenceDate"};

            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdClassroom = param.IdClassroom,
                IdGrade = param.IdGrade,
                IdLevel = param.IdLevel,
                Semester = param.Semester
            };

            var IdHomerooms = await GetAttendanceSummaryHandler.GetHomeroomByUser(_dbContext, CancellationToken, filterIdHomerooms);

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisStudentStatus = await AttendanceSummaryRedisCacheHandler.GetStudentStatus(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisPeriod = await AttendanceSummaryRedisCacheHandler.GetPeriod(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisFormula = await AttendanceSummaryRedisCacheHandler.GetFormula(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisAttendance = await AttendanceSummaryRedisCacheHandler.GetAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var GetMappingAttendance = redisMappingAttendance.FirstOrDefault();

            var GetWorkhabit = await _dbContext.Entity<MsMappingAttendanceWorkhabit>()
            .Include(e => e.Workhabit)
            .Where(x => x.IdMappingAttendance == GetMappingAttendance.Id)
            .ToListAsync(CancellationToken);

            var GetExcusedAbsen = redisAttendance.Where(e => e.AbsenceCategory == AbsenceCategory.Excused).Select(e => e.Id).ToList();
            var GetUnexcusedAbsen = redisAttendance.Where(e => e.AbsenceCategory == AbsenceCategory.Unexcused).Select(e => e.Id).ToList();
            var GetUaEa = redisAttendance.Where(e => e.AbsenceCategory != null).Select(e => e.Id).ToList();
            var GetLate = redisAttendance.Where(e => e.Code == "LT").Select(e => e.Id).ToList();

            var GetPeriod = redisPeriod.Distinct();

            var CurrentPeriod = await _dbContext.Entity<MsPeriod>()
               .Include(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == redisPeriod.Select(x => x.IdSchool).First() && x.StartDate.Date <= _datetime.ServerTime.Date
                                    && x.EndDate.Date >= _datetime.ServerTime.Date)
               .OrderByDescending(x => x.AttendanceEndDate)
               .FirstOrDefaultAsync(CancellationToken);

            var endDatePeriod = _datetime.ServerTime.Date;

            if (CurrentPeriod != null)
                if (param.IdAcademicYear != CurrentPeriod.Grade.Level.IdAcademicYear)
                    endDatePeriod = redisPeriod.Max(e => e.AttendanceEndDate);

            var listIdStudentActive = redisStudentStatus
                                        .Where(x => x.StartDate.Date <= _datetime.ServerTime.Date
                                               && x.EndDate.Date >= endDatePeriod)
                                        .Select(e => e.IdStudent)
                                        .ToList();


            var Query = _dbContext.Entity<TrAttendanceSummaryTerm>()
                .Include(e => e.Level)
                .Include(e => e.AcademicYear)
                .Include(e => e.Grade)
                .Include(e => e.Student)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Where(x => IdHomerooms.Contains(x.IdHomeroom)
                            && x.IdAcademicYear == param.IdAcademicYear && x.IdLevel == param.IdLevel
                            && listIdStudentActive.Contains(x.IdStudent)
                            );

            if (!string.IsNullOrEmpty(param.IdGrade))
                Query = Query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdPeriod))
                Query = Query.Where(x => x.IdPeriod == param.IdPeriod);
            if (!string.IsNullOrEmpty(param.IdClassroom))
                Query = Query.Where(x => x.Homeroom.GradePathwayClassroom.IdClassroom == param.IdClassroom);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                Query = Query.Where(x => x.Homeroom.Semester == param.Semester);

            var DataAttendanceSummaryTerm = await Query.ToListAsync(CancellationToken);

            var DataAttendanceSummaryTermByStudent = DataAttendanceSummaryTerm.Select(e => new
            {
                IdStudent = e.IdStudent,
                NamaStudent = NameUtil.GenerateFullName(e.Student.FirstName, e.Student.MiddleName, e.Student.LastName),
                Homeroom = e.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                IdAcademicYear = e.IdAcademicYear,
                IdLevel = e.IdLevel,
                IdGrade = e.IdGrade,
            }).Distinct().ToList();

            List<GetAttendanceSummaryDetailResult> dataAttendanceSummary = new List<GetAttendanceSummaryDetailResult>();

            foreach (var itemAttendanceSummary in DataAttendanceSummaryTermByStudent)
            {

                var attendanceRate = FormulaUtil.CalculateNew(redisFormula.AttendanceRate,
                                                                GetMappingAttendance.AbsentTerms,
                                                                DataAttendanceSummaryTerm
                                                                .Where(x => x.IdStudent == itemAttendanceSummary.IdStudent)
                                                                .ToList());

                var totalDay = DataAttendanceSummaryTerm
                                .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalDayName
                                    && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default
                                    && x.IdStudent == itemAttendanceSummary.IdStudent)
                                .Select(x => x.Total)
                                .Sum();

                var totalSession = DataAttendanceSummaryTerm
                                    .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalSessionName
                                        && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default
                                        && x.IdStudent == itemAttendanceSummary.IdStudent)
                                    .Select(x => x.Total)
                                    .Sum();

                var unexcusedAbsent = DataAttendanceSummaryTerm
                                            .Where(x => GetUnexcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                                && x.IdStudent == itemAttendanceSummary.IdStudent
                                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                            .Select(x => x.Total).Sum();

                var excusedAbsent = DataAttendanceSummaryTerm
                                            .Where(x => GetExcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                                && x.IdStudent == itemAttendanceSummary.IdStudent
                                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                            .Select(x => x.Total).Sum();

                var lateness = DataAttendanceSummaryTerm
                               .Where(x => GetLate.Contains(x.IdAttendanceWorkhabit)
                                   && x.IdStudent == itemAttendanceSummary.IdStudent
                                   && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                               .Select(x => x.Total).Sum();

                var presenceRate = FormulaUtil.CalculateNew(redisFormula.PresenceInClass,
                                                            GetMappingAttendance.AbsentTerms,
                                                            DataAttendanceSummaryTerm
                                                            .Where(x => x.IdStudent == itemAttendanceSummary.IdStudent)
                                                            .ToList());

                #region ExcusedAbsenceCategory
                var isExcusedAbsenceCategory = redisAttendance.Where(e => e.ExcusedAbsenceCategory != null).Any();
                List<ExcusedAbsence> listExcusedAbsenceCategory = new List<ExcusedAbsence>();
                if (isExcusedAbsenceCategory)
                {
                    var countPersonal = DataAttendanceSummaryTerm
                                        .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultPersonalName
                                            && x.IdStudent == itemAttendanceSummary.IdStudent
                                            && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.ExcusedAbsenceCategory)
                                        .Select(x => x.Total).Sum();

                    var Personal = DataAttendanceSummaryTerm
                    .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultPersonalName
                        && x.IdStudent == itemAttendanceSummary.IdStudent
                        && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.ExcusedAbsenceCategory)
                    .ToList();

                    var countAssignBySchool = DataAttendanceSummaryTerm
                                                .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultAssignBySchoolName
                                                    && x.IdStudent == itemAttendanceSummary.IdStudent
                                                    && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.ExcusedAbsenceCategory)
                                                .Select(x => x.Total).Sum();

                    var AssignBySchool = DataAttendanceSummaryTerm
                            .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultAssignBySchoolName
                                && x.IdStudent == itemAttendanceSummary.IdStudent
                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.ExcusedAbsenceCategory)
                            .ToList();

                    listExcusedAbsenceCategory = new List<ExcusedAbsence>()
                                                {
                                                    new ExcusedAbsence()
                                                    {
                                                        Category = ExcusedAbsenceCategory.Personal,
                                                        Count = countPersonal
                                                    },
                                                    new ExcusedAbsence()
                                                    {
                                                        Category = ExcusedAbsenceCategory.AssignBySchool,
                                                        Count = countAssignBySchool
                                                    }
                                                };
                }
                else
                {
                    var countAttendance = DataAttendanceSummaryTerm
                                            .Where(x => GetExcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                                && x.IdStudent == itemAttendanceSummary.IdStudent
                                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                            .Select(x => x.Total).Sum();

                    listExcusedAbsenceCategory = new List<ExcusedAbsence>()
                    {
                        new ExcusedAbsence()
                        {
                            Category = null,
                            Count = countAttendance
                        }
                    };
                }
                #endregion

                var listWorkhabit = GetWorkhabit.Select(x => new Workhabit
                {
                    Id = x.Id,
                    Code = x.Workhabit.Code,
                    Description = x.Workhabit.Description,
                    Count = DataAttendanceSummaryTerm
                                                .Where(y => y.IdAttendanceWorkhabit == x.Id
                                                    && y.IdStudent == itemAttendanceSummary.IdStudent
                                                    && y.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Workhabit)
                                                .Select(y => y.Total).Sum(),
                }).ToList();

                var absenceRate = DataAttendanceSummaryTerm
                                .Where(x => GetUaEa.Contains(x.IdAttendanceWorkhabit)
                                    && x.IdStudent == itemAttendanceSummary.IdStudent
                                    && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                .Select(x => x.Total).Sum();

                if (!string.IsNullOrEmpty(param.IdLevel))
                    GetPeriod = GetPeriod.Where(x => x.IdLevel == param.IdLevel);
                if (!string.IsNullOrEmpty(param.IdGrade))
                {
                    GetPeriod = GetPeriod.Where(x => x.IdGrade == param.IdGrade);

                    if (!string.IsNullOrEmpty(param.IdPeriod))
                        GetPeriod = GetPeriod.Where(x => x.Id == param.IdPeriod);

                    if (!string.IsNullOrEmpty(param.Semester.ToString()))
                        GetPeriod = GetPeriod.Where(x => x.Semester == param.Semester);
                }
                
                var startDate = GetPeriod.Select(e=>e.StartDate).Min();
                var endDate = GetPeriod.Select(e=>e.EndDate).Max();

                var attendanceSummary = new GetAttendanceSummaryDetailResult
                {
                    IdAcademicYear = itemAttendanceSummary.IdAcademicYear,
                    IdLevel = itemAttendanceSummary.IdLevel,
                    IdGrade = itemAttendanceSummary.IdGrade,
                    Student = new NameValueVm
                    {
                        Id = itemAttendanceSummary.IdStudent,
                        Name = itemAttendanceSummary.NamaStudent
                    },
                    Homeroom = new NameValueVm
                    {
                        Name = itemAttendanceSummary.Homeroom
                    },
                    AttendanceRate = attendanceRate >= 0 ? attendanceRate : 0,
                    ClassSession = GetMappingAttendance.AbsentTerms == AbsentTerm.Day ? totalDay : totalSession,
                    UnexcusedAbsent = unexcusedAbsent,
                    Lateness = lateness,
                    PresenceRate = presenceRate >= 0 ? presenceRate : 0,
                    ExcusedAbsence = listExcusedAbsenceCategory,
                    Workhabits = GetMappingAttendance.IsUseWorkhabit ? listWorkhabit : new List<Workhabit>(),
                    AbsenceRate = absenceRate,
                    StartDate = startDate,
                    EndDate = endDate,
                    excusedAbsent  = excusedAbsent
                };

                dataAttendanceSummary.Add(attendanceSummary);
            }

            var query = dataAttendanceSummary.Distinct();
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.Student.Name.ToLower().Contains(param.Search.ToLower()));

            if (!string.IsNullOrEmpty(param.Percent.ToString()))
            {
                if (param.AttendanceType == SummeryAttendanceTermAttendanceType.Attendance)
                {
                    if (param.Measure == SummeryAttendanceTermMeasure.Above)
                    {
                        query = query.Where(x => x.AttendanceRate > param.Percent);
                    }
                    else if (param.Measure == SummeryAttendanceTermMeasure.Equal)
                    {
                        query = query.Where(x => x.AttendanceRate == param.Percent);
                    }
                    else if (param.Measure == SummeryAttendanceTermMeasure.Below)
                    {
                        query = query.Where(x => x.AttendanceRate < param.Percent);
                    }
                }
                else if (param.AttendanceType == SummeryAttendanceTermAttendanceType.Absent)
                {
                    if (param.Measure == SummeryAttendanceTermMeasure.Above)
                    {
                        query = query.Where(x => (x.UnexcusedAbsent + x.ExcusedAbsence.Select(e => e.Count).Sum()) > param.Percent);
                    }
                    else if (param.Measure == SummeryAttendanceTermMeasure.Equal)
                    {
                        query = query.Where(x => (x.UnexcusedAbsent + x.ExcusedAbsence.Select(e => e.Count).Sum()) == param.Percent);
                    }
                    else if (param.Measure == SummeryAttendanceTermMeasure.Below)
                    {
                        query = query.Where(x => (x.UnexcusedAbsent + x.ExcusedAbsence.Select(e => e.Count).Sum()) < param.Percent);
                    }
                }
            }

            var GetAttendanceSummary = query.Distinct();

            switch (param.OrderBy)
            {
                case "StudentName":
                    GetAttendanceSummary = param.OrderType == OrderType.Desc
                        ? GetAttendanceSummary.OrderByDescending(x => x.Student.Name)
                        : GetAttendanceSummary.OrderBy(x => x.Student.Name);
                    break;
                case "Class":
                    GetAttendanceSummary = param.OrderType == OrderType.Desc
                        ? GetAttendanceSummary.OrderByDescending(x => x.Homeroom.Name)
                        : GetAttendanceSummary.OrderBy(x => x.Homeroom.Name);
                    break;
                case "AttendanceRate":
                    GetAttendanceSummary = param.OrderType == OrderType.Desc
                        ? GetAttendanceSummary.OrderByDescending(x => x.AttendanceRate)
                        : GetAttendanceSummary.OrderBy(x => x.AttendanceRate);
                    break;
                case "ClassSession":
                    GetAttendanceSummary = param.OrderType == OrderType.Desc
                        ? GetAttendanceSummary.OrderByDescending(x => x.ClassSession)
                        : GetAttendanceSummary.OrderBy(x => x.ClassSession);
                    break;
                case "UnexcusedAbsent":
                    GetAttendanceSummary = param.OrderType == OrderType.Desc
                        ? GetAttendanceSummary.OrderByDescending(x => x.UnexcusedAbsent)
                        : GetAttendanceSummary.OrderBy(x => x.UnexcusedAbsent);
                    break;
                case "ExcusedAbsent":
                    GetAttendanceSummary = param.OrderType == OrderType.Desc
                        ? GetAttendanceSummary.OrderByDescending(x => x.excusedAbsent)
                        : GetAttendanceSummary.OrderBy(x => x.excusedAbsent);
                    break;
                case "PresenceRate":
                    GetAttendanceSummary = param.OrderType == OrderType.Desc
                        ? GetAttendanceSummary.OrderByDescending(x => x.PresenceRate)
                        : GetAttendanceSummary.OrderBy(x => x.PresenceRate);
                    break;
                case "AbsenceDate":
                    GetAttendanceSummary = param.OrderType == OrderType.Desc
                        ? GetAttendanceSummary.OrderByDescending(x => x.AttendanceRate)
                        : GetAttendanceSummary.OrderBy(x => x.AttendanceRate);
                    break;
            };


            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = GetAttendanceSummary
                        .Select(e => new GetAttendanceSummaryDetailResult
                        {
                            IdGrade=e.IdGrade,
                            IdAcademicYear = e.IdAcademicYear,
                            IdLevel=e.IdLevel,
                            Student = e.Student,
                            Homeroom = e.Homeroom,
                            AttendanceRate = e.AttendanceRate,
                            ClassSession = e.ClassSession,
                            UnexcusedAbsent = e.UnexcusedAbsent,
                            ExcusedAbsence = e.ExcusedAbsence,
                            PresenceRate = e.PresenceRate,
                            Workhabits = e.Workhabits,
                            StartDate = e.StartDate,
                            EndDate = e.EndDate,
                            AbsenceRate = e.AbsenceRate,
                            Lateness = e.Lateness,
                            excusedAbsent = e.excusedAbsent,
                            AbsenceDate = e.UnexcusedAbsent + e.excusedAbsent
                        })
                        .Distinct().ToList();
            }
            else
            {
                items = GetAttendanceSummary
                        .SetPagination(param)
                       .Select(e => new GetAttendanceSummaryDetailResult
                       {
                           IdGrade = e.IdGrade,
                           IdAcademicYear = e.IdAcademicYear,
                           IdLevel = e.IdLevel,
                           Student = e.Student,
                           Homeroom = e.Homeroom,
                           AttendanceRate = e.AttendanceRate,
                           ClassSession = e.ClassSession,
                           UnexcusedAbsent = e.UnexcusedAbsent,
                           ExcusedAbsence = e.ExcusedAbsence,
                           PresenceRate = e.PresenceRate,
                           Workhabits = e.Workhabits,
                           StartDate = e.StartDate,
                           EndDate = e.EndDate,
                           AbsenceRate = e.AbsenceRate,
                           Lateness = e.Lateness,
                           excusedAbsent = e.excusedAbsent,
                           AbsenceDate = e.UnexcusedAbsent + e.excusedAbsent
                       })
                       .Distinct().ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : dataAttendanceSummary.Select(x => x.Student.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));


        }
    }
}
