using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
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
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailUnexcusedExcusedHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDetailUnexcusedExcusedHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailUnexcusedExcusedRequest>();
            var _columns = new[] { "date", "session", "subject", "teacherName", "attendanceStatus", "reason" };

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisTrHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetTrHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisAttendanceEntry = await AttendanceSummaryRedisCacheHandler.GetAttendanceEntry(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisUser = await AttendanceSummaryRedisCacheHandler.GetUser(_redisCache, _dbContext, CancellationToken);
            var redisStudentStatus = await AttendanceSummaryRedisCacheHandler.GetStudentStatus(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisHomeroomTeacher = await AttendanceSummaryRedisCacheHandler.GetHomeroomTeacher(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var listLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                 .Include(e => e.Lesson)
                 .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                         && e.IsAttendance
                         && redisAttendanceEntry.Select(x=> x.IdLesson).Distinct().ToList().Contains(e.IdLesson)
                         && e.IsPrimary)
                 .ToListAsync(CancellationToken);

            foreach (var data in redisAttendanceEntry.Where(x=> x.IsFromAttendanceAdministration).ToList())
            {
                data.IdUserAttendanceEntry = listLessonTeacher.Where(x => x.IdLesson == data.IdLesson).Select(x=> x.IdUser).FirstOrDefault();
            }

            var listAttendanceEntry = redisAttendanceEntry
                               .Where(e => (e.ScheduleDate.Date >= param.StartDate.Date && e.ScheduleDate.Date <= param.EndDate.Date)
                                        && e.IdStudent == param.IdStudent
                                        && e.Attendance.AbsenceCategory.HasValue
                                        && e.Status == AttendanceEntryStatus.Submitted
                                    )
                                .ToList();

            var listIdUserTeacher = listAttendanceEntry.Select(e => e.IdUserAttendanceEntry).Distinct().ToList();

            var listUser = redisUser
                            .Where(e => listIdUserTeacher.Contains(e.Id))
                            .Select(e => new
                            {
                                e.Id,
                                e.DisplayName
                            })
                            .ToList();

            if (param.ExcuseAbsenceCategory != null)
            {
                listAttendanceEntry = listAttendanceEntry.Where(x => x.Attendance.ExcusedAbsenceCategory == param.ExcuseAbsenceCategory).ToList();
            }

            List<GetAttendanceSummaryDetailUnexcusedExcusedResult> listUaEa = new List<GetAttendanceSummaryDetailUnexcusedExcusedResult>();
            foreach (var itemAttendanceEntry in listAttendanceEntry)
            {
                var teacherName = listUser.Where(e => e.Id == itemAttendanceEntry.IdUserAttendanceEntry).Select(e => e.DisplayName).FirstOrDefault();

                var listStatusStudentByDate = redisStudentStatus.Where(e => e.StartDate.Date <= itemAttendanceEntry.ScheduleDate.Date).Select(e => e.IdStudent).ToList();

                //moving student
                var listStudentEnrolmentBySchedule = redisHomeroomStudentEnrollment
                                    .Where(e => e.IdHomeroomStudent== itemAttendanceEntry.IdHomeroomStudent && e.Semester == itemAttendanceEntry.Semester)
                                    .ToList();

                var listTrStudentEnrolmentBySchedule = redisTrHomeroomStudentEnrollment
                                            .Where(e => e.IdHomeroomStudent == itemAttendanceEntry.IdHomeroomStudent && e.Semester == itemAttendanceEntry.Semester)
                                            .ToList();

                var listStudentEnrollmentUnion = listStudentEnrolmentBySchedule.Union(listTrStudentEnrolmentBySchedule)
                                                   .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                   .ToList();

                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, itemAttendanceEntry.ScheduleDate, itemAttendanceEntry.Semester.ToString(), itemAttendanceEntry.IdLesson);

                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                              .ToList();

                var idHomeroom = listStudentEnrolmentBySchedule.Select(e => e.Homeroom.Id).FirstOrDefault();
                var listHomeroomTeacher = redisHomeroomTeacher.Where(e => e.IdHomeroom == idHomeroom)
                                            .Where(e => e.Position.Code == PositionConstant.ClassAdvisor)
                                            .Select(e => new
                                            {
                                                name = NameUtil.GenerateFullName(e.Teacher.FirstName, e.Teacher.LastName)
                                            })
                                            .ToList();

                if (studentEnrollmentMoving.Any())
                {
                    var newUaEa = new GetAttendanceSummaryDetailUnexcusedExcusedResult
                    {
                        Date = itemAttendanceEntry.ScheduleDate,
                        Session = itemAttendanceEntry.Session.Name,
                        Subject = itemAttendanceEntry.Subject.Description,
                        TeacherName = teacherName,
                        HomeroomTeacherName = String.Join(", ", listHomeroomTeacher.Select(e => e.name).ToList()),
                        AttendanceStatus = itemAttendanceEntry.Attendance.Description,
                        Reason = itemAttendanceEntry.Notes,
                        AbsenceCategory = itemAttendanceEntry.Attendance.AbsenceCategory,
                        IdAttendance = itemAttendanceEntry.Attendance.Id
                    };

                    listUaEa.Add(newUaEa);
                }
            }

            var queryUaEa = listUaEa.Distinct();

            if (!string.IsNullOrEmpty(param.AbsenceCategory.ToString()))
                queryUaEa = queryUaEa.Where(e => e.AbsenceCategory == param.AbsenceCategory);

            if (!string.IsNullOrEmpty(param.Search))
            {
                if(param.AbsenceCategory== null)
                    queryUaEa = queryUaEa.Where(e => e.AttendanceStatus.Contains(param.Search));
                else
                    queryUaEa = queryUaEa.Where(e => e.Subject.Contains(param.Search) || e.TeacherName.Contains(param.Search));
            }

            if (redisMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any())
            {
                queryUaEa = queryUaEa
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                Session = e.Session,
                                Subject = e.Subject,
                                TeacherName = e.TeacherName,
                                AttendanceStatus = e.AttendanceStatus,
                                Reason = e.Reason,
                                AbsenceCategory = e.AbsenceCategory,
                                IdAttendance = e.IdAttendance
                            })
                            .Select(e => new GetAttendanceSummaryDetailUnexcusedExcusedResult
                            {
                                Date = e.Key.Date,
                                Session = e.Key.Session,
                                Subject = e.Key.Subject,
                                TeacherName = e.Key.TeacherName,
                                AttendanceStatus = e.Key.AttendanceStatus,
                                Reason = e.Key.Reason,
                                AbsenceCategory = e.Key.AbsenceCategory,
                                IdAttendance = e.Key.IdAttendance
                            }).OrderBy(e => e.Date).Distinct();
            }
            else
            {
                queryUaEa = queryUaEa
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                TeacherName = e.HomeroomTeacherName,
                                AttendanceStatus = e.AttendanceStatus,
                                Reason = e.Reason,
                                AbsenceCategory = e.AbsenceCategory,
                                IdAttendance = e.IdAttendance
                            })
                            .Select(e => new GetAttendanceSummaryDetailUnexcusedExcusedResult
                            {
                                Date = e.Key.Date,
                                TeacherName = e.Key.TeacherName,
                                AttendanceStatus = e.Key.AttendanceStatus,
                                Reason = e.Key.Reason,
                                AbsenceCategory = e.Key.AbsenceCategory,
                                IdAttendance = e.Key.IdAttendance
                            }).OrderBy(e => e.Date).Distinct();
            }


            switch (param.OrderBy)
            {
                case "date":
                    queryUaEa = param.OrderType == OrderType.Desc
                        ? queryUaEa.OrderByDescending(x => x.Date)
                        : queryUaEa.OrderBy(x => x.Date);
                    break;
                case "session":
                    queryUaEa = param.OrderType == OrderType.Desc
                        ? queryUaEa.OrderByDescending(x => x.Session)
                        : queryUaEa.OrderBy(x => x.Session);
                    break;
                case "subject":
                    queryUaEa = param.OrderType == OrderType.Desc
                        ? queryUaEa.OrderByDescending(x => x.Subject)
                        : queryUaEa.OrderBy(x => x.Subject);
                    break;
                case "teacherName":
                    queryUaEa = param.OrderType == OrderType.Desc
                        ? queryUaEa.OrderByDescending(x => x.TeacherName)
                        : queryUaEa.OrderBy(x => x.TeacherName);
                    break;
                case "attendanceStatus":
                    queryUaEa = param.OrderType == OrderType.Desc
                        ? queryUaEa.OrderByDescending(x => x.AttendanceStatus)
                        : queryUaEa.OrderBy(x => x.AttendanceStatus);
                    break;
                case "reason":
                    queryUaEa = param.OrderType == OrderType.Desc
                        ? queryUaEa.OrderByDescending(x => x.Reason)
                        : queryUaEa.OrderBy(x => x.Reason);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = queryUaEa
                    .ToList();
            }
            else
            {
                items = queryUaEa
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryUaEa.Select(x => x.Date).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
