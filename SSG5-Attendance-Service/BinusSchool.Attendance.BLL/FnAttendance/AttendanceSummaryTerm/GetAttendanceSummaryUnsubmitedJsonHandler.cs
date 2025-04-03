using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm;
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
    public class GetAttendanceSummaryUnsubmitedJsonHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        private readonly IMachineDateTime _datetime;
        public GetAttendanceSummaryUnsubmitedJsonHandler(IAttendanceDbContext dbContext, IRedisCache redisCache, IMachineDateTime datetime)
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
                queryScheduleLesoon = queryScheduleLesoon.Where(e => (e.ScheduleDate.Date >= Convert.ToDateTime(param.StartDate).Date && e.ScheduleDate.Date <= Convert.ToDateTime(param.EndDate).Date));
            else
                queryScheduleLesoon = queryScheduleLesoon.Where(e => e.ScheduleDate.Date <= _datetime.ServerTime.Date);

            if (!string.IsNullOrEmpty(param.ClassId))
                queryScheduleLesoon = queryScheduleLesoon.Where(e => e.ClassID == param.ClassId);

            var listScheduleLesoon = queryScheduleLesoon.ToList();

            var listIdScheduleLesson = listScheduleLesoon.Select(e => e.Id).ToList();

            var listAttendanceEntry = redisAttendanceEntry
                              .Where(e => listIdLesson.Contains(e.IdLesson) && listIdScheduleLesson.Contains(e.IdScheduleLesson))
                              .GroupBy(e => new
                              {
                                  IdScheduleLesson = e.IdScheduleLesson,
                                  IdHomeroomStudent = e.IdHomeroomStudent,
                              })
                              .Select(e => new RedisAttendanceSummaryAttendanceEntryResult
                              {
                                  IdScheduleLesson = e.Key.IdScheduleLesson,
                                  IdHomeroomStudent = e.Key.IdHomeroomStudent,
                              })
                              .ToList();

            var listMappingAttendance = redisMappingAttendance.Where(e => listIdlevel.Contains(e.IdLevel)).ToList();

            var listLessonTeacher = redisLessonTeacher.Where(e => e.IdUserTeacher == param.IdUser && listIdLesson.Contains(e.IdLesson)).ToList();

            var unsubmitedRequest = new AttendanceSummaryUnsubmitedRequest
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

            var listUnsubmited = GetUnsubmited(unsubmitedRequest).Result;

            var ListStudent = listUnsubmited.SelectMany(e => e.ListStudent.Select(e => e)).GroupBy(e => e).ToList();

            var listStudentAttendance = new List<GetStudentByAttendance>();
            foreach (var item in ListStudent.Select(e => e.Key).OrderBy(e => e).ToList())
            {
                listStudentAttendance.Add(new GetStudentByAttendance
                {
                    IdStudent = item,
                    Unsubmited = listUnsubmited.Where(e => e.ListStudent.Any(f => f == item)).OrderBy(e => e.Date).ToList()
                });
            }

            var json = JsonSerializer.Serialize(listStudentAttendance);

            return Request.CreateApiResult2(json as object);
        }

        public static async Task<List<GetAttendanceSummaryUnsubmitedResult>> GetUnsubmited(AttendanceSummaryUnsubmitedRequest listAttendanceSummaryUnsubmitedRequest)
        {
            List<string> idLessonNoHomeroom = new List<string>();
            List<string> idLessonNoHomeroomGrouping = new List<string>();
            List<GetAttendanceSummaryUnsubmitedResult> listUnsubmited = new List<GetAttendanceSummaryUnsubmitedResult>();
            var listAnyAttendanceperDate = new List<GetAnyAttendance>();

            foreach (var itemScheduleLesoon in listAttendanceSummaryUnsubmitedRequest.ListScheduleLesoon)
            {
                var listStatusStudentByDate = listAttendanceSummaryUnsubmitedRequest.ListStaudetStatus
                                                .Where(e => e.StartDate.Date <= itemScheduleLesoon.ScheduleDate.Date && e.EndDate.Date >= itemScheduleLesoon.ScheduleDate)
                                                .Select(e => e.IdStudent).ToList();

                //moving Student
                var listStudentEnrolmentBySchedule = listAttendanceSummaryUnsubmitedRequest.ListHomeroomStudentEnrollment
                                            .Where(e => listStatusStudentByDate.Contains(e.IdStudent) && e.Semester == itemScheduleLesoon.Semester)
                                            .ToList();

                var listTrStudentEnrolmentBySchedule = listAttendanceSummaryUnsubmitedRequest.ListTrHomeroomStudentEnrollment
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

                var listIdHomeroomStudent = studentEnrollmentMoving.Select(e => e.IdHomeroomStudent).OrderBy(e => e).ToList();
                var listIdStudent = studentEnrollmentMoving.Select(e => e.IdStudent).OrderBy(e => e).ToList();

                var listAttendanceEntryBySchedule = listAttendanceSummaryUnsubmitedRequest.ListAttendanceEntry
                                                    .Where(e => e.IdScheduleLesson == itemScheduleLesoon.Id
                                                            && listIdHomeroomStudent.Contains(e.IdHomeroomStudent))
                                                    .Select(e => e.IdHomeroomStudent).ToList();

                var homeroom = studentEnrollmentMoving.FirstOrDefault();
                var teacher = listAttendanceSummaryUnsubmitedRequest.ListSchedule.Where(e => e.IdLesson == itemScheduleLesoon.IdLesson
                                                    && e.IdSession == itemScheduleLesoon.Session.Id
                                                    && e.IdWeek == itemScheduleLesoon.IdWeek
                                                    && e.IdDay == itemScheduleLesoon.IdDay
                                                ).FirstOrDefault();

                if (teacher == null)
                    continue;

                RedisAttendanceSummaryHomeroomTeacherResult homeroomTeacher = default;
                if (homeroom != null)
                {
                    homeroomTeacher = listAttendanceSummaryUnsubmitedRequest.ListHomeroomTeacher
                                        .Where(e => e.IdHomeroom == homeroom.Homeroom.Id && e.IsAttendance)
                                        .FirstOrDefault();
                }

                if (PositionConstant.SubjectTeacher == listAttendanceSummaryUnsubmitedRequest.SelectedPosition)
                {
                    var accessAttendance = listAttendanceSummaryUnsubmitedRequest.ListLessonTeacher
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

                var listStudent = listAttendanceSummaryUnsubmitedRequest.ListMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any()
                            ? studentEnrollmentMoving.Where(e => !listAttendanceEntryBySchedule.Contains(e.IdHomeroomStudent)).Select(e => e.IdStudent).Distinct().ToList()
                            : listAttendanceSummaryUnsubmitedRequest.ListHomeroomStudentEnrollment.Where(e => e.Homeroom.Id == homeroom.Homeroom.Id && listStatusStudentByDate.Contains(e.IdStudent) && !listAttendanceEntryBySchedule.Contains(e.IdHomeroomStudent)).Select(e => e.IdStudent).Distinct().ToList();

                if (studentEnrollmentMoving.Any())
                {
                    if (listIdHomeroomStudent.Count() != listAttendanceEntryBySchedule.Count())
                        listUnsubmited.Add(new GetAttendanceSummaryUnsubmitedResult
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
                    else
                    {
                        if (!listAnyAttendanceperDate.Any(x => x.Date == itemScheduleLesoon.ScheduleDate.Date && x.IdHomeroom == homeroom.Homeroom.Id))
                        {
                            listAnyAttendanceperDate.Add(new GetAnyAttendance
                            {
                                Date = itemScheduleLesoon.ScheduleDate.Date,
                                IdHomeroom = homeroom.Homeroom.Id
                            });
                        }
                    }

                }
                else
                {
                    listUnsubmited.Add(new GetAttendanceSummaryUnsubmitedResult
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
            }


            if (listAttendanceSummaryUnsubmitedRequest.ListMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Day).Any())
            {
                foreach (var item in listAnyAttendanceperDate)
                {
                    var removeAttendance = listUnsubmited.Where(e => e.Date == item.Date && e.Homeroom.Id == item.IdHomeroom).ToList();

                    foreach (var itemRemove in removeAttendance)
                    {
                        listUnsubmited.Remove(itemRemove);
                    }
                }
            }

            return listUnsubmited;
        }
    }

    public class GetStudentByAttendance
    {
        public string IdStudent { get; set; }
        public int Total { get; set; }
        public List<GetAttendanceSummaryUnsubmitedResult> Unsubmited { get; set; }
    }
}
