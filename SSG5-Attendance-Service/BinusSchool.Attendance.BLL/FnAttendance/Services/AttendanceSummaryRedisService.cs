using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Attendance.FnAttendance.Constants;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Models;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.Services
{
    public class AttendanceSummaryRedisService : IAttendanceSummaryRedisService
    {
        private readonly IRedisCache _redisCache;
        private readonly IAttendanceSummaryService _attendanceSummaryService;

        public AttendanceSummaryRedisService(IRedisCache redisCache,
            IAttendanceSummaryService attendanceSummaryService)
        {
            _redisCache = redisCache;
            _attendanceSummaryService = attendanceSummaryService;
        }

        public async Task<List<PeriodResult>> GetPeriodAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            var results =
                await _redisCache.GetAsync<List<PeriodResult>>(RedisKeyConstants.GetPeriodKey(idAcademicYear, idLevel),
                    cancellationToken);

            if (results is null)
                results = await _attendanceSummaryService.GetPeriodAsync(idAcademicYear, idLevel, cancellationToken);

            return results;
        }

        public async Task<List<HomeroomStudentEnrollmentResult>> GetHomeroomStudentEnrollmentAsync(
            string idAcademicYear,
            string idLevel, CancellationToken cancellationToken)
        {
            var results =
                await _redisCache.GetAsync<List<HomeroomStudentEnrollmentResult>>(
                    RedisKeyConstants.GetHomeroomStudentEnrollmentKey(idAcademicYear, idLevel),
                    cancellationToken);

            if (results is null)
                results = await _attendanceSummaryService.GetHomeroomStudentEnrollmentAsync(idAcademicYear, idLevel,
                    cancellationToken);

            return results;
        }

        public async Task<List<HomeroomStudentEnrollmentResult>> GetTrHomeroomStudentEnrollmentAsync(
            string idAcademicYear,
            string idLevel, CancellationToken cancellationToken)
        {
            var results =
                await _redisCache.GetAsync<List<HomeroomStudentEnrollmentResult>>(
                    RedisKeyConstants.GetHomeroomStudentEnrollmentTransactionKey(idAcademicYear, idLevel),
                    cancellationToken);

            if (results is null)
                results = await _attendanceSummaryService.GetTrHomeroomStudentEnrollmentAsync(idAcademicYear, idLevel,
                    cancellationToken);

            return results;
        }

        public async Task<List<HomeroomTeacherResult>> GetHomeroomTeacherAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            var results =
                await _redisCache.GetAsync<List<HomeroomTeacherResult>>(
                    RedisKeyConstants.GetHomeroomTeacherKey(idAcademicYear, idLevel),
                    cancellationToken);

            if (results is null)
                results = await _attendanceSummaryService.GetHomeroomTeacherAsync(idAcademicYear, idLevel,
                    cancellationToken);

            return results;
        }

        public async Task<List<ScheduleResult>> GetScheduleAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            var results =
                await _redisCache.GetAsync<List<ScheduleResult>>(
                    RedisKeyConstants.GetScheduleKey(idAcademicYear, idLevel),
                    cancellationToken);

            if (results is null)
                results = await _attendanceSummaryService.GetScheduleAsync(idAcademicYear, idLevel,
                    cancellationToken);

            return results;
        }

        public async Task<List<NonTeachingLoadResult>> GetNonTeachingLoadResultAsync(string idAcademicYear,
            CancellationToken cancellationToken)
        {
            var results =
                await _redisCache.GetAsync<List<NonTeachingLoadResult>>(
                    RedisKeyConstants.GetNonTeachingLoadKey(idAcademicYear),
                    cancellationToken);

            if (results is null)
                results = await _attendanceSummaryService.GetNonTeachingLoadResultAsync(idAcademicYear,
                    cancellationToken);

            return results;
        }

        public async Task<List<DepartmentResult>> GetDepartmentLevelResultAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            var results =
                await _redisCache.GetAsync<List<DepartmentResult>>(
                    RedisKeyConstants.GetNonTeachingLoadKey(idAcademicYear),
                    cancellationToken);

            if (results is null)
                results = await _attendanceSummaryService.GetDepartmentLevelResultAsync(idAcademicYear, idLevel,
                    cancellationToken);

            return results;
        }

        public Task<StudentStatusResult> GetStudentStatusResultAsync(string idAcademicYear, string idStudent,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<AttendanceEntryResult>> GetAttendanceEntryAsync(string idAcademicYear, string idStudent,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<ScheduleLessonResult>> GetScheduleLessonAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            var results =
                await _redisCache.GetAsync<List<ScheduleLessonResult>>(
                    RedisKeyConstants.GetScheduleLessonKey(idAcademicYear, idLevel),
                    cancellationToken);

            if (results is null)
                results = await _attendanceSummaryService.GetScheduleLessonAsync(idAcademicYear, idLevel,
                    cancellationToken);

            return results;
        }

        public Task<List<MappingAttendanceResult>> GetMappingAttendanceAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<LessonTeacherResult>> GetLessonTeacherAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<AttendanceMappingAttendanceResult>> GetAttendanceMappingAttendanceAsync(string idAcademicYear,
            string idLevel, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<CodeWithIdVm>> GetMsAttendanceMappingAttendanceAsync(string idLevel,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<CodeWithIdVm>> GetMsMappingAttendanceWorkhabitAsync(string idLevel,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<int, List<HomeroomResult>>> GetHomeroomsGroupedBySemester(string idGrade, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, List<AttendanceEntryResult>>> GetAttendanceEntriesGroupedAsync(string[] idSchedules, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, List<AttendanceEntryResult>>> GetAttendanceEntriesPendingGroupedAsync(string[] idSchedules, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<StudentEnrollmentDto2>> GetStudentEnrolledAsync(string idHomeroom, DateTime startAttendanceDt, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<StudentStatusDto>> GetStudentStatusesAsync(string[] studentIds, string idAcademicYear, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetLessonByUser(GetHomeroomByIdUserRequest param,
            CancellationToken cancellationToken)
        {
            var homeroomStudentEnrollments =
                await GetHomeroomStudentEnrollmentAsync(param.IdAcademicYear, param.IdLevel, cancellationToken);
            var homeroomStudentEnrollmentsMoving =
                await GetTrHomeroomStudentEnrollmentAsync(param.IdAcademicYear, param.IdLevel, cancellationToken);
            var homeroomTeachers =
                await GetHomeroomTeacherAsync(param.IdAcademicYear, param.IdLevel, cancellationToken);
            var schedules =
                await GetScheduleAsync(param.IdAcademicYear, param.IdLevel, cancellationToken);
            var nonTeachingLoads =
                await GetNonTeachingLoadResultAsync(param.IdAcademicYear, cancellationToken);
            var departments =
                await GetDepartmentLevelResultAsync(param.IdAcademicYear, param.IdLevel, cancellationToken);

            var queryHomeroomStudentEnrollment = homeroomStudentEnrollments.Distinct().Union(homeroomStudentEnrollmentsMoving.Distinct());

            if (!string.IsNullOrEmpty(param.IdClassroom))
                queryHomeroomStudentEnrollment =
                    queryHomeroomStudentEnrollment.Where(e => e.IdClassroom == param.IdClassroom);

            if (!string.IsNullOrEmpty(param.IdLevel))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.Level.Id == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.Grade.Id == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                queryHomeroomStudentEnrollment =
                    queryHomeroomStudentEnrollment.Where(e => e.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                queryHomeroomStudentEnrollment =
                    queryHomeroomStudentEnrollment.Where(e => e.Homeroom.Id == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.IdSubject))
                queryHomeroomStudentEnrollment =
                    queryHomeroomStudentEnrollment.Where(e => e.IdSubject == param.IdSubject);

            if (!string.IsNullOrEmpty(param.ClassId))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.ClassId == param.ClassId);

            var finalHomeroomStudentEnrollments = queryHomeroomStudentEnrollment
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

            var results = new List<string>();
            var listIdLesson = finalHomeroomStudentEnrollments.Select(e => e.IdLesson).Distinct().ToList();

            //HomeroomTeacher
            if (param.SelectedPosition == null || param.SelectedPosition.ToLower() == "all")
            {
                results.AddRange(listIdLesson);
            }
            else if (param.SelectedPosition == PositionConstant.ClassAdvisor ||
                     param.SelectedPosition == PositionConstant.CoTeacher)
            {
                var homeroomTeacher = homeroomTeachers
                    .Where(x => x.Teacher.IdUser == param.IdUser && x.Position.Code == param.SelectedPosition)
                    .Select(e => e.IdHomeroom)
                    .Distinct().ToList();

                results.AddRange(finalHomeroomStudentEnrollments.Where(e => homeroomTeacher.Contains(e.idHomeroom))
                    .Select(e => e.IdLesson).ToList());
            }
            else if (param.SelectedPosition == PositionConstant.SubjectTeacher)
            {
                var subjectTeacher = schedules
                    .Where(x => x.Teacher.IdUser == param.IdUser && listIdLesson.Contains(x.IdLesson))
                    .Select(e => e.IdLesson)
                    .Distinct()
                    .ToList();
                results.AddRange(subjectTeacher);
            }
            else
            {
                var teacherNonTeaching = nonTeachingLoads
                    .Where(x => x.IdUserTeacher == param.IdUser && x.PositionCode == param.SelectedPosition)
                    .ToList();

                foreach (var item in teacherNonTeaching)
                {
                    var dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                    dataNewPosition.TryGetValue("Department", out var departemenPosition);
                    if (departemenPosition != null)
                    {
                        var getDepartmentLevelbyIdLevel =
                            departments.Any(e => e.IdDepartment == departemenPosition.Id);
                        if (getDepartmentLevelbyIdLevel)
                        {
                            var idGrades = departments.Where(e => e.IdDepartment == departemenPosition.Id)
                                .Select(e => e.IdGrade)
                                .ToList();

                            var idLessonByGrade = finalHomeroomStudentEnrollments
                                .Where(e => idGrades.Contains(e.idGrade)).Select(e => e.IdLesson).ToList();
                            results.AddRange(idLessonByGrade);
                        }
                    }


                    //ByGrade or Level
                    dataNewPosition.TryGetValue("Grade", out var gradePosition);
                    dataNewPosition.TryGetValue("Level", out var levelPosition);
                    dataNewPosition.TryGetValue("Subject", out var subjectPosition);
                    if (subjectPosition != null && gradePosition != null && levelPosition != null)
                    {
                        var idLessonBySubject = finalHomeroomStudentEnrollments
                            .Where(e => e.idSubject == subjectPosition.Id).Select(e => e.IdLesson).ToList();
                        if (idLessonBySubject.Any())
                        {
                        }

                        results.AddRange(idLessonBySubject);
                    }
                    else if (subjectPosition == null && gradePosition != null && levelPosition != null)
                    {
                        var idLessonByGrade = finalHomeroomStudentEnrollments.Where(e => e.idGrade == gradePosition.Id)
                            .Select(e => e.IdLesson).ToList();
                        results.AddRange(idLessonByGrade);
                    }
                    else if (subjectPosition == null && gradePosition == null && levelPosition != null)
                    {
                        var idLessonByGrade = finalHomeroomStudentEnrollments.Where(e => e.idLevel == levelPosition.Id)
                            .Select(e => e.IdLesson).ToList();
                        results.AddRange(idLessonByGrade);
                    }
                }
            }

            results = results.Distinct().ToList();

            return results;
        }

        public Task<List<AttendanceEntryResult>> GetAttendanceEntryAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<AttendanceEntryResult>> GetAttendanceEntryByStudentAsync(string idAcademicYear, string idStudent, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<StudentEnrollmentDto2>> GetStudentEnrolledByStudentAsync(string idAcademicYear, string idStudent, DateTime startAttendanceDt, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MsFormula> GetFormulaAsync(string idAcademicYear, string idLevel, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<ScheduleLessonResult>> GetScheduleLessonByGradeAsync(string idAcademicYear, string idGrade, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<AttendanceEntryResult>> GetAttendanceEntryUnexcusedAbsenceAsync(string idAcademicYear, string idLevel, string idGrade, List<string> ListStudent, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
