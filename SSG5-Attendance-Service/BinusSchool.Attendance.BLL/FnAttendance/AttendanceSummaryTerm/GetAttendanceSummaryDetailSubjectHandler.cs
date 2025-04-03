using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDetailSubjectHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailSubjectRequest>();
            var _columns = new[] { "homeroom", "classIdSubject", "teacherName", "unsubmited", "pending" };

            #region get id lesson per user login
            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                IdSubject = param.IdSubject
            };

            var listIdLesson = await AttendanceSummaryRedisCacheHandler.GetLessonByUser(_dbContext, CancellationToken, filterIdHomerooms, _redisCache);
            #endregion

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisTrHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetTrHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisSchedule = await AttendanceSummaryRedisCacheHandler.GetSchedule(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisStudentStatus = await AttendanceSummaryRedisCacheHandler.GetStudentStatus(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisAttendanceEntry = await AttendanceSummaryRedisCacheHandler.GetAttendanceEntry(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisScheduleLesson = await AttendanceSummaryRedisCacheHandler.GetScheduleLesson(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime.Date);
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisLessonTeacher = await AttendanceSummaryRedisCacheHandler.GetLessonTeacher(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisHomeroomTeacher = await AttendanceSummaryRedisCacheHandler.GetHomeroomTeacher(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var queryAttendanceEntry = redisAttendanceEntry
                               .Where(e => listIdLesson.Contains(e.IdLesson)
                                        && (e.ScheduleDate.Date >= Convert.ToDateTime(param.StartDate).Date && e.ScheduleDate.Date <= Convert.ToDateTime(param.EndDate))
                                        && e.Status == AttendanceEntryStatus.Submitted
                                    );

            var listAttendanceEntry = queryAttendanceEntry
                                        .GroupBy(e => new RedisAttendanceSummaryAttendanceEntryResult
                                        {
                                            IdScheduleLesson = e.IdScheduleLesson,
                                            IdHomeroomStudent = e.IdHomeroomStudent,
                                        })
                                        .Select(e => e.Key)
                                        .ToList();

            var listAttendanceEntryPending = queryAttendanceEntry
                                                .Where(e => e.Status == AttendanceEntryStatus.Pending)
                                                .GroupBy(e => new RedisAttendanceSummaryAttendanceEntryResult
                                                {
                                                    ScheduleDate = e.ScheduleDate,
                                                    ClassID = e.ClassID,
                                                    IdStudent = e.IdStudent,
                                                    IdLesson = e.IdLesson,
                                                    IdGrade = e.IdGrade,
                                                    IdWeek = e.IdWeek,
                                                    IdDay = e.IdDay,
                                                    Session = new RedisAttendanceSummarySession
                                                    {
                                                        Id = e.Session.Id,
                                                        Name = e.Session.Name,
                                                        SessionID = e.Session.SessionID.ToString(),
                                                    },
                                                    Subject = new RedisAttendanceSummarySubject
                                                    {
                                                        Id = e.Subject.Id,
                                                        Code = e.Subject.Code,
                                                        Description = e.Subject.Description,
                                                        SubjectID = e.Subject.SubjectID,
                                                    }
                                                })
                                                .Select(e => e.Key)
                                                .ToList();

            var listScheduleLesoon = redisScheduleLesson
                                .Where(e => listIdLesson.Contains(e.IdLesson)
                                        && (e.ScheduleDate.Date >= param.StartDate.Date && e.ScheduleDate.Date <= param.EndDate.Date)
                                        )
                                .ToList();

            var listSchedule = redisSchedule
                               .Where(e => listIdLesson.Contains(e.IdLesson))
                               .ToList();


            var listLessonTeacher = redisLessonTeacher
                             .Where(e => e.IdUserTeacher == param.IdUser
                                     && e.IsAttendance
                                     && listIdLesson.Contains(e.IdLesson))
                            .ToList();

            List<GetAttendanceSummaryDetailSubjectResult> dataSubject = new List<GetAttendanceSummaryDetailSubjectResult>();
            List<string> idLessonNoHomeroom = new List<string>();
            List<string> idLessonNoHomeroomGrouping = new List<string>();
            foreach (var itemScheduleLesoon in listScheduleLesoon)
            {
                var termAttendance = redisMappingAttendance.Where(x => x.IdLevel == itemScheduleLesoon.IdLevel).FirstOrDefault();
                if (termAttendance != null)
                {
                    if (termAttendance.AbsentTerms == AbsentTerm.Day)
                        continue;
                }

                var listStatusStudentByDate = redisStudentStatus.Where(e => e.StartDate.Date <= itemScheduleLesoon.ScheduleDate.Date).Select(e => e.IdStudent).ToList();
                var listStudentEnrolmentBySchedule = redisHomeroomStudentEnrollment
                                            .Where(e => e.IdLesson == itemScheduleLesoon.IdLesson && listStatusStudentByDate.Contains(e.IdStudent) && e.Semester == itemScheduleLesoon.Semester)
                                            .Select(e => new
                                            {
                                                e.IdHomeroomStudent,
                                                idHomeroom = e.Homeroom.Id,
                                                e.Homeroom
                                            })
                                            .ToList();

                var homeroom = listStudentEnrolmentBySchedule.FirstOrDefault();

                var Teacher = listSchedule.Where(e => e.IdLesson == itemScheduleLesoon.IdLesson
                                                    && e.IdSession == itemScheduleLesoon.Session.Id
                                                    && e.IdWeek == itemScheduleLesoon.IdWeek
                                                    && e.IdDay == itemScheduleLesoon.IdDay
                                                ).FirstOrDefault();

                if (homeroom == null)
                {
                    if (!idLessonNoHomeroom.Where(e => e == itemScheduleLesoon.IdLesson).Any())
                        idLessonNoHomeroom.Add(itemScheduleLesoon.IdLesson);

                    idLessonNoHomeroomGrouping.Add(itemScheduleLesoon.IdLesson);
                    continue;
                }

                if (!dataSubject.Where(e => e.ClassIdSubject == $"{itemScheduleLesoon.ClassID}-{itemScheduleLesoon.Subject.Description}").Any())
                {
                    var listScheduleLesoonSubject = listScheduleLesoon.Where(e => e.Subject.Id == itemScheduleLesoon.Subject.Id).ToList();
                    var listAttendanceEntryPendingSubject = listAttendanceEntryPending.Where(e => e.Subject.Id == itemScheduleLesoon.Subject.Id).ToList();

                    var unsubmitedAndPendingRequest = new AttendanceSummaryUnsubmitedRequest
                    {
                        ListScheduleLesoon = listScheduleLesoonSubject,
                        ListStaudetStatus = redisStudentStatus,
                        ListHomeroomStudentEnrollment = redisHomeroomStudentEnrollment,
                        ListTrHomeroomStudentEnrollment = redisTrHomeroomStudentEnrollment,
                        ListAttendanceEntry = listAttendanceEntry,
                        ListSchedule = listSchedule,
                        ListMappingAttendance = redisMappingAttendance,
                        SelectedPosition = param.SelectedPosition,
                        ListLessonTeacher = listLessonTeacher,
                        ListHomeroomTeacher = redisHomeroomTeacher,
                    };

                    var listUnsubmited = GetAttendanceSummaryUnsubmitedHandler.GetUnsubmited(unsubmitedAndPendingRequest).Result.Where(e=>e.ClassID==itemScheduleLesoon.ClassID).ToList();

                    unsubmitedAndPendingRequest.ListAttendanceEntry = listAttendanceEntryPendingSubject;
                    var listPending = GetAttendanceSummaryPendingHandler.GetPending(unsubmitedAndPendingRequest).Result;

                    var countUnsubmited = 0;
                    var countPending = 0;
                    if (redisMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any())
                    {
                        countUnsubmited = listUnsubmited
                                            .GroupBy(e => new
                                            {
                                                e.Date,
                                                e.Homeroom,
                                                e.ClassID,
                                                e.TotalStudent,
                                                e.SubjectId,
                                                e.Session.Id,
                                                TeacherId = e.Teacher.Id,
                                                TeacherName = e.Teacher.Description
                                            })
                                            .Select(e=>e.Key.TotalStudent)
                                            .Sum();

                        countPending = listPending
                                           .GroupBy(e => new
                                           {
                                               e.Date,
                                               e.Homeroom,
                                               e.ClassID,
                                               e.TotalStudent,
                                               e.SubjectId,
                                               e.Session.Id,
                                               TeacherId = e.Teacher.Id,
                                               TeacherName = e.Teacher.Description
                                           })
                                           .Select(e => e.Key.TotalStudent)
                                           .Sum();
                    }
                    else
                    {
                        countUnsubmited = listUnsubmited
                                            .GroupBy(e => new
                                            {
                                                e.Date,
                                                e.Homeroom,
                                                e.ClassID,
                                                e.TotalStudent,
                                                TeacherId = e.Teacher.Id,
                                                TeacherName = e.Teacher.Description
                                            })
                                            .Select(e => e.Key.TotalStudent)
                                            .Sum(); ;

                        countPending = listPending
                                            .GroupBy(e => new
                                            {
                                                e.Date,
                                                e.Homeroom,
                                                e.ClassID,
                                                e.TotalStudent,
                                                TeacherId = e.Teacher.Id,
                                                TeacherName = e.Teacher.Description
                                            })
                                            .Select(e => e.Key.TotalStudent)
                                            .Sum(); ;
                    }

                     var newSubject = new GetAttendanceSummaryDetailSubjectResult
                    {
                        IdAcademicYear = itemScheduleLesoon.IdAcademicYear,
                        IdLevel = itemScheduleLesoon.IdLevel,
                        IdGrade = itemScheduleLesoon.IdGrade,
                        IdSubject = itemScheduleLesoon.Subject.Id,
                        ClassIdSubject = $"{itemScheduleLesoon.ClassID}-{itemScheduleLesoon.Subject.Description}",
                        TeacherName = Teacher != null ? NameUtil.GenerateFullName(Teacher.Teacher.FirstName, Teacher.Teacher.LastName) : string.Empty,
                        Homeroom = homeroom.Homeroom.Description,
                        Unsubmited = countUnsubmited,
                        Pending = countPending,
                    };

                    dataSubject.Add(newSubject);
                }
            }

            var querySubject = dataSubject.Distinct();

            if (!string.IsNullOrEmpty(param.Search))
                querySubject = querySubject.Where(e => e.ClassIdSubject.ToLower().Contains(param.Search.ToLower()) || e.TeacherName.ToLower().Contains(param.Search.ToLower()));

            switch (param.OrderBy)
            {
                case "homeroom":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Homeroom)
                        : querySubject.OrderBy(x => x.Homeroom);
                    break;
                case "classIdSubject":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.ClassIdSubject)
                        : querySubject.OrderBy(x => x.ClassIdSubject);
                    break;
                case "teacherName":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.TeacherName)
                        : querySubject.OrderBy(x => x.TeacherName);
                    break;
                case "unsubmited":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Unsubmited)
                        : querySubject.OrderBy(x => x.Unsubmited);
                    break;
                case "pending":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Pending)
                        : querySubject.OrderBy(x => x.Pending);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = querySubject
                    .ToList();
            }
            else
            {
                items = querySubject
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : querySubject.Select(x => x.IdSubject).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
