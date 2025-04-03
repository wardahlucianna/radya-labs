using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using DocumentFormat.OpenXml.Math;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummarySubmitedHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IMachineDateTime _datetime;
        public GetAttendanceSummarySubmitedHandler(IAttendanceDbContext dbContext, IRedisCache redisCache, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryUnsubmitedRequest>();
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
            var redisScheduleLesson = await AttendanceSummaryRedisCacheHandler.GetScheduleLesson(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime.Date);
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisLessonTeacher = await AttendanceSummaryRedisCacheHandler.GetLessonTeacher(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisHomeroomTeacher = await AttendanceSummaryRedisCacheHandler.GetHomeroomTeacher(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var queryRedisHomeroomStudentEnrollment = redisHomeroomStudentEnrollment.Distinct();
            var queryRedisTrHomeroomStudentEnrollment = redisTrHomeroomStudentEnrollment.Distinct();
            if (!string.IsNullOrEmpty(param.IdGrade))
            {
                queryRedisHomeroomStudentEnrollment = queryRedisHomeroomStudentEnrollment.Where(e => e.Grade.Id == param.IdGrade);
                queryRedisTrHomeroomStudentEnrollment = queryRedisTrHomeroomStudentEnrollment.Where(e => e.Grade.Id == param.IdGrade);
            }

            if (!string.IsNullOrEmpty(param.IdHomeroom))
            {
                queryRedisHomeroomStudentEnrollment = queryRedisHomeroomStudentEnrollment.Where(e => e.Homeroom.Id == param.IdHomeroom);
                queryRedisTrHomeroomStudentEnrollment = queryRedisTrHomeroomStudentEnrollment.Where(e => e.Homeroom.Id == param.IdHomeroom);
            }

            if (!string.IsNullOrEmpty(param.IdSubject))
            {
                queryRedisHomeroomStudentEnrollment = queryRedisHomeroomStudentEnrollment.Where(e => e.IdSubject == param.IdSubject);
                queryRedisTrHomeroomStudentEnrollment = queryRedisTrHomeroomStudentEnrollment.Where(e => e.IdSubject == param.IdSubject);
            }

            if (!string.IsNullOrEmpty(param.IdClassroom))
            {
                queryRedisHomeroomStudentEnrollment = queryRedisHomeroomStudentEnrollment.Where(e => e.IdClassroom == param.IdClassroom);
                queryRedisTrHomeroomStudentEnrollment = queryRedisTrHomeroomStudentEnrollment.Where(e => e.IdClassroom == param.IdClassroom);
            }

            redisHomeroomStudentEnrollment = queryRedisHomeroomStudentEnrollment.ToList();
            redisTrHomeroomStudentEnrollment = queryRedisTrHomeroomStudentEnrollment.ToList();

            var listIdlevel = redisHomeroomStudentEnrollment.Select(e => e.Level.Id).Distinct().ToList();
            var listIdGrade = redisHomeroomStudentEnrollment.Select(e => e.Grade.Id).Distinct().ToList();

            var queryScheduleLesoon = redisScheduleLesson.Where(e => listIdLesson.Contains(e.IdLesson));

            if (!string.IsNullOrEmpty(param.StartDate.ToString()) && !string.IsNullOrEmpty(param.EndDate.ToString()))
                queryScheduleLesoon = queryScheduleLesoon.Where(e => e.ScheduleDate.Date >= Convert.ToDateTime(param.StartDate).Date && e.ScheduleDate.Date <= Convert.ToDateTime(param.EndDate).Date);
            else
                queryScheduleLesoon = queryScheduleLesoon.Where(e => e.ScheduleDate.Date <= _datetime.ServerTime.Date);

            if (!string.IsNullOrEmpty(param.ClassId))
                queryScheduleLesoon = queryScheduleLesoon.Where(e => e.ClassID == param.ClassId);

            var listScheduleLesoon = queryScheduleLesoon.ToList();

            var listIdScheduleLesson = listScheduleLesoon.Select(e => e.Id).ToList();

            var listAttendanceEntry = redisAttendanceEntry
                              .Where(e => listIdLesson.Contains(e.IdLesson) && listIdScheduleLesson.Contains(e.IdScheduleLesson) && e.Status == AttendanceEntryStatus.Submitted)
                              .GroupBy(e => new
                              {
                                  e.IdScheduleLesson,
                                  e.IdHomeroomStudent,
                                  e.IdStudent,
                              })
                              .Select(e => new RedisAttendanceSummaryAttendanceEntryResult
                              {
                                  IdScheduleLesson = e.Key.IdScheduleLesson,
                                  IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                  IdStudent = e.Key.IdStudent,
                              })
                              .ToList();

            var listMappingAttendance = redisMappingAttendance.Where(e => listIdlevel.Contains(e.IdLevel)).ToList();

            var listLessonTeacher = redisLessonTeacher.Where(e => e.IdUserTeacher == param.IdUser && listIdLesson.Contains(e.IdLesson)).ToList();

            var submitedRequest = new AttendanceSummaryUnsubmitedRequest
            {
                ListScheduleLesoon = listScheduleLesoon,
                ListStaudetStatus = redisStudentStatus,
                ListHomeroomStudentEnrollment = redisHomeroomStudentEnrollment,
                ListTrHomeroomStudentEnrollment = redisTrHomeroomStudentEnrollment,
                ListAttendanceEntry = listAttendanceEntry,
                ListSchedule = redisSchedule,
                ListMappingAttendance = listMappingAttendance,
                SelectedPosition = param.SelectedPosition,
                ListLessonTeacher = listLessonTeacher,
                ListHomeroomTeacher = redisHomeroomTeacher,
            };

            var listSubmited = GetSubmited(submitedRequest).Result;

            IEnumerable<GetAttendanceSummaryUnsubmitedResult> querySubmited = default;

            var ListStudent = listSubmited.SelectMany(e => e.ListStudent.Select(e => e)).GroupBy(e => e).ToList();
            var listStudentAttendance = new List<GetStudentByAttendance>();
            foreach (var item in ListStudent.Select(e => e.Key).OrderBy(e => e).ToList())
                listStudentAttendance.Add(new GetStudentByAttendance
                {
                    IdStudent = item,
                    Total = listSubmited
                                .Where(e => e.ListStudent.Any(f => f == item))
                                .OrderBy(e => e.Date)
                                .Select(e => new GetAttendanceSummaryUnsubmitedResult
                                {
                                    Date = e.Date,
                                    ClassID = e.ClassID,
                                    Session = e.Session,
                                    HomeroomTeacher = e.HomeroomTeacher,
                                    Teacher = e.Teacher,
                                })
                                .Count(),
                    //Unsubmited = listSubmited
                    //            .Where(e => e.ListStudent.Any(f => f == item))
                    //            .OrderBy(e => e.Date)
                    //            .Select(e=>new GetAttendanceSummaryUnsubmitedResult
                    //            {
                    //                Date  = e.Date,
                    //                ClassID = e.ClassID,
                    //                Session = e.Session,
                    //                HomeroomTeacher = e.HomeroomTeacher,
                    //                Teacher = e.Teacher,
                    //            })
                    //            .ToList()
                });

            var json1 = JsonSerializer.Serialize(listStudentAttendance);

            if (redisMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any())
            {
                querySubmited = listSubmited
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
                                    .Select(e => new GetAttendanceSummaryUnsubmitedResult
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
                    querySubmited = querySubmited
                                        .Where(e => e.ClassID.ToLower().Contains(param.Search)
                                        || e.Teacher.Id.ToLower().Contains(param.Search)
                                        || e.Teacher.Description.ToLower().Contains(param.Search));
            }
            else
            {
                querySubmited = listSubmited
                                    .GroupBy(e => new
                                    {
                                        e.Date,
                                        idHomeroom = e.Homeroom.Id,
                                        homeroom = e.Homeroom.Description,
                                        teacherId = e.HomeroomTeacher.Id,
                                        teacherName = e.HomeroomTeacher.Description
                                    })
                                    .Select(e => new GetAttendanceSummaryUnsubmitedResult
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
                                        TotalStudent = listSubmited.Where(f => f.Date == e.Key.Date && f.Homeroom.Id == e.Key.idHomeroom).Select(f => f.TotalStudent).Max(),
                                    }).OrderBy(e => e.Date).Distinct();

                if (!string.IsNullOrEmpty(param.Search))
                    querySubmited = querySubmited
                                        .Where(e => e.Teacher.Description.ToLower().Contains(param.Search)
                                            || e.Teacher.Id.ToLower().Contains(param.Search)
                                            || e.Homeroom.Description.ToLower().Contains(param.Search));
            }


            switch (param.OrderBy)
            {
                case "date":
                    querySubmited = param.OrderType == OrderType.Desc
                        ? querySubmited.OrderByDescending(x => x.Date)
                        : querySubmited.OrderBy(x => x.Date);
                    break;
                case "clasId":
                    querySubmited = param.OrderType == OrderType.Desc
                        ? querySubmited.OrderByDescending(x => x.ClassID)
                        : querySubmited.OrderBy(x => x.ClassID);
                    break;
                case "teacher":
                    querySubmited = param.OrderType == OrderType.Desc
                        ? querySubmited.OrderByDescending(x => x.Teacher.Description)
                        : querySubmited.OrderBy(x => x.Teacher.Description);
                    break;
                case "homeroom":
                    querySubmited = param.OrderType == OrderType.Desc
                        ? querySubmited.OrderByDescending(x => x.Homeroom.Description)
                        : querySubmited.OrderBy(x => x.Homeroom.Description);
                    break;
                case "subjectId":
                    querySubmited = param.OrderType == OrderType.Desc
                        ? querySubmited.OrderByDescending(x => x.SubjectId)
                        : querySubmited.OrderBy(x => x.SubjectId);
                    break;
                case "sessionId":
                    querySubmited = param.OrderType == OrderType.Desc
                        ? querySubmited.OrderByDescending(x => x.Session.SessionID)
                        : querySubmited.OrderBy(x => x.Session.SessionID);
                    break;
                case "totalstudent":
                    querySubmited = param.OrderType == OrderType.Desc
                        ? querySubmited.OrderByDescending(x => x.TotalStudent)
                        : querySubmited.OrderBy(x => x.TotalStudent);
                    break;
            };

            var json = JsonSerializer.Serialize(querySubmited.ToList());

            return Request.CreateApiResult2(json as object);
        }

        public static async Task<List<GetAttendanceSummaryUnsubmitedResult>> GetSubmited(AttendanceSummaryUnsubmitedRequest listAttendanceSummarySubmitedRequest)
        {
            var idLessonNoHomeroom = new List<string>();
            var idLessonNoHomeroomGrouping = new List<string>();
            var listSubmited = new List<GetAttendanceSummaryUnsubmitedResult>();

            foreach (var itemScheduleLesoon in listAttendanceSummarySubmitedRequest.ListScheduleLesoon)
            {
                var listStatusStudentByDate = listAttendanceSummarySubmitedRequest.ListStaudetStatus
                                                .Where(e => e.StartDate.Date <= itemScheduleLesoon.ScheduleDate.Date && e.EndDate.Date >= itemScheduleLesoon.ScheduleDate)
                                                .Select(e => e.IdStudent).ToList();

                //moving Student
                var listStudentEnrolmentBySchedule = listAttendanceSummarySubmitedRequest.ListHomeroomStudentEnrollment
                                            .Where(e => listStatusStudentByDate.Contains(e.IdStudent) && e.Semester == itemScheduleLesoon.Semester)
                                            .ToList();

                var listTrStudentEnrolmentBySchedule = listAttendanceSummarySubmitedRequest.ListTrHomeroomStudentEnrollment
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

                var listIdHomeroomStudent = studentEnrollmentMoving.Select(e => e.IdHomeroomStudent).ToList();

                var listAttendanceEntryBySchedule = listAttendanceSummarySubmitedRequest.ListAttendanceEntry
                                                    .Where(e => e.IdScheduleLesson == itemScheduleLesoon.Id
                                                            && listIdHomeroomStudent.Contains(e.IdHomeroomStudent))
                                                    .ToList();

                var homeroom = studentEnrollmentMoving.FirstOrDefault();
                var teacher = listAttendanceSummarySubmitedRequest.ListSchedule.Where(e => e.IdLesson == itemScheduleLesoon.IdLesson
                                                    && e.IdSession == itemScheduleLesoon.Session.Id
                                                    && e.IdWeek == itemScheduleLesoon.IdWeek
                                                    && e.IdDay == itemScheduleLesoon.IdDay
                                                ).FirstOrDefault();

                if (listAttendanceSummarySubmitedRequest.ListMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Day).Any())
                {
                    var listStudentByHomeroom = studentEnrollmentMoving.Where(e => e.Homeroom.Id == homeroom.Homeroom.Id).Select(e => e.IdStudent).ToList();

                    listStatusStudentByDate = listStatusStudentByDate.Where(e => listStudentByHomeroom.Contains(e)).ToList();
                }

                if (teacher == null)
                    continue;

                RedisAttendanceSummaryHomeroomTeacherResult homeroomTeacher = default;
                if (homeroom != null)
                    homeroomTeacher = listAttendanceSummarySubmitedRequest.ListHomeroomTeacher
                                        .Where(e => e.IdHomeroom == homeroom.Homeroom.Id && e.IsAttendance)
                                        .FirstOrDefault();

                if (PositionConstant.SubjectTeacher == listAttendanceSummarySubmitedRequest.SelectedPosition)
                {
                    var accessAttendance = listAttendanceSummarySubmitedRequest.ListLessonTeacher
                                              .Where(e => e.ClassId == itemScheduleLesoon.ClassID
                                                      && e.IdLesson == itemScheduleLesoon.IdLesson)
                                              .Select(e => e.IdLesson)
                                              .ToList();

                    if (!accessAttendance.Any())
                        continue;
                }

                if (homeroom == null)
                {
                    if (!idLessonNoHomeroom.Where(e => e == itemScheduleLesoon.IdLesson).Any())
                        idLessonNoHomeroom.Add(itemScheduleLesoon.IdLesson);

                    idLessonNoHomeroomGrouping.Add(itemScheduleLesoon.IdLesson);
                    continue;
                }

                var listStudent = listAttendanceSummarySubmitedRequest.ListMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any()
                            ? listAttendanceEntryBySchedule.Select(e => e.IdStudent).Distinct().ToList()
                            : listAttendanceEntryBySchedule.Where(e => listStatusStudentByDate.Contains(e.IdStudent)).Select(e => e.IdStudent).Distinct().ToList();

                if (studentEnrollmentMoving.Any())
                    if (listStudent.Any())
                        listSubmited.Add(new GetAttendanceSummaryUnsubmitedResult
                        {
                            Date = itemScheduleLesoon.ScheduleDate.Date,
                            ClassID = itemScheduleLesoon.ClassID,
                            Teacher = new ItemValueVm
                            {
                                Id = teacher.Teacher.IdUser,
                                Description = NameUtil.GenerateFullName(teacher.Teacher.FirstName, teacher.Teacher.LastName)
                            },
                            HomeroomTeacher = homeroomTeacher == null ? default : new ItemValueVm
                            {
                                Id = homeroomTeacher != null ? homeroomTeacher.Teacher.IdUser : string.Empty,
                                Description = homeroomTeacher != null ? NameUtil.GenerateFullName(homeroomTeacher.Teacher.FirstName, homeroomTeacher.Teacher.LastName) : string.Empty
                            },
                            Homeroom = new ItemValueVm
                            {
                                Id = homeroom.Homeroom.Id,
                                Description = homeroom.Homeroom.Description,
                            },
                            SubjectId = itemScheduleLesoon.Subject.SubjectID,
                            Session = new RedisAttendanceSummarySession
                            {
                                Id = itemScheduleLesoon.Session.Id,
                                Name = itemScheduleLesoon.Session.Name,
                                SessionID = itemScheduleLesoon.Session.SessionID.ToString(),
                            },
                            TotalStudent = listStudent.Count(),
                            ListStudent = listStudent
                        });
            }

            return listSubmited;
        }
    }
}
