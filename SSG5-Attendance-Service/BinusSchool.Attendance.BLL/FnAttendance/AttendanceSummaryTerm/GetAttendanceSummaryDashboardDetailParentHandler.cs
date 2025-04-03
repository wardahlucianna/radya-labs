using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Persistence.AttendanceDb.Abstractions;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDashboardDetailParentHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDashboardDetailParentHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDashboardDetailParentRequest>();
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
            var redisPeriod = await AttendanceSummaryRedisCacheHandler.GetPeriod(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisHomeroomTeacher = await AttendanceSummaryRedisCacheHandler.GetHomeroomTeacher(paramRedis, _redisCache, _dbContext, CancellationToken);

            #endregion

            var GetHomeroomStudent = redisHomeroomStudentEnrollment
                               .Where(e => e.IdStudent == param.IdStudent)
                               .ToList();

            var GetLevelGrade = GetHomeroomStudent.FirstOrDefault();

            if (GetLevelGrade == null)
                return Request.CreateApiResult2();

            var queryPeriod = redisPeriod
                                .Where(e => e.IdGrade == GetLevelGrade.Grade.Id
                                            && (_datetime.ServerTime.Date >= e.StartDate && _datetime.ServerTime.Date <= e.EndDate));

            DateTime startdate = default;
            DateTime enddate = default;
            if (!string.IsNullOrEmpty(param.PeriodType))
            {
                if (param.PeriodType == "Term")
                {
                    var getPeriod = redisPeriod.FirstOrDefault();

                    startdate = getPeriod.AttendanceStartDate;
                    enddate = getPeriod.AttendanceEndDate;
                }
                else if (param.PeriodType == "Semester")
                {
                    var getPeriod = redisPeriod.FirstOrDefault();

                    var listPeriod = redisPeriod
                               .Where(e => e.IdGrade == GetLevelGrade.Grade.Id
                                           && e.Semester == getPeriod.Semester).ToList();

                    startdate = listPeriod.Select(e => e.AttendanceStartDate).Min();
                    enddate = listPeriod.Select(e => e.AttendanceEndDate).Max();
                }
            }

            var queryAttendanceEntry = redisAttendanceEntry
                               .Where(e => e.IdStudent == param.IdStudent && e.Attendance.Description != "Present" && e.Status == AttendanceEntryStatus.Submitted)
                                .ToList();

            if (!string.IsNullOrEmpty(param.PeriodType))
            {
                if (param.PeriodType != "All")
                {
                    queryAttendanceEntry = queryAttendanceEntry
                                            .Where(e => e.ScheduleDate.Date >= startdate.Date && e.ScheduleDate.Date <= enddate.Date)
                                            .ToList();
                }
            }

            if (!string.IsNullOrEmpty(param.IdAttendance))
            {
                queryAttendanceEntry = queryAttendanceEntry.Where(e => e.Attendance.Id == param.IdAttendance).ToList();
            }

            var listAttendanceEntry = queryAttendanceEntry.ToList();

            var listIdUserTeacher = listAttendanceEntry.Select(e => e.IdUserTeacher).Distinct().ToList();

            var listUser = redisUser
                            .Where(e => listIdUserTeacher.Contains(e.Id))
                            .Select(e => new
                            {
                                e.Id,
                                e.DisplayName
                            })
                            .ToList();

            List<GetAttendanceSummaryDetailUnexcusedExcusedResult> listUaEa = new List<GetAttendanceSummaryDetailUnexcusedExcusedResult>();
            foreach (var itemAttendanceEntry in listAttendanceEntry)
            {
                var teacherName = listUser.Where(e => e.Id == itemAttendanceEntry.IdUserTeacher).Select(e => e.DisplayName).FirstOrDefault();

                var listStatusStudentByDate = redisStudentStatus.Where(e => e.StartDate.Date <= itemAttendanceEntry.ScheduleDate.Date).Select(e => e.IdStudent).ToList();

                //moving student
                var listStudentEnrolmentBySchedule = redisHomeroomStudentEnrollment
                                    .Where(e => e.IdHomeroomStudent == itemAttendanceEntry.IdHomeroomStudent && e.Semester == itemAttendanceEntry.Semester)
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

                var idHomeroom = studentEnrollmentMoving.Select(e => e.Homeroom.Id).FirstOrDefault();

                var homeroomTeacher = redisHomeroomTeacher
                                    .Where(e => e.IdHomeroom == idHomeroom && e.IsAttendance)
                                    .FirstOrDefault();

                if (studentEnrollmentMoving.Any())
                {
                    var newUaEa = new GetAttendanceSummaryDetailUnexcusedExcusedResult
                    {
                        Date = itemAttendanceEntry.ScheduleDate,
                        Session = itemAttendanceEntry.Session.Name,
                        Subject = itemAttendanceEntry.Subject.Description,
                        TeacherName = teacherName,
                        HomeroomTeacherName = NameUtil.GenerateFullName(homeroomTeacher.Teacher.FirstName,homeroomTeacher.Teacher.LastName),
                        AttendanceStatus = itemAttendanceEntry.Attendance.Description,
                        Reason = itemAttendanceEntry.Notes,
                        AbsenceCategory = itemAttendanceEntry.Attendance.AbsenceCategory,
                        IdAttendance = itemAttendanceEntry.Attendance.Id
                    };

                    listUaEa.Add(newUaEa);
                }
            }

            var queryUaEa = listUaEa.Distinct();

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
