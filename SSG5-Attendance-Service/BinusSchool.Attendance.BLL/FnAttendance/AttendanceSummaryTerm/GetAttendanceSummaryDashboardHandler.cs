using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDashboardHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDashboardHandler(IAttendanceDbContext dbContext, IMachineDateTime DateTime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _dateTime = DateTime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDashboardRequest>();

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
            };

            //var redisHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var queryHomeroomStudentEnrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
               .Include(e => e.Lesson)
               .Include(e => e.Subject)
               .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear && e.HomeroomStudent.IdStudent == param.IdStudent);
            
            var GetHomeroomStudent = await queryHomeroomStudentEnrollment
                    .AsNoTracking()
                    .GroupBy(e => new Data.Model.Attendance.FnAttendance.AttendanceV2.GetHomeroom
                    {
                        IdLesson = e.IdLesson,
                        IdHomeroomStudent = e.IdHomeroomStudent,
                        ClassId = e.Lesson.ClassIdGenerated,
                        IdStudent = e.HomeroomStudent.Student.Id,
                        FirstName = e.HomeroomStudent.Student.FirstName,
                        MiddleName = e.HomeroomStudent.Student.MiddleName,
                        LastName = e.HomeroomStudent.Student.LastName,
                        Homeroom = new ItemValueVm
                        {
                            Id = e.HomeroomStudent.IdHomeroom,
                            Description = e.HomeroomStudent.Homeroom.Grade.Code + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                        },
                        IdClassroom = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                        ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                        ClassroomName = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                        Grade = new CodeWithIdVm
                        {
                            Id = e.HomeroomStudent.Homeroom.IdGrade,
                            Code = e.HomeroomStudent.Homeroom.Grade.Code,
                            Description = e.HomeroomStudent.Homeroom.Grade.Description
                        },
                        Level = new CodeWithIdVm
                        {
                            Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                            Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                            Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                        },
                        Semester = e.HomeroomStudent.Homeroom.Semester,
                        IdSubject = e.Subject.Id,
                        SubjectCode = e.Subject.Code,
                        SubjectName = e.Subject.Description,
                        SubjectID = e.Subject.SubjectID,
                        IsFromMaster = true,
                        IsDelete = false,
                        IdHomeroomStudentEnrollment = e.Id,
                    })
                    .Select(e => e.Key)
                    .ToListAsync(CancellationToken);

            var idLevel = GetHomeroomStudent.Where(e => e.IdStudent == param.IdStudent).Select(e => e.Level.Id).Distinct().FirstOrDefault();
            paramRedis.IdLevel = idLevel;

            //var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var queryMappingAttendance = _dbContext.Entity<MsMappingAttendance>()
                               .Where(e => e.Level.IdAcademicYear == param.IdAcademicYear)
                               ;

            if (!string.IsNullOrEmpty(idLevel))
                queryMappingAttendance = queryMappingAttendance.Where(e => e.IdLevel == idLevel);

            var listMappingAttendance = await queryMappingAttendance
                                   .GroupBy(e => new RedisAttendanceSummaryMappingAttendanceResult
                                   {
                                       Id = e.Id,
                                       IdLevel = e.IdLevel,
                                       AbsentTerms = e.AbsentTerms,
                                       IsNeedValidation = e.IsNeedValidation,
                                       IsUseWorkhabit = e.IsUseWorkhabit,
                                       IsUseDueToLateness = e.IsUseDueToLateness,
                                   })
                                    .Select(e => e.Key)
                                    .ToListAsync(CancellationToken);

            //var redisPeriod = await AttendanceSummaryRedisCacheHandler.GetPeriod(paramRedis, _redisCache, _dbContext, CancellationToken);

            var queryPeriod = _dbContext.Entity<MsPeriod>()
                              .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(idLevel))
                queryPeriod = queryPeriod.Where(e => e.Grade.IdLevel == idLevel);

            var listPeriod = await queryPeriod
                                   .GroupBy(e => new RedisAttendanceSummaryPeriodResult
                                   {
                                       Id = e.Id,
                                       IdGrade = e.IdGrade,
                                       StartDate = e.StartDate,
                                       EndDate = e.EndDate,
                                       Semester = e.Semester,
                                       IdLevel = e.Grade.IdLevel,
                                       AttendanceStartDate = e.AttendanceStartDate,
                                       AttendanceEndDate = e.AttendanceEndDate
                                   })
                                    .Select(e => e.Key)
                                    .ToListAsync(CancellationToken);

            //var redisFormula = await AttendanceSummaryRedisCacheHandler.GetFormula(paramRedis, _redisCache, _dbContext, CancellationToken);
            var queryFormula = _dbContext.Entity<MsFormula>()
              .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(idLevel))
                queryFormula = queryFormula.Where(e => e.IdLevel == idLevel);

            var getFormula = await queryFormula.FirstOrDefaultAsync(CancellationToken);
            #endregion

            if (getFormula == null)
                return Request.CreateApiResult2();

            //var GetHomeroomStudent = redisHomeroomStudentEnrollment
            //                    .Where(e => e.IdStudent == param.IdStudent)
            //                    .ToList();

            var GetLevelGrade = GetHomeroomStudent.FirstOrDefault();

            if (GetLevelGrade == null)
                return Request.CreateApiResult2();

            var GetPeriod = listPeriod
                                .Where(e =>e.IdGrade == GetLevelGrade.Grade.Id
                                            && (_dateTime.ServerTime.Date >= e.StartDate && _dateTime.ServerTime.Date <= e.EndDate))
                                .FirstOrDefault();

            if(GetPeriod==null)
                throw new BadRequestException("Period is not found");

            var GetStudentInfo = GetHomeroomStudent
                                    .Where(e => e.Semester == GetPeriod.Semester)
                                    .FirstOrDefault();

            if (GetStudentInfo == null)
            {
                GetStudentInfo = GetHomeroomStudent
                        .Where(e => e.Semester == (GetPeriod.Semester == 2 ? 1:1))
                        .FirstOrDefault();
            }

            var query = _dbContext.Entity<TrAttendanceSummaryTerm>()
                .Where(e => e.IdStudent == param.IdStudent && e.IdAcademicYear == param.IdAcademicYear);

            var GetMappingAttendance = listMappingAttendance.FirstOrDefault();

            var LastUpdate = await _dbContext.Entity<TrAttendanceSummaryLog>()
               .Where(x => x.IsDone)
               .OrderByDescending(e => e.StartDate)
               .Select(e => e.StartDate)
               .FirstOrDefaultAsync(CancellationToken);

            if (param.PeriodType == "Term")
            {
                query = query.Where(e => e.IdPeriod == GetPeriod.Id);
            }
            else if (param.PeriodType == "Semester")
            {
                query = query.Where(e => e.Semester == GetPeriod.Semester);
            }

            var GetAttendanceSummaryTerm = await query.ToListAsync(CancellationToken);
            var GetAttendance = GetAttendanceSummaryTerm
                                    .Where(e => e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                    .Select(e => new
                                    {
                                        Id = e.IdAttendanceWorkhabit,
                                        Name = e.AttendanceWorkhabitName
                                    })
                                    .Distinct()
                                    .ToList();

            var GetWorkhabit = GetAttendanceSummaryTerm
                                    .Where(e => e.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Workhabit)
                                    .Select(e => new
                                    {
                                        Id = e.IdAttendanceWorkhabit,
                                        Name = e.AttendanceWorkhabitName
                                    })
                                    .Distinct()
                                    .ToList();

            GetAttendanceSummaryDashboardResult Items = new GetAttendanceSummaryDashboardResult
            {
                BinusianId = GetStudentInfo != null? GetStudentInfo.IdStudent : string.Empty,
                IdLevel = GetStudentInfo != null? GetStudentInfo.Level.Id : string.Empty,
                StudentName = GetStudentInfo != null ? NameUtil.GenerateFullName(GetStudentInfo.FirstName, GetStudentInfo.MiddleName, GetStudentInfo.LastName) : string.Empty,
                Homeroom = GetStudentInfo != null ? (GetStudentInfo.Grade.Code + GetStudentInfo.ClassroomCode) : string.Empty,
                Term = GetMappingAttendance.AbsentTerms,
                UseWorkhabit = GetMappingAttendance.IsUseWorkhabit,
                AttendanceRate = FormulaUtil.CalculateNew(getFormula.AttendanceRate,
                                                                GetMappingAttendance.AbsentTerms,
                                                                GetAttendanceSummaryTerm
                                                                .ToList())>=0
                                                                ? FormulaUtil.CalculateNew(getFormula.AttendanceRate,
                                                                    GetMappingAttendance.AbsentTerms,
                                                                    GetAttendanceSummaryTerm
                                                                    .ToList())
                                                                : 0,
                TotalDay = GetMappingAttendance.AbsentTerms == AbsentTerm.Day
                                        ? GetAttendanceSummaryTerm
                                            .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalDayName
                                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default)
                                            .Select(x => x.Total)
                                            .Sum()
                                        : GetAttendanceSummaryTerm
                                            .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalSessionName
                                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default)
                                            .Select(x => x.Total)
                                            .Sum(),
                Attendances = GetAttendance
                                .Select(x => new AttendanceStudent
                                {
                                    IdAttendance = x.Id,
                                    AttendanceName = x.Name,
                                    Count = GetAttendanceSummaryTerm.Where(f => f.IdAttendanceWorkhabit == x.Id).Sum(e=>e.Total),
                                })
                                .ToList(),

                Workhabits = GetMappingAttendance.IsUseWorkhabit
                                ? GetWorkhabit
                                    .Select(x => new WorkhabitStudent
                                    {
                                        IdWorkhabit = x.Id,
                                        WorkhabitName = x.Name,
                                        Count = GetAttendanceSummaryTerm.Where(f => f.IdAttendanceWorkhabit == x.Id).Sum(e => e.Total),
                                    })
                                    .ToList()
                                :new List<WorkhabitStudent>(),
                ValidDate = LastUpdate,
            };

            return Request.CreateApiResult2(Items as object);
        }


    }
}
