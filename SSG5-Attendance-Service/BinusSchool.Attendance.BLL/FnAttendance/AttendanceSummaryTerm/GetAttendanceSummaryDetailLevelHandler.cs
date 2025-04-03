using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailLevelHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;
        public GetAttendanceSummaryDetailLevelHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailLevelRequest>();
            var columns = new[] { "grade", "class", "homeroomTeacherName", "unsubmited", "pending" };

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
            var redisPeriod = await AttendanceSummaryRedisCacheHandler.GetPeriod(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisHomeroomTeacher = await AttendanceSummaryRedisCacheHandler.GetHomeroomTeacher(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var idGrades = await _dbContext.Entity<MsHomeroom>()
                        .Where(x => IdHomerooms.Contains(x.Id))
                        .Select(e => e.IdGrade)
                                 .ToListAsync(CancellationToken);

            var GetPeriod = redisPeriod.Distinct();

            if (!GetPeriod.Any())
                throw new BadRequestException("Period is not found");

            var GetMappingAttendance = redisMappingAttendance.FirstOrDefault();

            if (GetMappingAttendance == null)
                throw new BadRequestException("Attendance mapping is not found");

            var listIdStudentActive = redisStudentStatus
                                         //.Where(x => x.StartDate.Date <= _datetime.ServerTime.Date
                                         //       && x.EndDate.Date >= _datetime.ServerTime.Date)
                                         .Select(e => e.IdStudent)
                                         .ToList();

            var Query = _dbContext.Entity<TrAttendanceSummaryTerm>()
            .Include(e => e.Level)
            .Include(e => e.AcademicYear)
            .Include(e => e.Grade)
            .Include(e => e.Student)
            .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
            .Where(x => idGrades.Contains(x.IdGrade)
                    && x.IdAcademicYear == param.IdAcademicYear
                    && x.IdLevel == param.IdLevel
                    && listIdStudentActive.Contains(x.IdStudent));

            if (!string.IsNullOrEmpty(param.IdGrade))
                Query = Query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdPeriod))
                Query = Query.Where(x => x.IdPeriod == param.IdPeriod);
            if (!string.IsNullOrEmpty(param.IdClassroom))
                Query = Query.Where(x => x.Homeroom.GradePathwayClassroom.IdClassroom == param.IdClassroom);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                Query = Query.Where(x => x.Homeroom.Semester == param.Semester);

            var DataAttendanceSummaryTerm = await Query.ToListAsync(CancellationToken);


            var DataAttendanceSummaryTermByLevel = DataAttendanceSummaryTerm.Select(e => new
            {
                idGrade = e.IdGrade,
                Grade = e.Grade.Description,
                idClassroom = e.Homeroom.GradePathwayClassroom.IdClassroom,
                Homeroom = e.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                Idhomeroom = e.IdHomeroom
            }).Distinct().ToList();

            var ListIdGrade = DataAttendanceSummaryTermByLevel.Select(e => e.idGrade).ToList();
            var ListIdClassroom = DataAttendanceSummaryTermByLevel.Select(e => e.idClassroom).ToList();

            var listHomeroomTeacher = redisHomeroomTeacher
                                        .Where(x => ListIdGrade.Contains(x.IdGrade)
                                                    && ListIdClassroom.Contains(x.IdClassroom)
                                                    && x.IsAttendance)
                                        .Select(e => new
                                        {
                                            idGrade = e.IdGrade,
                                            idClassroom = e.IdClassroom,
                                            idUserTeacher = e.Teacher.IdUser,
                                            teacherName = NameUtil.GenerateFullName(e.Teacher.FirstName, e.Teacher.LastName),
                                            IdHomeroom = e.IdHomeroom,
                                        })
                                        .Distinct().ToList();

            List<GetAttendanceSummaryDetailLevelResult> dataAttendanceSummary = new List<GetAttendanceSummaryDetailLevelResult>();
            foreach (var itemAttendanceSummary in DataAttendanceSummaryTermByLevel)
            {
                var teacher = listHomeroomTeacher.Where(f => f.idClassroom == itemAttendanceSummary.idClassroom && f.idGrade == itemAttendanceSummary.idGrade).Any()
                                            ? listHomeroomTeacher
                                    .Where(f => f.idClassroom == itemAttendanceSummary.idClassroom && f.idGrade == itemAttendanceSummary.idGrade)
                                                .Select(f => new CodeWithIdVm
                                                {
                                                    Id = f.idUserTeacher,
                                                    Description = f.teacherName
                                                })
                                                .FirstOrDefault()
                                : default;

                var unsubmited = DataAttendanceSummaryTerm
                                    .Where(f => 
                                            //f.Homeroom.GradePathwayClassroom.IdClassroom == itemAttendanceSummary.idClassroom
                                            f.IdHomeroom == itemAttendanceSummary.Idhomeroom
                                            && f.IdGrade == itemAttendanceSummary.idGrade
                                            && f.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceStatus
                                            && f.AttendanceWorkhabitName == "Unsubmitted")
                                    .Sum(f => f.Total);

                var a = DataAttendanceSummaryTerm
                                    .Where(f =>
                                            //f.Homeroom.GradePathwayClassroom.IdClassroom == itemAttendanceSummary.idClassroom
                                            f.IdHomeroom == itemAttendanceSummary.Idhomeroom
                                            && f.IdGrade == itemAttendanceSummary.idGrade
                                            && f.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceStatus
                                            && f.AttendanceWorkhabitName == "Unsubmitted"
                                            && f.Total > 0)
                                    .ToList();

                var pending = DataAttendanceSummaryTerm
                                   .Where(f => f.Homeroom.GradePathwayClassroom.IdClassroom == itemAttendanceSummary.idClassroom
                                           && f.IdGrade == itemAttendanceSummary.idGrade
                                           && f.AttendanceWorkhabitType == TrAttendanceSummaryTermType.AttendanceStatus
                                           && f.AttendanceWorkhabitName == "Pending")
                                   .Sum(f => f.Total);

                if (string.IsNullOrEmpty(param.IdGrade))
                {
                    GetPeriod = GetPeriod.Where(x => x.IdLevel == param.IdLevel);
                }
                else
                {
                    GetPeriod = GetPeriod.Where(x => x.IdGrade == param.IdGrade);
                    if (!string.IsNullOrEmpty(param.IdPeriod))
                    {
                        GetPeriod = GetPeriod.Where(x => x.Id == param.IdPeriod);
                    }

                    if (!string.IsNullOrEmpty(param.Semester.ToString()))
                    {
                        GetPeriod = GetPeriod.Where(x => x.Semester == param.Semester);
                    }
                }

                var startDate = GetPeriod.Select(e => e.StartDate).Min();
                var endDate = GetPeriod.Select(e => e.EndDate).Max();

                var attendanceSummary = new GetAttendanceSummaryDetailLevelResult
                {
                    Grade = new CodeWithIdVm
                    {
                        Id = itemAttendanceSummary.idGrade,
                        Description = itemAttendanceSummary.Grade
                    },
                    Class = new CodeWithIdVm
                    {
                        Id = itemAttendanceSummary.idClassroom,
                        Description = itemAttendanceSummary.Homeroom
                    },
                    Teacher = teacher,
                    Unsubmited = unsubmited,
                    Pending = pending,
                    StartDate = startDate,
                    EndDate = endDate,
                    IdHomeroom = itemAttendanceSummary.Idhomeroom
                };

                dataAttendanceSummary.Add(attendanceSummary);
            }

            var queryAttendanceSummary = dataAttendanceSummary.Distinct();
            if (!string.IsNullOrEmpty(param.Search))
                queryAttendanceSummary = queryAttendanceSummary.Where(e => e.Teacher.Description.ToLower().Contains(param.Search.ToLower()));

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = queryAttendanceSummary
                    .ToList();
            }
            else
            {
                items = queryAttendanceSummary
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryAttendanceSummary.Select(x => x.Grade.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
