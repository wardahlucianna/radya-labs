using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class AttendanceSummaryRedisCacheHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTerm;
        private readonly IRedisCache _redisCache;

        public AttendanceSummaryRedisCacheHandler(IAttendanceDbContext AttendanceDbContext, IAttendanceSummaryTerm AttendanceSummaryTerm, IRedisCache redisCache)
        {
            _dbContext = AttendanceDbContext;
            _attendanceSummaryTerm = AttendanceSummaryTerm;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public static async Task<List<string>> GetLessonByUser(IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken, GetHomeroomByIdUserRequest param, IRedisCache redisCache)
        {
            List<string> IdLesson = new List<string>();

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel,
            };

            var redisHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetHomeroomStudentEnrollment(paramRedis, redisCache, _dbContext, CancellationToken);
            var redisHomeroomTeacher = await AttendanceSummaryRedisCacheHandler.GetHomeroomTeacher(paramRedis, redisCache, _dbContext, CancellationToken);
            var redisSchedule = await AttendanceSummaryRedisCacheHandler.GetSchedule(paramRedis, redisCache, _dbContext, CancellationToken);
            var redisNonTeachingLoad = await AttendanceSummaryRedisCacheHandler.GetNonTeachingLoad(paramRedis, redisCache, _dbContext, CancellationToken);
            var redisDepartmentLevel = await AttendanceSummaryRedisCacheHandler.GetDepartmentLevel(paramRedis, redisCache, _dbContext, CancellationToken);
            #endregion

            var queryHomeroomStudentEnrollment = redisHomeroomStudentEnrollment.Distinct();

            if (!string.IsNullOrEmpty(param.IdClassroom))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.IdClassroom == param.IdClassroom);
            if (!string.IsNullOrEmpty(param.IdLevel))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.Level.Id == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.Grade.Id == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.IdClassroom == param.IdClassroom);
            if (!string.IsNullOrEmpty(param.IdSubject))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.IdSubject == param.IdSubject);

            if (!string.IsNullOrEmpty(param.ClassId))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.ClassId == param.ClassId);

            var GetHomeroomStudentEnrollment = queryHomeroomStudentEnrollment
                                                .GroupBy(e => new
                                                {
                                                    e.IdLesson,
                                                    idSubject = e.IdSubject,
                                                    idHomeroom = e.Homeroom.Id,
                                                    idGrade = e.Grade.Id,
                                                    idLevel = e.Level.Id,
                                                })
                                                .Select(e => new
                                                {
                                                    e.Key.IdLesson,
                                                    e.Key.idHomeroom,
                                                    e.Key.idGrade,
                                                    e.Key.idLevel,
                                                    e.Key.idSubject
                                                })
                                                .ToList();

            var listIdLesson = GetHomeroomStudentEnrollment.Select(e => e.IdLesson).Distinct().ToList();
            var idLevel = GetHomeroomStudentEnrollment.Select(e => e.idLevel).Distinct().FirstOrDefault();

            //HomeroomTeacher
            if (param.SelectedPosition == null || param.SelectedPosition.ToLower() == "all")
            {
                IdLesson.AddRange(listIdLesson);

            }
            else if (param.SelectedPosition == PositionConstant.ClassAdvisor || param.SelectedPosition == PositionConstant.CoTeacher)
            {
                var HomeroomTeacher = redisHomeroomTeacher
                .Where(x => x.Teacher.IdUser == param.IdUser && x.Position.Code == param.SelectedPosition)
                .Select(e => e.IdHomeroom)
                .Distinct().ToList();

                IdLesson.AddRange(GetHomeroomStudentEnrollment.Where(e => HomeroomTeacher.Contains(e.idHomeroom)).Select(e => e.IdLesson).ToList());
            }
            else if (param.SelectedPosition == PositionConstant.SubjectTeacher)
            {
                var GetSubjetTeacher = redisSchedule
                                    .Where(x => x.Teacher.IdUser == param.IdUser && listIdLesson.Contains(x.IdLesson))
                                    .Select(e => e.IdLesson)
                                    .Distinct()
                                    .ToList();
                IdLesson.AddRange(GetSubjetTeacher);
            }
            else
            {
                var GetTeacherNonTeaching = redisNonTeachingLoad
                                 .Where(x => x.IdUserTeacher == param.IdUser && x.PositionCode == param.SelectedPosition)
                                 .ToList();

                foreach (var item in GetTeacherNonTeaching)
                {
                    var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                    _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                    if (_DepartemenPosition != null)
                    {
                        var getDepartmentLevelbyIdLevel = redisDepartmentLevel.Any(e => e.IdDepartment == _DepartemenPosition.Id);
                        if (getDepartmentLevelbyIdLevel)
                        {
                            var GetIdGrades = redisDepartmentLevel.Where(e => e.IdDepartment == _DepartemenPosition.Id)
                                .Select(e => e.IdGrade)
                                .ToList();

                            var IdLessonByGrade = GetHomeroomStudentEnrollment.Where(e => GetIdGrades.Contains(e.idGrade)).Select(e => e.IdLesson).ToList();
                            IdLesson.AddRange(IdLessonByGrade);
                        }
                    }


                    //ByGrade or Level
                    _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                    _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                    _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
                    if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null)
                    {
                        var IdLessonBySubject = GetHomeroomStudentEnrollment.Where(e => e.idSubject == _SubjectPosition.Id).Select(e => e.IdLesson).ToList();
                        if (IdLessonBySubject.Any())
                        {

                        }
                        IdLesson.AddRange(IdLessonBySubject);

                    }
                    else if (_SubjectPosition == null && _GradePosition != null && _LevelPosition != null)
                    {
                        var IdLessonByGrade = GetHomeroomStudentEnrollment.Where(e => e.idGrade == _GradePosition.Id).Select(e => e.IdLesson).ToList();
                        IdLesson.AddRange(IdLessonByGrade);
                    }
                    else if (_SubjectPosition == null && _GradePosition == null && _LevelPosition != null)
                    {
                        var IdLessonByGrade = GetHomeroomStudentEnrollment.Where(e => e.idLevel == _LevelPosition.Id).Select(e => e.IdLesson).ToList();
                        IdLesson.AddRange(IdLessonByGrade);
                    }

                }
            }

            IdLesson = IdLesson.Distinct().ToList();

            return IdLesson;
        }
        public static async Task<List<GetHomeroom>> GetHomeroomStudentEnrollment(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-HomeroomStudentEnrollment-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            //var listHomeroomStudentEnrollment = await _redisCache.GetAsync<List<GetHomeroom>>(key);
            var listHomeroomStudentEnrollment = await _redisCache.GetListByPatternAsync<GetHomeroom>(key);

            if (listHomeroomStudentEnrollment == null)
            {
                var listPeriod = await GetPeriod(param, _redisCache, _dbContext, CancellationToken);
                var queryHomeroomStudentEnrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                    .Include(e => e.Lesson)
                    .Include(e => e.Subject)
                    .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.Homeroom.Grade.IdLevel == param.IdLevel);

                listHomeroomStudentEnrollment = await queryHomeroomStudentEnrollment
                    .AsNoTracking()
                    .GroupBy(e => new GetHomeroom
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

                listHomeroomStudentEnrollment.ForEach(e =>
                {
                    e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).DefaultIfEmpty().Min();
                    e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).DefaultIfEmpty().Min();
                });

                await _redisCache.SetListAsync(key, listHomeroomStudentEnrollment, TimeSpan.FromMinutes(5));
                //await _redisCache.SetAsync(key, listHomeroomStudentEnrollment, TimeSpan.FromMinutes(5));
            }

            return listHomeroomStudentEnrollment;
        }

        public static async Task<List<GetHomeroom>> GetTrHomeroomStudentEnrollment(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-TrHomeroomStudentEnrollment-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listHomeroomStudentEnrollment = await _redisCache.GetListByPatternAsync<GetHomeroom>(key);

            if (listHomeroomStudentEnrollment == null)
            {
                var queryHomeroomStudentEnrollment = _dbContext.Entity<TrHomeroomStudentEnrollment>()
                   .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                   .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                   .Include(e => e.LessonNew)
                   .Include(e => e.SubjectNew)
                   .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.Homeroom.Grade.IdLevel == param.IdLevel);

                listHomeroomStudentEnrollment = await queryHomeroomStudentEnrollment
                    .AsNoTracking()
                    .GroupBy(e => new GetHomeroom
                    {
                        IdLesson = e.IdLessonNew,
                        IdHomeroomStudent = e.IdHomeroomStudent,
                        ClassId = e.LessonNew.ClassIdGenerated,
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
                        IdSubject = e.SubjectNew.Id,
                        SubjectCode = e.SubjectNew.Code,
                        SubjectName = e.SubjectNew.Description,
                        SubjectID = e.SubjectNew.SubjectID,
                        IsDelete = e.IsDelete,
                        IsFromMaster = false,
                        EffectiveDate = e.StartDate,
                        IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                        Datein = e.DateIn
                    })
                    .Select(e => e.Key)
                    .ToListAsync(CancellationToken);

                await _redisCache.SetListAsync(key, listHomeroomStudentEnrollment, TimeSpan.FromMinutes(5));
            }

            return listHomeroomStudentEnrollment;
        }

        public static async Task<List<RedisAttendanceSummaryHomeroomTeacherResult>> GetHomeroomTeacher(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-HomeroomTeacher-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listHomeroomTeacher = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryHomeroomTeacherResult>(key);

            if (listHomeroomTeacher == null)
            {
                var queryHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                                    .Include(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                                    .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                                    .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                    .Include(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                                    .Include(e => e.Staff)
                                    .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Grade.IdLevel == param.IdLevel);

                listHomeroomTeacher = await queryHomeroomTeacher
                                        .GroupBy(e => new RedisAttendanceSummaryHomeroomTeacherResult
                                        {
                                            IdHomeroom = e.Homeroom.Id,
                                            IdGrade = e.Homeroom.IdGrade,
                                            IdClassroom = e.Homeroom.GradePathwayClassroom.IdClassroom,
                                            IsAttendance = e.IsAttendance,
                                            Teacher = new RedisAttendanceSummaryTeacher
                                            {
                                                IdUser = e.IdBinusian,
                                                FirstName = e.Staff.FirstName,
                                                LastName = e.Staff.LastName,
                                            },
                                            Position = new CodeWithIdVm
                                            {
                                                Id = e.TeacherPosition.LtPosition.Id,
                                                Code = e.TeacherPosition.LtPosition.Code,
                                                Description = e.TeacherPosition.LtPosition.Description
                                            }
                                        })
                                        .Select(e => e.Key)
                                        .ToListAsync(CancellationToken); ;

                await _redisCache.SetListAsync(key, listHomeroomTeacher, TimeSpan.FromMinutes(5));
            }

            return listHomeroomTeacher;
        }

        public static async Task<List<RedisAttendanceSummaryScheduleResult>> GetSchedule(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-Schedule-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listSchedule = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryScheduleResult>(key);

            if (listSchedule == null)
            {
                var querySchedule = _dbContext.Entity<MsSchedule>()
                                    .Include(e => e.User)
                                    .Include(e => e.Lesson)
                                    .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    querySchedule = querySchedule.Where(e => e.Lesson.Grade.IdLevel == param.IdLevel);


                listSchedule = await querySchedule
                                .GroupBy(e => new RedisAttendanceSummaryScheduleResult
                                {
                                    IdLesson = e.IdLesson,
                                    Teacher = new RedisAttendanceSummaryTeacher
                                    {
                                        IdUser = e.IdUser,
                                        FirstName = e.User.FirstName,
                                        LastName = e.User.LastName
                                    },
                                    IdWeek = e.IdWeek,
                                    IdDay = e.IdDay,
                                    IdSession = e.IdSession,
                                })
                                .Select(e => e.Key)
                                .ToListAsync(CancellationToken);

                await _redisCache.SetListAsync(key, listSchedule, TimeSpan.FromMinutes(5));
            }

            return listSchedule;
        }

        public static async Task<List<RedisAttendanceSummaryNonTeachingLoadResult>> GetNonTeachingLoad(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-NonTeachinLoad-{param.IdAcademicYear}";
            //var listNonTeachingLoad = await _redisCache.GetAsync<List<RedisAttendanceSummaryNonTeachingLoadResult>>(key);
            var listNonTeachingLoad = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryNonTeachingLoadResult>(key);

            if (listNonTeachingLoad == null)
            {
                var queryNonTeachingLoad = _dbContext.Entity<TrNonTeachingLoad>()
                                     .Include(e => e.NonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                                     .Where(x => x.Data != null && x.NonTeachingLoad.IdAcademicYear == param.IdAcademicYear);

                listNonTeachingLoad = await queryNonTeachingLoad
                                .GroupBy(e => new RedisAttendanceSummaryNonTeachingLoadResult
                                {
                                    IdUserTeacher = e.IdUser,
                                    PositionCode = e.NonTeachingLoad.TeacherPosition.LtPosition.Code,
                                    Data = e.Data
                                })
                                .Select(e => e.Key)
                                .ToListAsync(CancellationToken);

                //await _redisCache.SetAsync(key, listNonTeachingLoad, TimeSpan.FromMinutes(5));
                await _redisCache.SetListAsync(key, listNonTeachingLoad, TimeSpan.FromMinutes(5));
            }

            return listNonTeachingLoad;
        }

        public static async Task<List<RedisAttendanceSummaryDepartmentLevelResult>> GetDepartmentLevel(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-DepartmentLevel-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listDepartmentLevel = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryDepartmentLevelResult>(key);

            if (listDepartmentLevel == null)
            {
                var queryDepartmentLevel = _dbContext.Entity<MsDepartmentLevel>()
                                             .Include(e => e.Department)
                                             .Where(e => e.Department.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryDepartmentLevel = queryDepartmentLevel.Where(e => e.IdLevel == param.IdLevel);

                listDepartmentLevel = await queryDepartmentLevel
                                .SelectMany(e => e.Level.Grades.Select(f => new RedisAttendanceSummaryDepartmentLevelResult
                                {
                                    IdDepartment = e.Id,
                                    IdGrade = f.Id
                                }))
                                .GroupBy(e => new RedisAttendanceSummaryDepartmentLevelResult
                                {
                                    IdDepartment = e.IdDepartment,
                                    IdGrade = e.IdGrade,
                                })
                                .Select(e => e.Key)
                                .ToListAsync(CancellationToken);

                await _redisCache.SetListAsync(key, listDepartmentLevel, TimeSpan.FromMinutes(5));
            }

            return listDepartmentLevel;
        }

        public static async Task<List<RedisAttendanceSummaryStudentStatusResult>> GetStudentStatus(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken, DateTime datetime)
        {

            var key = $"ListSummaryAttendanceTerm-StudentStatus-{param.IdAcademicYear}";
            var listStudentStatus = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryStudentStatusResult>(key);

            if (listStudentStatus == null)
            {
                var getStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                                            .Where(e => e.IdAcademicYear == param.IdAcademicYear && e.ActiveStatus).ToListAsync(CancellationToken);

                //var listPeriod = await GetPeriod(param, _redisCache, _dbContext, CancellationToken);
                listStudentStatus = getStudentStatus
                                .GroupBy(e => new RedisAttendanceSummaryStudentStatusResult
                                {
                                    IdStudent = e.IdStudent,
                                    StartDate = e.StartDate,
                                    EndDate = e.EndDate == null
                                                ? datetime.Date
                                                : Convert.ToDateTime(e.EndDate),
                                })
                                .Select(e => e.Key).ToList();

                await _redisCache.SetListAsync(key, listStudentStatus, TimeSpan.FromMinutes(5));
            }

            return listStudentStatus;
        }

        public static async Task<List<RedisAttendanceSummaryAttendanceEntryResult>> GetAttendanceEntry(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken, DateTime datetime)
        {

            var key = $"ListSummaryAttendanceTerm-AttendanceEntry-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listAttendanceEntry = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryAttendanceEntryResult>(key);

            if (listAttendanceEntry == null)
            {
                var listStudentStatus = await GetStudentStatus(param, _redisCache, _dbContext, CancellationToken, datetime);
                var queryAttendanceEntry = _dbContext.Entity<TrAttendanceEntryV2>()
                                               .Include(e => e.ScheduleLesson).ThenInclude(e => e.Session)
                                               .Include(e => e.ScheduleLesson).ThenInclude(e => e.Subject)
                                               .Include(e => e.ScheduleLesson).ThenInclude(e => e.Lesson)
                                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                                                    .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                               .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                                               .Include(e => e.AttendanceEntryWorkhabitV2s)
                                               .Where(e => e.ScheduleLesson.IdAcademicYear == param.IdAcademicYear && e.ScheduleLesson.IsGenerated == true);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryAttendanceEntry = queryAttendanceEntry.Where(e => e.ScheduleLesson.IdLevel == param.IdLevel);

                var GetAttendanceEntry = await queryAttendanceEntry.ToListAsync(CancellationToken);

                List<TrAttendanceEntryV2> attendanceStudentNoActive = new List<TrAttendanceEntryV2>();
                foreach (var itemAttendanceEntry in GetAttendanceEntry)
                {
                    var listStatusStudentByDate = listStudentStatus
                                                    .Where(e => e.StartDate.Date <= itemAttendanceEntry.ScheduleLesson.ScheduleDate.Date
                                                                && e.EndDate.Date >= itemAttendanceEntry.ScheduleLesson.ScheduleDate.Date
                                                                && e.IdStudent == itemAttendanceEntry.HomeroomStudent.IdStudent)
                                                    .Any();
                    if (!listStatusStudentByDate)
                        attendanceStudentNoActive.Add(itemAttendanceEntry);
                }

                if (attendanceStudentNoActive.Any())
                {
                    var idAttendanceEntryStudentNoActive = attendanceStudentNoActive.Select(e => e.IdAttendanceEntry).ToList();
                    GetAttendanceEntry = GetAttendanceEntry.Where(e => !idAttendanceEntryStudentNoActive.Contains(e.IdAttendanceEntry)).ToList();
                }

                listAttendanceEntry = GetAttendanceEntry
                               .GroupBy(e => new RedisAttendanceSummaryAttendanceEntryResult
                               {
                                   IdScheduleLesson = e.IdScheduleLesson,
                                   IdHomeroomStudent = e.IdHomeroomStudent,
                                   ScheduleDate = e.ScheduleLesson.ScheduleDate,
                                   IdLesson = e.ScheduleLesson.IdLesson,
                                   ClassID = e.ScheduleLesson.ClassID,
                                   IdGrade = e.ScheduleLesson.IdGrade,
                                   IdDay = e.ScheduleLesson.IdDay,
                                   IdWeek = e.ScheduleLesson.IdWeek,
                                   IdAcademicYear = e.ScheduleLesson.IdAcademicYear,
                                   IdLevel = e.ScheduleLesson.IdLevel,
                                   IdStudent = e.HomeroomStudent.IdStudent,
                                   Session = new RedisAttendanceSummarySession
                                   {
                                       Id = e.ScheduleLesson.Session.Id,
                                       Name = e.ScheduleLesson.Session.Name,
                                       SessionID = e.ScheduleLesson.Session.SessionID.ToString()
                                   },
                                   Subject = new RedisAttendanceSummarySubject
                                   {
                                       Id = e.ScheduleLesson.Subject.Id,
                                       Code = e.ScheduleLesson.Subject.SubjectID,
                                       Description = e.ScheduleLesson.Subject.Description,
                                       SubjectID = e.ScheduleLesson.Subject.SubjectID,
                                   },
                                   Status = e.Status,
                                   Attendance = new RedisAttendanceSummaryAttendance
                                   {
                                       Id = e.AttendanceMappingAttendance.Attendance.Id,
                                       Code = e.AttendanceMappingAttendance.Attendance.Code,
                                       Description = e.AttendanceMappingAttendance.Attendance.Description,
                                       AbsenceCategory = e.AttendanceMappingAttendance.Attendance.AbsenceCategory,
                                       ExcusedAbsenceCategory = e.AttendanceMappingAttendance.Attendance.ExcusedAbsenceCategory
                                   },
                                   Notes = e.Notes,
                                   IdUserTeacher = e.IdBinusian,
                                   Student = new RedisAttendanceSummaryStudent
                                   {
                                       IdStudent = e.HomeroomStudent.Student.Id,
                                       FirstName = e.HomeroomStudent.Student.FirstName,
                                       LastName = e.HomeroomStudent.Student.LastName,
                                       MiddleName = e.HomeroomStudent.Student.MiddleName,
                                   },
                                   IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                                   IdUserAttendanceEntry = e.UserIn,
                                   IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                                   AttendanceEntryWorkhabit = e.AttendanceEntryWorkhabitV2s.Select(e => new GetAttendanceEntryWorkhabitV2
                                   {
                                       IdAttendanceEntry = e.IdAttendanceEntry,
                                       IdMappingAttendanceWorkhabit = e.IdMappingAttendanceWorkhabit
                                   }).ToList(),
                                   Semester = e.ScheduleLesson.Lesson.Semester,
                                   GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                                   Classroom = new CodeWithIdVm
                                   {
                                       Id = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                                       Description = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                                       Code = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                   }
                               })
                               .Select(e => e.Key)
                               .ToList();

                await _redisCache.SetListAsync(key, listAttendanceEntry, TimeSpan.FromMinutes(5));
            }

            return listAttendanceEntry;
        }

        public static async Task<List<RedisAttendanceSummaryScheduleLessonResult>> GetScheduleLesson(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken, DateTime dateTime)
        {

            var key = $"ListSummaryAttendanceTerm-ScheduleLesson-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listScheduleLesson = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryScheduleLessonResult>(key);

            if (listScheduleLesson == null)
            {
                var queryScheduleLesson = _dbContext.Entity<MsScheduleLesson>()
                                                .Include(e => e.Subject)
                                                .Include(e => e.Session)
                                                .Include(e => e.Lesson)
                                                .Where(e => e.IdAcademicYear == param.IdAcademicYear && e.ScheduleDate.Date <= dateTime.Date && e.IsGenerated == true);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryScheduleLesson = queryScheduleLesson.Where(e => e.IdLevel == param.IdLevel);

                listScheduleLesson = await queryScheduleLesson
                                       .GroupBy(e => new RedisAttendanceSummaryScheduleLessonResult
                                       {
                                           Id = e.Id,
                                           ScheduleDate = e.ScheduleDate,
                                           IdLesson = e.IdLesson,
                                           ClassID = e.ClassID,
                                           Session = new RedisAttendanceSummarySession
                                           {
                                               Id = e.Session.Id,
                                               Name = e.Session.Name,
                                               SessionID = e.Session.SessionID.ToString()
                                           },
                                           IdGrade = e.IdGrade,
                                           IdDay = e.IdDay,
                                           IdWeek = e.IdWeek,
                                           IdAcademicYear = e.IdAcademicYear,
                                           IdLevel = e.IdLevel,
                                           Subject = new RedisAttendanceSummarySubject
                                           {
                                               Id = e.Subject.Id,
                                               Code = e.Subject.Code,
                                               Description = e.Subject.Description,
                                               SubjectID = e.Subject.SubjectID,
                                           },
                                           Semester = e.Lesson.Semester
                                       })
                                        .Select(e => e.Key)
                                    .ToListAsync(CancellationToken);


                await _redisCache.SetListAsync(key, listScheduleLesson, TimeSpan.FromMinutes(5));
            }

            return listScheduleLesson;
        }

        public static async Task<List<RedisAttendanceSummaryMappingAttendanceResult>> GetMappingAttendance(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-MappingAttendance-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listMappingAttendance = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryMappingAttendanceResult>(key);

            if (listMappingAttendance == null)
            {
                var queryMappingAttendance = _dbContext.Entity<MsMappingAttendance>()
                               .Where(e => e.Level.IdAcademicYear == param.IdAcademicYear)
                               ;

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryMappingAttendance = queryMappingAttendance.Where(e => e.IdLevel == param.IdLevel);

                listMappingAttendance = await queryMappingAttendance
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


                await _redisCache.SetListAsync(key, listMappingAttendance, TimeSpan.FromMinutes(5));
            }

            return listMappingAttendance;
        }

        public static async Task<List<RedisAttendanceSummaryPeriodResult>> GetPeriod(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-Period-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listPeriod = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryPeriodResult>(key);

            if (listPeriod == null)
            {
                var queryPeriod = _dbContext.Entity<MsPeriod>()
                               .Include(x=> x.Grade).ThenInclude(x=> x.Level).ThenInclude(x=> x.AcademicYear)
                               .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                               ;

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryPeriod = queryPeriod.Where(e => e.Grade.IdLevel == param.IdLevel);

                listPeriod = await queryPeriod
                                       .GroupBy(e => new RedisAttendanceSummaryPeriodResult
                                       {
                                           Id = e.Id,
                                           IdGrade = e.IdGrade,
                                           StartDate = e.StartDate,
                                           EndDate = e.EndDate,
                                           Semester = e.Semester,
                                           IdLevel = e.Grade.IdLevel,
                                           AttendanceStartDate = e.AttendanceStartDate,
                                           AttendanceEndDate = e.AttendanceEndDate,
                                           IdSchool = e.Grade.Level.AcademicYear.IdSchool
                                       })
                                        .Select(e => e.Key)
                                        .ToListAsync(CancellationToken);


                await _redisCache.SetListAsync(key, listPeriod, TimeSpan.FromMinutes(5));
            }

            return listPeriod;
        }

        public static async Task<List<RedisAttendanceSummaryLessonTeacherResult>> GetLessonTeacher(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-LessonTeacher-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var listLessonTeacher = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryLessonTeacherResult>(key);

            if (listLessonTeacher == null)
            {
                var queryLessonTeacher = _dbContext.Entity<MsLessonTeacher>()
                             .Include(e => e.Lesson)
                               .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear && e.IsAttendance)
                               ;

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryLessonTeacher = queryLessonTeacher.Where(e => e.Lesson.Grade.IdLevel == param.IdLevel);

                listLessonTeacher = await queryLessonTeacher
                                       .GroupBy(e => new RedisAttendanceSummaryLessonTeacherResult
                                       {
                                           IdUserTeacher = e.IdUser,
                                           IdLesson = e.IdLesson,
                                           ClassId = e.Lesson.ClassIdGenerated,
                                           IsAttendance = e.IsAttendance
                                       })
                                        .Select(e => e.Key)
                                        .ToListAsync(CancellationToken);


                await _redisCache.SetListAsync(key, listLessonTeacher, TimeSpan.FromMinutes(5));
            }

            return listLessonTeacher;
        }

        public static async Task<MsFormula> GetFormula(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-Formula-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var getFormula = await _redisCache.GetAsync<MsFormula>(key);

            if (getFormula == null)
            {
                var queryFormula = _dbContext.Entity<MsFormula>()
                               .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryFormula = queryFormula.Where(e => e.IdLevel == param.IdLevel);

                getFormula = await queryFormula.FirstOrDefaultAsync(CancellationToken);

                await _redisCache.SetAsync(key, getFormula, TimeSpan.FromMinutes(5));
            }

            return getFormula;
        }

        public static async Task<List<RedisAttendanceSummaryUserResult>> GetUser(IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-User";

            var getUser = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryUserResult>(key);

            if (getUser == null)
            {
                var queryUser = _dbContext.Entity<MsUser>();
                getUser = await queryUser
                            .Select(e => new RedisAttendanceSummaryUserResult
                            {
                                Id = e.Id,
                                DisplayName = e.DisplayName
                            })
                            .ToListAsync(CancellationToken);

                await _redisCache.SetListAsync(key, getUser, TimeSpan.FromMinutes(5));
            }

            return getUser;
        }

        public static async Task<List<RedisAttendanceSummaryAttendance>> GetAttendance(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {

            var key = $"ListSummaryAttendanceTerm-Attendance-{param.IdAcademicYear}";

            var getAttendance = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryAttendance>(key);

            if (getAttendance == null)
            {
                var queryAttendance = _dbContext.Entity<MsAttendance>()
                               .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                               .Select(e => new RedisAttendanceSummaryAttendance
                               {
                                   Id = e.Id,
                                   Description = e.Description,
                                   Code = e.Code,
                                   AbsenceCategory = e.AbsenceCategory,
                                   ExcusedAbsenceCategory = e.ExcusedAbsenceCategory
                               });

                getAttendance = await queryAttendance.ToListAsync(CancellationToken);

                await _redisCache.SetListAsync(key, getAttendance, TimeSpan.FromMinutes(5));
            }

            return getAttendance;
        }

        public static async Task<List<RedisAttendanceSummaryAttendanceMappingAttendanceResult>> GetAttendanceMappingAttendance(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {
            var key = $"ListSummaryAttendanceTerm-AttendanceMappingAttendance-{param.IdAcademicYear}";
            if (!string.IsNullOrEmpty(param.IdLevel))
                key += $"-{param.IdLevel}";

            var getAttendanceMappingAttendance = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryAttendanceMappingAttendanceResult>(key);
            if (getAttendanceMappingAttendance == null)
            {
                var queryAttendanceMappingAttendance = _dbContext.Entity<MsAttendanceMappingAttendance>()
                                .Include(x => x.MappingAttendance)
                                .Include(x => x.Attendance)
                                .Where(x => x.Attendance.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryAttendanceMappingAttendance = queryAttendanceMappingAttendance.Where(e => e.MappingAttendance.IdLevel == param.IdLevel);

                getAttendanceMappingAttendance = await queryAttendanceMappingAttendance
                                                .Select(e => new RedisAttendanceSummaryAttendanceMappingAttendanceResult
                                                {
                                                    Id = e.Id,
                                                    AbsenceCategory = e.Attendance.AbsenceCategory,
                                                })
                                                .ToListAsync(CancellationToken);

                await _redisCache.SetListAsync(key, getAttendanceMappingAttendance, TimeSpan.FromMinutes(5));
            }

            return getAttendanceMappingAttendance;
        }

        public static async Task<List<RedisAttendanceSummaryTermResult>> GetAttendanceSummaryTerm(RedisAttendanceSummaryRequest param, IRedisCache _redisCache, IAttendanceDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {
            //var key = $"ListSummaryAttendanceTerm-AttendanceSummaryTerm-{param.IdAcademicYear}";
            //if (!string.IsNullOrEmpty(param.IdLevel))
            //    key += $"-{param.IdLevel}";

            //var getAttendanceSummaryTerm = await _redisCache.GetListByPatternAsync<RedisAttendanceSummaryTermResult>(key);
            //if (getAttendanceSummaryTerm == null)
            //{
                var getAttendanceSummaryTerm = new List<RedisAttendanceSummaryTermResult>();

                var queryAttendanceSummaryTerm = _dbContext.Entity<TrAttendanceSummaryTerm>()
                                .Include(x => x.Level)
                                .Include(x => x.Grade)
                                .Include(x => x.AcademicYear)
                                .Include(x => x.Homeroom).ThenInclude(e => e.Grade)
                                .Include(x => x.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Include(x => x.Student)
                                .Where(x => x.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryAttendanceSummaryTerm = queryAttendanceSummaryTerm.Where(e => e.IdLevel == param.IdLevel);

                getAttendanceSummaryTerm = await queryAttendanceSummaryTerm
                                                .Select(e => new RedisAttendanceSummaryTermResult
                                                {
                                                    AcademicYear = new CodeWithIdVm
                                                    {
                                                        Id = e.AcademicYear.Id,
                                                        Code = e.AcademicYear.Code,
                                                        Description = e.AcademicYear.Description,
                                                    },
                                                    Level = new CodeWithIdVm
                                                    {
                                                        Id = e.Level.Id,
                                                        Code = e.Level.Code,
                                                        Description = e.Level.Description,
                                                    },
                                                    Grade = new CodeWithIdVm
                                                    {
                                                        Id = e.Grade.Id,
                                                        Code = e.Grade.Code,
                                                        Description = e.Grade.Description,
                                                    },
                                                    Homeroom = new ItemValueVm
                                                    {
                                                        Id = e.IdHomeroom,
                                                        Description = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code
                                                    },
                                                    Student = new RedisAttendanceSummaryStudent
                                                    {
                                                        IdStudent = e.Student.Id,
                                                        FirstName = e.Student.FirstName,
                                                        MiddleName = e.Student.MiddleName,
                                                        LastName = e.Student.LastName,
                                                    },
                                                    AttendanceWorkhabitName = e.AttendanceWorkhabitName,
                                                    AttendanceWorkhabitType = e.AttendanceWorkhabitType,
                                                    Total = e.Total
                                                })
                                                .ToListAsync(CancellationToken);

            //    await _redisCache.SetListAsync(key, getAttendanceSummaryTerm, TimeSpan.FromMinutes(5));
            //}

            return getAttendanceSummaryTerm;
        }
    }
}
