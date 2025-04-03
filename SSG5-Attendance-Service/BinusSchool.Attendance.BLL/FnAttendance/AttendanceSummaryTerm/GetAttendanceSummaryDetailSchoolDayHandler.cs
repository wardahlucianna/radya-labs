using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
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
    public class GetAttendanceSummaryDetailSchoolDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IAttendanceSummary _apiAttendanceSummary;
        private readonly IRedisCache _redisCache;

        public GetAttendanceSummaryDetailSchoolDayHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IAttendanceSummary ApiAttendanceSummary, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _apiAttendanceSummary = ApiAttendanceSummary;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailSchoolDayRequest>();
            var _columns = new[] { "Subject", "session", "classId", "homeroom" };

            #region get id lesson per user login
            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                IdClassroom = param.IdClassroom,
                ClassId = param.ClassId
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
            var redisAttendanceMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetAttendanceMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var getAttendaceAndWorkhabit = await _apiAttendanceSummary.GetAttendanceAndWorkhabitByLevel(new GetAttendanceAndWorkhabitByLevelRequest
            {
                IdLevel = param.IdLevel
            });

            var attendaceAndWorkhabit = getAttendaceAndWorkhabit.Payload;

            var listAttendaceAndWorkhabit = attendaceAndWorkhabit.Attendances.Select(e => new GetAttendanceAndWorkhabitResult
            {
                Id = e.Id,
                Code = e.Code,
                Description = e.Description,
                Type = "Attendance",
                AbsenceCategory= redisAttendanceMappingAttendance.Where(f=>f.Id==e.Id).Select(e=>e.AbsenceCategory).FirstOrDefault()
            }).ToList();

            listAttendaceAndWorkhabit.AddRange(attendaceAndWorkhabit.Workhabits.Select(e => new GetAttendanceAndWorkhabitResult
            {
                Id = e.Id,
                Code = e.Code,
                Description = e.Description,
                Type = "Workhabits",
            }).ToList());

            var listHomeroomStudentEnrollment = redisHomeroomStudentEnrollment
                                .Where(e => listIdLesson.Contains(e.IdLesson))
                                .GroupBy(e => new GetHomeroom
                                {
                                    IdSubject = e.IdSubject,
                                    IdLesson = e.IdLesson,
                                    IdStudent = e.IdStudent,
                                    FirstName = e.FirstName,
                                    MiddleName = e.MiddleName,
                                    LastName = e.LastName,
                                    IdHomeroomStudent = e.IdHomeroomStudent,
                                    Homeroom = new ItemValueVm
                                    {
                                        Id = e.Homeroom.Id,
                                    },
                                    ClassroomCode = e.ClassroomCode,
                                    Grade = new CodeWithIdVm
                                    {
                                        Id = e.Grade.Id,
                                        Code = e.Grade.Code,
                                        Description = e.Grade.Description
                                    },
                                    Level = new CodeWithIdVm
                                    {
                                        Id = e.Level.Id,
                                        Code = e.Level.Code,
                                        Description = e.Level.Description
                                    },
                                    Semester = e.Semester
                                })
                                .Select(e => e.Key)
                                .ToList();

            var listIdlevel = listHomeroomStudentEnrollment.Select(e => e.Level.Id).Distinct().ToList();
            var listIdGrade = listHomeroomStudentEnrollment.Select(e => e.Grade.Id).Distinct().ToList();

            var queryAttendanceEntry = redisAttendanceEntry
                               .Where(e =>listIdLesson.Contains(e.IdLesson)
                                        && (e.ScheduleDate.Date >= Convert.ToDateTime(param.StartDate).Date && e.ScheduleDate.Date <= Convert.ToDateTime(param.EndDate))
                                    );

            if (!string.IsNullOrEmpty(param.IdSession))
                queryAttendanceEntry = queryAttendanceEntry.Where(e => e.Session.Id == param.IdSession);

            var listAttendanceEntry = queryAttendanceEntry
                                        .Select(e => new GetAttendanceEntryBySchoolDayResult
                                        {
                                            IdScheduleLesson = e.IdScheduleLesson,
                                            ScheduleDate = e.ScheduleDate,
                                            Status = e.Status,
                                            IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                                            IdHomeroomStudent = e.IdHomeroomStudent,
                                            AttendanceEntryWorkhabit = e.AttendanceEntryWorkhabit,
                                            FirstName = e.Student.FirstName,
                                            LastName = e.Student.LastName,
                                            MiddleName = e.Student.MiddleName,
                                            IdStudent = e.Student.IdStudent,
                                        })
                                        .ToList();

            var queryScheduleLesoon = redisScheduleLesson
                                .Where(e => listIdLesson.Contains(e.IdLesson)
                                        && (e.ScheduleDate.Date >= param.StartDate.Date && e.ScheduleDate.Date <= param.EndDate.Date)
                                        );

            if (!string.IsNullOrEmpty(param.IdSession))
                queryScheduleLesoon = queryScheduleLesoon.Where(e => e.Session.Id == param.IdSession);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                queryScheduleLesoon = queryScheduleLesoon.Where(e => e.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.ClassId))
                queryScheduleLesoon = queryScheduleLesoon.Where(e => e.ClassID == param.ClassId);

            var listScheduleLesoon = queryScheduleLesoon.ToList();

            var listSchedule = redisSchedule.Where(e => listIdLesson.Contains(e.IdLesson)).ToList();

            var listLessonTeacher = redisLessonTeacher.Where(e => e.IdUserTeacher == param.IdUser && e.IsAttendance && listIdLesson.Contains(e.IdLesson)).ToList();

            List<GetAttendanceSummaryDetailSchoolDayResult> dataSchoolDay = new List<GetAttendanceSummaryDetailSchoolDayResult>();
            List<string> idLessonNoHomeroom = new List<string>();
            List<string> idLessonNoHomeroomGrouping = new List<string>();
            foreach (var itemScheduleLesoon in listScheduleLesoon)
            {
                var listStatusStudentByDate = redisStudentStatus.Where(e => e.StartDate.Date <= itemScheduleLesoon.ScheduleDate.Date).Select(e => e.IdStudent).ToList();

                //moving student
                var listStudentEnrolmentBySchedule = redisHomeroomStudentEnrollment
                                    .Where(e => listStatusStudentByDate.Contains(e.IdStudent) && e.Semester == itemScheduleLesoon.Semester)
                                    .ToList();

                var listTrStudentEnrolmentBySchedule = redisTrHomeroomStudentEnrollment
                                            .Where(e => listStatusStudentByDate.Contains(e.IdStudent) && e.Semester == itemScheduleLesoon.Semester)
                                            .ToList();

                var listStudentEnrollmentUnion = listStudentEnrolmentBySchedule.Union(listTrStudentEnrolmentBySchedule)
                                                   .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                   .ToList();

                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, itemScheduleLesoon.ScheduleDate, itemScheduleLesoon.Semester.ToString(), itemScheduleLesoon.IdLesson);

                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                              .ToList();


                var homeroom = studentEnrollmentMoving.FirstOrDefault();

                if (!string.IsNullOrEmpty(param.IdClassroom))
                {
                    if (homeroom.IdClassroom != param.IdClassroom)
                        continue;
                }


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


                if (redisMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any())
                {
                    if (!dataSchoolDay.Where(e => e.Date == itemScheduleLesoon.ScheduleDate
                                    && e.Session.Id == itemScheduleLesoon.Session.Id
                                    && e.ClassId == itemScheduleLesoon.ClassID).Any())
                    {
                        var listIdHomeroomStudent = listStudentEnrollmentMoving.Select(e => e.IdHomeroomStudent).Distinct().ToList();

                        var listAttendanceEntryBySchedule = listAttendanceEntry
                                                   .Where(e => e.IdScheduleLesson == itemScheduleLesoon.Id
                                                           && listIdHomeroomStudent.Contains(e.IdHomeroomStudent)).ToList();

                        var listAttendanceAndWorkhabit = GetDataAttendanceAndWorkhabit(listAttendaceAndWorkhabit, listAttendanceEntryBySchedule, studentEnrollmentMoving);

                        var newLessonSchoolDay = new GetAttendanceSummaryDetailSchoolDayResult
                        {
                            Date = itemScheduleLesoon.ScheduleDate,
                            Session = new ItemValueVm
                            {
                                Id = itemScheduleLesoon.Session.Id,
                                Description = itemScheduleLesoon.Session.SessionID
                            },
                            ClassId = itemScheduleLesoon.ClassID,
                            DataAttendanceAndWorkhabit = listAttendanceAndWorkhabit.Select(e => new GetAttendanceAndWorkhabitResult
                            {
                                Id = e.Id,
                                Code = e.Code,
                                Description = e.Description,
                                Type = e.Type,
                                AbsenceCategory = e.AbsenceCategory,
                                Total = e.Total,
                                Students = e.Students,
                            }).ToList()
                        };

                        dataSchoolDay.Add(newLessonSchoolDay);

                    }
                }
                else
                {
                    if (!dataSchoolDay.Where(e => e.Date == itemScheduleLesoon.ScheduleDate && e.Homeroom== homeroom.Grade.Code + homeroom.ClassroomCode).Any())
                    {
                        var listStudentEnrolmentByHomeroom = listStudentEnrolmentBySchedule.Where(e=>e.Homeroom.Id==homeroom.Homeroom.Id).ToList();
                        var listIdHomeroomStudent = listStudentEnrolmentByHomeroom.Select(e => e.IdHomeroomStudent).Distinct().ToList();
                        var listAttendanceEntryBySchedule = listAttendanceEntry
                                                   .Where(e => e.ScheduleDate == itemScheduleLesoon.ScheduleDate
                                                           && listIdHomeroomStudent.Contains(e.IdHomeroomStudent)).ToList();

                        var listAttendanceAndWorkhabit = GetDataAttendanceAndWorkhabit(listAttendaceAndWorkhabit, listAttendanceEntryBySchedule, listStudentEnrolmentByHomeroom);

                        var newLessonSchoolDay = new GetAttendanceSummaryDetailSchoolDayResult
                        {
                            Date = itemScheduleLesoon.ScheduleDate,
                            Homeroom = homeroom.Grade.Code + homeroom.ClassroomCode,
                            DataAttendanceAndWorkhabit = listAttendanceAndWorkhabit.Select(e => new GetAttendanceAndWorkhabitResult
                            {
                                Id = e.Id,
                                Code = e.Code,
                                Description = e.Description,
                                Type = e.Type,
                                AbsenceCategory = e.AbsenceCategory,
                                Total = e.Total,
                                Students = e.Students,
                            }).ToList()
                        };

                        dataSchoolDay.Add(newLessonSchoolDay);
                    }
                }

            }

            var querySchoolDay = dataSchoolDay.Distinct();

            switch (param.OrderBy)
            {
                case "date":
                    querySchoolDay = param.OrderType == OrderType.Desc
                        ? querySchoolDay.OrderByDescending(x => x.Date)
                        : querySchoolDay.OrderBy(x => x.Date);
                    break;
                case "session":
                    querySchoolDay = param.OrderType == OrderType.Desc
                        ? querySchoolDay.OrderByDescending(x => x.Session.Description)
                        : querySchoolDay.OrderBy(x => x.Session.Description);
                    break;
                case "classId":
                    querySchoolDay = param.OrderType == OrderType.Desc
                        ? querySchoolDay.OrderByDescending(x => x.ClassId)
                        : querySchoolDay.OrderBy(x => x.ClassId);
                    break;
                case "homeroom":
                    querySchoolDay = param.OrderType == OrderType.Desc
                        ? querySchoolDay.OrderByDescending(x => x.Homeroom)
                        : querySchoolDay.OrderBy(x => x.Homeroom);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = querySchoolDay
                    .ToList();
            }
            else
            {
                items = querySchoolDay
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : querySchoolDay.Select(x => x.DataAttendanceAndWorkhabit).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        public List<GetAttendanceAndWorkhabitResult> GetDataAttendanceAndWorkhabit(List<GetAttendanceAndWorkhabitResult> listAttendaceAndWorkhabit, List<GetAttendanceEntryBySchoolDayResult> listAttendanceEntryBySchedule, List<GetHomeroom> listStudentEnrolmentBySchedule)
        {
            var AttendanceIdHomeroomStudent = listAttendanceEntryBySchedule.Select(e=>e.IdHomeroomStudent).Distinct().ToList();

            var listStudentUnsubmited = listStudentEnrolmentBySchedule
                                        .Where(e=> !AttendanceIdHomeroomStudent.Contains(e.IdHomeroomStudent))
                                        .GroupBy(e => new
                                        {
                                            IdStudent = e.IdStudent,
                                            FirstName = e.FirstName,
                                            MiddleName = e.MiddleName,
                                            LastName = e.LastName,
                                        })
                                        .Select(e => new GetStudentAttendance
                                        {
                                            Student = new NameValueVm
                                            {
                                                Id = e.Key.IdStudent,
                                                Name = NameUtil.GenerateFullName(e.Key.FirstName, e.Key.MiddleName, e.Key.LastName)
                                            },
                                            Status = "Unsubmited"
                                        }).ToList();

            List<GetAttendanceAndWorkhabitResult> data = new List<GetAttendanceAndWorkhabitResult>();
            foreach (var itemAttendaceAndWorkhabit in listAttendaceAndWorkhabit)
            {
                var type = itemAttendaceAndWorkhabit.Type;

                if (type == "Attendance")
                {
                    var queryAttendanceEntryByType = listAttendanceEntryBySchedule
                                                        .Where(e => e.IdAttendanceMappingAttendance == itemAttendaceAndWorkhabit.Id)
                                                        .GroupBy(e => new
                                                        {
                                                            idStudent = e.IdStudent,
                                                            studentName = NameUtil.GenerateFullName(e.FirstName, e.MiddleName, e.LastName)
                                                        }).Select(e => e.Key);

                    itemAttendaceAndWorkhabit.Total = queryAttendanceEntryByType.Count();

                    itemAttendaceAndWorkhabit.Students = queryAttendanceEntryByType
                        .GroupBy(e => new GetStudentAttendance
                        {
                            Student = new NameValueVm
                            {
                                Id = e.idStudent,
                                Name = e.studentName
                            },
                            Status = itemAttendaceAndWorkhabit.Description
                        }).Select(e=>e.Key).ToList();
                }
                else
                {
                    var listAttendanceEntryWorkhabit = listAttendanceEntryBySchedule
                                                        .Where(e => e.AttendanceEntryWorkhabit.Any(f => f.IdMappingAttendanceWorkhabit == itemAttendaceAndWorkhabit.Id))
                                                        .GroupBy(e => new
                                                        {
                                                            idStudent = e.IdStudent,
                                                            studentName = NameUtil.GenerateFullName(e.FirstName, e.MiddleName, e.LastName)
                                                        }).Select(e=>e.Key);

                    itemAttendaceAndWorkhabit.Total = listAttendanceEntryWorkhabit.Count();

                    itemAttendaceAndWorkhabit.Students = listAttendanceEntryWorkhabit
                        .GroupBy(e => new GetStudentAttendance
                        {
                            Student = new NameValueVm
                            {
                                Id = e.idStudent,
                                Name = e.studentName
                            },
                            Status = itemAttendaceAndWorkhabit.Description
                        }).Select(e=>e.Key).ToList();
                }

                data.Add(itemAttendaceAndWorkhabit);
            }

            data.Add(new GetAttendanceAndWorkhabitResult
            {
                Id = "",
                Code = "Unsubmited",
                Description = "Unsubmited",
                Total = listStudentUnsubmited.Count(),
                Type = "Attendance",
                Students = listStudentUnsubmited
            });

            data.Add(new GetAttendanceAndWorkhabitResult
            {
                Id = "",
                Code = "Pending",
                Description = "Pending",
                Total = listAttendanceEntryBySchedule.Where(e => e.Status == AttendanceEntryStatus.Pending).Any()
                        ? listAttendanceEntryBySchedule.Where(e => e.Status == AttendanceEntryStatus.Pending).Count()
                        : 0,
                Type = "Attendance",
                Students = listAttendanceEntryBySchedule.Where(e => e.Status == AttendanceEntryStatus.Pending)
                    .GroupBy(e => new GetStudentAttendance
                    {
                        Student = new NameValueVm
                        {
                            Id = e.IdStudent,
                            Name = NameUtil.GenerateFullName(e.FirstName, e.MiddleName, e.LastName)
                        },
                        Status = "Pending"
                    }).Select(e=>e.Key).ToList()
            });

            

            return data;
        }
    }
}
