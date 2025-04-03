using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
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

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailWorkhabitHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDetailWorkhabitHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailWorkhabitRequest>();
            var _columns = new[] { "date", "session", "subject", "teacherName", "comment" };

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

            var listAttendanceEntry = redisAttendanceEntry
                               .Where(e => (e.ScheduleDate.Date >= param.StartDate.Date && e.ScheduleDate.Date <= param.EndDate.Date)
                                        && e.IdStudent == param.IdStudent
                                        && e.AttendanceEntryWorkhabit.Any(f => f.IdMappingAttendanceWorkhabit == param.IdMappingAttendanceWorkhabit)
                                        && e.Status == AttendanceEntryStatus.Submitted
                                    )
                                .ToList();

            var listIdUserTeacher = listAttendanceEntry.Select(e => e.IdUserTeacher).Distinct().ToList();

            var listUser = redisUser
                            .Where(e => listIdUserTeacher.Contains(e.Id))
                            .Select(e => new
                            {
                                e.Id,
                                e.DisplayName
                            })
                            .ToList();

            List<GetAttendanceSummaryDetailWorkhabitResult> listWorkhabit = new List<GetAttendanceSummaryDetailWorkhabitResult>();
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
                    var newWorkhabit = new GetAttendanceSummaryDetailWorkhabitResult
                    {
                        Date = itemAttendanceEntry.ScheduleDate,
                        Session = itemAttendanceEntry.Session.Name,
                        Subject = itemAttendanceEntry.Subject.Description,
                        TeacherName = teacherName,
                        HomeroomTeacherName = String.Join(", ", listHomeroomTeacher.Select(e => e.name).ToList()),
                        Comment = itemAttendanceEntry.Notes,
                    };

                    listWorkhabit.Add(newWorkhabit);
                }
            }

            var queryWorkhabit = listWorkhabit.Distinct();

            if (redisMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any())
            {
                queryWorkhabit = queryWorkhabit
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                Session = e.Session,
                                Subject = e.Subject,
                                TeacherName = e.TeacherName,
                                Comment = e.Comment,
                            })
                            .Select(e => new GetAttendanceSummaryDetailWorkhabitResult
                            {
                                Date = e.Key.Date,
                                Session = e.Key.Session,
                                Subject = e.Key.Subject,
                                TeacherName = e.Key.TeacherName,
                                Comment = e.Key.Comment,
                            }).OrderBy(e => e.Date).Distinct();
            }
            else
            {
                queryWorkhabit = queryWorkhabit
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                TeacherName = e.HomeroomTeacherName,
                                Comment = e.Comment,
                            })
                            .Select(e => new GetAttendanceSummaryDetailWorkhabitResult
                            {
                                Date = e.Key.Date,
                                TeacherName = e.Key.TeacherName,
                                Comment = e.Key.Comment,
                            }).OrderBy(e => e.Date).Distinct();
            }


            switch (param.OrderBy)
            {
                case "date":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.Date)
                        : queryWorkhabit.OrderBy(x => x.Date);
                    break;
                case "session":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.Session)
                        : queryWorkhabit.OrderBy(x => x.Session);
                    break;
                case "subject":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.Subject)
                        : queryWorkhabit.OrderBy(x => x.Subject);
                    break;
                case "teacherName":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.TeacherName)
                        : queryWorkhabit.OrderBy(x => x.TeacherName);
                    break;
                case "commentt":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.Comment)
                        : queryWorkhabit.OrderBy(x => x.Comment);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = queryWorkhabit
                    .ToList();
            }
            else
            {
                items = queryWorkhabit
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryWorkhabit.Select(x => x.Date).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
