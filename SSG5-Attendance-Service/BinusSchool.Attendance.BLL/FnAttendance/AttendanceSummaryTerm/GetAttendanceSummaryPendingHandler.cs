using System;
using System.Collections.Generic;
using System.Linq;
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
using BinusSchool.Persistence.AttendanceDb.Abstractions;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryPendingHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IMachineDateTime _datetime;

        public GetAttendanceSummaryPendingHandler(IAttendanceDbContext dbContext, IRedisCache redisCache, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
            _datetime = datetime;

        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryPendingRequest>();
            string[] _columns = { "date", "clasId", "teacher", "homeroom", "subjectId", "sessionId", "totalstudent" };

            #region get id lesson per user login
            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                IdHomeroom = param.IdHomeroom,
                IdSubject = param.IdSubject,
                IdClassroom = param.IdClassroom
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
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisHomeroomTeacher = await AttendanceSummaryRedisCacheHandler.GetHomeroomTeacher(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var listSchedule = redisSchedule
                               .Where(e => listIdLesson.Contains(e.IdLesson))
                               .ToList();

            var queryPendingEntry = redisAttendanceEntry
                            .Where(e => listIdLesson.Contains(e.IdLesson)
                                   && e.Status == AttendanceEntryStatus.Pending);

            if (!string.IsNullOrEmpty(param.StartDate.ToString()) && !string.IsNullOrEmpty(param.EndDate.ToString()))
                queryPendingEntry = queryPendingEntry.Where(e => (e.ScheduleDate.Date >= Convert.ToDateTime(param.StartDate).Date && e.ScheduleDate.Date <= Convert.ToDateTime(param.EndDate).Date));
            else
                queryPendingEntry = queryPendingEntry.Where(e => e.ScheduleDate.Date <= _datetime.ServerTime.Date);

            var listAttendanceEntry = queryPendingEntry.ToList();

            var listIdGrade = redisHomeroomStudentEnrollment.Select(e => e.Grade.Id).Distinct().ToList();
            var listIdlevel = redisHomeroomStudentEnrollment.Select(e => e.Level.Id).Distinct().ToList();

            var listMappingAttendance = redisMappingAttendance.Where(e => listIdlevel.Contains(e.IdLevel)).ToList();

            if (!string.IsNullOrEmpty(param.IdClassroom))
            {
                redisHomeroomStudentEnrollment = redisHomeroomStudentEnrollment.Where(e => e.IdClassroom == param.IdClassroom).ToList();
                redisTrHomeroomStudentEnrollment = redisTrHomeroomStudentEnrollment.Where(e => e.IdClassroom == param.IdClassroom).ToList();
            }

            var pendingRequest = new AttendanceSummaryUnsubmitedRequest
            {
                ListStaudetStatus = redisStudentStatus,
                ListHomeroomStudentEnrollment = redisHomeroomStudentEnrollment,
                ListTrHomeroomStudentEnrollment = redisTrHomeroomStudentEnrollment,
                ListAttendanceEntry = listAttendanceEntry,
                ListSchedule = listSchedule,
                ListMappingAttendance = listMappingAttendance,
                SelectedPosition = param.SelectedPosition,
                ListHomeroomTeacher = redisHomeroomTeacher,
            };

            var listPending = GetPending(pendingRequest).Result;

            IEnumerable<GetAttendanceSummaryPendingResult> queryPending = default;
            if (listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any())
            {
                queryPending = listPending
                                    .GroupBy(e => new
                                    {
                                        e.Date,
                                        idHomeroom = e.Homeroom.Id,
                                        homeroom = e.Homeroom.Description,
                                        e.ClassID,
                                        e.TotalStudent,
                                        e.SubjectId,
                                        idSession = e.Session.Id,
                                        sessionName = e.Session.Name,
                                        sessionID = e.Session.SessionID,
                                        teacherId = e.Teacher.Id,
                                        teacherName = e.Teacher.Description
                                    })
                                    .Select(e => new GetAttendanceSummaryPendingResult
                                    {
                                        Date = e.Key.Date,
                                        ClassID = e.Key.ClassID,
                                        Homeroom = new ItemValueVm
                                        {
                                            Id = e.Key.idHomeroom,
                                            Description = e.Key.homeroom
                                        },
                                        Teacher = new ItemValueVm
                                        {
                                            Id = e.Key.teacherId,
                                            Description = e.Key.teacherName
                                        },
                                        Session = new RedisAttendanceSummarySession
                                        {
                                            Id = e.Key.idSession,
                                            Name = e.Key.sessionName,
                                            SessionID = e.Key.sessionID,
                                        },
                                        SubjectId = e.Key.SubjectId,
                                        TotalStudent = e.Key.TotalStudent,

                                    }).OrderBy(e => e.Date).Distinct();

                if (!string.IsNullOrEmpty(param.Search))
                    queryPending = queryPending
                                    .Where(e => e.ClassID.ToLower().Contains(param.Search)
                                    || e.Teacher.Id.ToLower().Contains(param.Search)
                                    || e.Teacher.Description.ToLower().Contains(param.Search));
            }
            else
            {
                queryPending = listPending
                                .GroupBy(e => new
                                {
                                    e.Date,
                                    idHomeroom = e.Homeroom.Id,
                                    homeroom = e.Homeroom.Description,
                                    e.TotalStudent,
                                    teacherId = e.HomeroomTeacher.Id,
                                    teacherName = e.HomeroomTeacher.Description
                                })
                                .Select(e => new GetAttendanceSummaryPendingResult
                                {
                                    Date = e.Key.Date,
                                    Homeroom = new ItemValueVm
                                    {
                                        Id = e.Key.idHomeroom,
                                        Description = e.Key.homeroom
                                    },
                                    Teacher = new ItemValueVm
                                    {
                                        Id = e.Key.teacherId,
                                        Description = e.Key.teacherName
                                    },
                                    TotalStudent = e.Key.TotalStudent,

                                }).OrderBy(e => e.Date).Distinct();

                if (!string.IsNullOrEmpty(param.Search))
                    queryPending = queryPending
                                    .Where(e => e.Teacher.Description.ToLower().Contains(param.Search)
                                        || e.Teacher.Id.ToLower().Contains(param.Search)
                                        || e.Homeroom.Description.ToLower().Contains(param.Search));
            }

           

            switch (param.OrderBy)
            {
                case "date":
                    queryPending = param.OrderType == OrderType.Desc
                        ? queryPending.OrderByDescending(x => x.Date)
                        : queryPending.OrderBy(x => x.Date);
                    break;
                case "clasId":
                    queryPending = param.OrderType == OrderType.Desc
                        ? queryPending.OrderByDescending(x => x.ClassID)
                        : queryPending.OrderBy(x => x.ClassID);
                    break;
                case "teacher":
                    queryPending = param.OrderType == OrderType.Desc
                        ? queryPending.OrderByDescending(x => x.Teacher.Description)
                        : queryPending.OrderBy(x => x.Teacher.Description);
                    break;
                case "homeroom":
                    queryPending = param.OrderType == OrderType.Desc
                        ? queryPending.OrderByDescending(x => x.Homeroom.Description)
                        : queryPending.OrderBy(x => x.Homeroom.Description);
                    break;
                case "subjectId":
                    queryPending = param.OrderType == OrderType.Desc
                        ? queryPending.OrderByDescending(x => x.SubjectId)
                        : queryPending.OrderBy(x => x.SubjectId);
                    break;
                case "sessionId":
                    queryPending = param.OrderType == OrderType.Desc
                        ? queryPending.OrderByDescending(x => x.Session.SessionID)
                        : queryPending.OrderBy(x => x.Session.SessionID);
                    break;
                case "totalstudent":
                    queryPending = param.OrderType == OrderType.Desc
                        ? queryPending.OrderByDescending(x => x.TotalStudent)
                        : queryPending.OrderBy(x => x.TotalStudent);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = queryPending
                    .ToList();
            }
            else
            {
                items = queryPending
                    .SetPagination(param)
                     .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryPending.Select(x => x.Date).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        public static async Task<List<GetAttendanceSummaryPendingResult>> GetPending(AttendanceSummaryUnsubmitedRequest listAttendanceSummaryUnsubmitedRequest)
        {
            List<string> idLessonNoHomeroom = new List<string>();
            List<string> idLessonNoHomeroomGrouping = new List<string>();
            List<GetAttendanceSummaryPendingResult> attendance = new List<GetAttendanceSummaryPendingResult>();

            foreach (var itemAttendance in listAttendanceSummaryUnsubmitedRequest.ListAttendanceEntry)
            {
                var listStatusStudentByDate = listAttendanceSummaryUnsubmitedRequest.ListStaudetStatus
                                                .Where(e => e.StartDate.Date <= itemAttendance.ScheduleDate.Date && e.EndDate.Date >= itemAttendance.ScheduleDate.Date)
                                                .Select(e => e.IdStudent).ToList();


                //moving Student
                var listStudentEnrolmentBySchedule = listAttendanceSummaryUnsubmitedRequest.ListHomeroomStudentEnrollment
                                            .Where(e => listStatusStudentByDate.Contains(e.IdStudent) && e.Semester == itemAttendance.Semester)
                                            .ToList();

                var listTrStudentEnrolmentBySchedule = listAttendanceSummaryUnsubmitedRequest.ListTrHomeroomStudentEnrollment
                                            .Where(e => listStatusStudentByDate.Contains(e.IdStudent) && e.Semester == itemAttendance.Semester)
                                            .ToList();

                var listStudentEnrollmentUnion = listStudentEnrolmentBySchedule.Union(listTrStudentEnrolmentBySchedule)
                                                   .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                   .ToList();

                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, itemAttendance.ScheduleDate, itemAttendance.Semester.ToString(), itemAttendance.IdLesson);

                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                              .ToList();

                var listIdHomeroomStudent = studentEnrollmentMoving.Select(e => e.IdHomeroomStudent).ToList();

                var listAttendanceEntryBySchedule = listAttendanceSummaryUnsubmitedRequest.ListAttendanceEntry
                                                    .Where(e => e.IdScheduleLesson == itemAttendance.IdScheduleLesson
                                                            && listIdHomeroomStudent.Contains(e.IdHomeroomStudent))
                                                    .Select(e => e.IdHomeroomStudent).Distinct().ToList();

                var homeroom = studentEnrollmentMoving.FirstOrDefault();
                if (homeroom == null)
                    continue;
                var teacher = listAttendanceSummaryUnsubmitedRequest.ListSchedule.Where(e => e.IdLesson == itemAttendance.IdLesson
                                                   && e.IdSession == itemAttendance.Session.Id
                                                   && e.IdWeek == itemAttendance.IdWeek
                                                   && e.IdDay == itemAttendance.IdDay
                                               ).FirstOrDefault();

                var homeroomTeacher = listAttendanceSummaryUnsubmitedRequest.ListHomeroomTeacher
                                        .Where(e => e.IdHomeroom == homeroom.Homeroom.Id && e.IsAttendance)
                                        .FirstOrDefault();

                if (homeroom == null)
                {
                    if (!idLessonNoHomeroom.Where(e => e == itemAttendance.IdLesson).Any())
                        idLessonNoHomeroom.Add(itemAttendance.IdLesson);

                    idLessonNoHomeroomGrouping.Add(itemAttendance.IdLesson);
                    continue;
                }

                if (listAttendanceEntryBySchedule.Any())
                {
                    attendance.Add(new GetAttendanceSummaryPendingResult
                    {
                        Date = itemAttendance.ScheduleDate.Date,
                        ClassID = itemAttendance.ClassID,
                        Teacher = new ItemValueVm
                        {
                            Id = teacher.Teacher.IdUser,
                            Description = NameUtil.GenerateFullName(teacher.Teacher.FirstName, teacher.Teacher.LastName)
                        },
                        HomeroomTeacher = new ItemValueVm
                        {
                            Id = homeroomTeacher.Teacher.IdUser,
                            Description = NameUtil.GenerateFullName(homeroomTeacher.Teacher.FirstName, homeroomTeacher.Teacher.LastName)
                        },
                        Homeroom = new ItemValueVm
                        {
                            Id = homeroom.Homeroom.Id,
                            Description = homeroom.Homeroom.Description,
                        },
                        SubjectId = itemAttendance.Subject.SubjectID,
                        Session = new RedisAttendanceSummarySession
                        {
                            Id = itemAttendance.Session.Id,
                            Name = itemAttendance.Session.Name,
                            SessionID = itemAttendance.Session.SessionID.ToString(),
                        },
                        TotalStudent = listAttendanceEntryBySchedule.Count(),
                    });
                }
            }

            return attendance;
        }
    }
}
