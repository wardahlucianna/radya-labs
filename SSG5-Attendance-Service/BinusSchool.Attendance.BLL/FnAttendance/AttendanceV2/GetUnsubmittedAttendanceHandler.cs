using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetUnsubmittedAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetimeNow;
        private readonly IRedisCache _redisCache;

        public GetUnsubmittedAttendanceHandler(IAttendanceDbContext dbContext, IMachineDateTime datetimeNow, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetimeNow = datetimeNow;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnresolvedAttendanceV2Request>();

            var result = await ExecuteAsync(param);

            return Request.CreateApiResult2(result as object);
        }

        private async Task<GetUnresolvedAttendanceV2Result> ExecuteAsync(GetUnresolvedAttendanceV2Request param)
        {
            var datetime = _datetimeNow.ServerTime;

            var scheduleRealization = await GetScheduleRealizationByUserRedis(param.IdUser);

            var scheduleRealizationChanged = await GetScheduleRealizationChangedRedis(param);

            var listIdLessonRealization = scheduleRealization.Select(x => x.IdLesson).ToList();

            var currentPeriodSemester = await GetCurrentPeriodSemesterRedis();

            var currSemester = currentPeriodSemester != null ? currentPeriodSemester : 1;

            List<string> listIdLesson = new List<string>();
            List<string> listIdHomeroom = new List<string>();
            var ListSchedule = new List<MsSchedule>();
            if (PositionConstant.ClassAdvisor == param.CurrentPosition)
            {
                var listHomeroom = await GetListHomeroomTeacherRedis(param);

                if (!listHomeroom.Any())
                    throw new BadRequestException("You are not Homeroom Teacher");

                listIdHomeroom = listHomeroom.ToList();

                listIdLesson = await GetListIdLessonRedis(listHomeroom);

            }

            if (PositionConstant.SubjectTeacher == param.CurrentPosition)
            {
                var listIdLessonByLessonTeacher = await GetListIdLessonByLessonTeacherRedis(param, listIdLessonRealization);

                if (!listIdLessonByLessonTeacher.Any())
                    throw new BadRequestException("You are not subject teacher");

                ListSchedule = await GetListScheduleRedis(param, listIdLessonByLessonTeacher, listIdLessonRealization);

                listIdLesson = ListSchedule.Select(x => x.IdLesson).Distinct().ToList();

                if (!listIdLesson.Any())
                    throw new BadRequestException("You are not subject teacher");
            }

            var listStudentStatus = await GetListStudentStatusRedis(param);

            var listPeriod = await GetlistPeriodRedis(param);

            var listHomeroomStudentEnrollment = await GetlistHomeroomStudentEnrollmentRedis(param, listIdLesson);

            listHomeroomStudentEnrollment.ForEach(e =>
            {
                e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
            });

            var getTrHomeroomStudentEnrollment = await GetTrHomeroomStudentEnrollmentRedis(param);

            var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                    .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                    .ToList();

            if (PositionConstant.ClassAdvisor == param.CurrentPosition)
            {
                listStudentEnrollmentUnion = listStudentEnrollmentUnion.Where(x => listIdHomeroom.Contains(x.Homeroom.Id)).ToList();
            }

            var listIdlevel = listHomeroomStudentEnrollment.Select(e => e.Level.Id).Distinct().ToList();
            var listIdGrade = listHomeroomStudentEnrollment.Select(e => e.Grade.Id).Distinct().ToList();

            var listScheduleLesoonData = await GetListScheduleLesoonDataRedis(param, datetime, listIdLesson);

            var listScheduleLesoonDataB4FilterSchedule = listScheduleLesoonData;

            if (PositionConstant.SubjectTeacher == param.CurrentPosition)
            {
                listScheduleLesoonData = listScheduleLesoonData.Where(x => ListSchedule.Any(y => y.IdLesson == x.IdLesson && y.IdDay == x.IdDay && y.Sessions.StartTime == x.StartTime && y.Sessions.EndTime == x.EndTime && y.IdUser == param.IdUser)).ToList();
            }

            var listScheduleLesoon = listScheduleLesoonData
                                        .Where(x => !scheduleRealization.Any(y => y.IdLesson == x.IdLesson && y.ScheduleDate == x.ScheduleDate && y.SessionID == x.SessionID))
                                        .ToList();

            //validasi kembali ke lesson teacher idlesson nya
            if (PositionConstant.SubjectTeacher == param.CurrentPosition)
            {
                var listIdLessonByTeacher = await GetListIdLessonByTeacherRedis(param);

                listScheduleLesoon = listScheduleLesoon.Where(x => listIdLessonByTeacher.Contains(x.IdLesson)).ToList();
            }

            foreach (var item in scheduleRealization)
            {
                if (listScheduleLesoonDataB4FilterSchedule.Any(x => x.IdLesson == item.IdLesson && x.SessionID == item.SessionID && x.ScheduleDate == item.ScheduleDate && !item.IsCancel))
                    listScheduleLesoon.Add(listScheduleLesoonDataB4FilterSchedule.Where(x => x.IdLesson == item.IdLesson && x.SessionID == item.SessionID && x.ScheduleDate == item.ScheduleDate && !item.IsCancel).FirstOrDefault());
            }

            listScheduleLesoon = listScheduleLesoon.Where(x => !scheduleRealizationChanged.Any(y => y.IdLesson == x.IdLesson && y.ScheduleDate == x.ScheduleDate && y.SessionID == x.SessionID)).ToList();

            var listMappingAttendance = await GetListMappingAttendanceRedis(listIdlevel);

            var listLessonTeacher = await GetListLessonTeacherRedis(param, listIdLesson, listIdLessonRealization);

            var listAttendanceEntry = await GetListAttendanceEntryRedis(listScheduleLesoon);

            if (listStudentEnrollmentUnion.Any())
                foreach (var item in listAttendanceEntry)
                {
                    item.IdHomeroom = listStudentEnrollmentUnion.Where(x => x.IdHomeroomStudent == item.IdHomeroomStudent).Select(x => x.Homeroom.Id).FirstOrDefault();
                }

            var countStudent = 0;

            var listAnyAttendanceperDate = new List<GetAnyAttendance>();
            List<UnresolvedAttendanceGroupV2Result> attendance = new List<UnresolvedAttendanceGroupV2Result>();
            for (var i = 0; i < listScheduleLesoon.Count(); i++)
            {
                var data = listScheduleLesoon[i];

                if(data.Id== "68e431ae-2253-4014-99e1-437ac767eabb")
                {

                }

                var scheduleRealizationByLesson = scheduleRealization.Where(e => e.IdLesson == data.IdLesson).ToList();

                if (scheduleRealizationByLesson.Any())
                {
                    var scheduleRealizationByDate = scheduleRealizationByLesson.Where(e => data.ScheduleDate >= e.ScheduleDate).OrderBy(e => e.ScheduleDate).ThenBy(e => e.DateIn).LastOrDefault();

                    if (scheduleRealizationByDate != null)
                    {
                        if (scheduleRealizationByDate.IdBinusianSubtitute != param.IdUser)
                            continue;
                    }
                }

                var semester = listPeriod
                                .Where(e => e.IdGrade == data.IdGrade && (e.StartDate.Date <= data.ScheduleDate.Date && e.EndDate.Date >= data.ScheduleDate.Date))
                                .Select(e => e.Semester)
                                .FirstOrDefault();

                var listStatusStudentByDate = listStudentStatus
                                            .Where(e => e.StartDate.Date <= data.ScheduleDate.Date
                                                        && e.EndDate.GetValueOrDefault().Date >= data.ScheduleDate.Date)
                                            .Select(e => e.IdStudent).ToList();

                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, data.ScheduleDate, data.Semester.ToString(), data.IdLesson);

                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                              .ToList();

                var homeroom = studentEnrollmentMoving.FirstOrDefault();

                if (homeroom == null)
                    continue;

                var listAttendanceEntryBySchedule = new List<string>();

                if (param.CurrentPosition == PositionConstant.ClassAdvisor)
                {
                    listAttendanceEntryBySchedule = listAttendanceEntry.Where(e => e.IdScheduleLesson == data.Id && e.IdHomeroom == homeroom.Homeroom.Id).Select(e => e.IdHomeroomStudent).ToList();
                }
                else
                {
                    listAttendanceEntryBySchedule = listAttendanceEntry.Where(e => e.IdScheduleLesson == data.Id).Select(e => e.IdHomeroomStudent).ToList();

                }

                if (param.CurrentPosition == PositionConstant.ClassAdvisor)
                {
                    var listScheduleLesoonByDate = listScheduleLesoon.Where(e => e.ScheduleDate.Date == data.ScheduleDate.Date).ToList();
                    List<string> IdStudentMoving = new List<string>();
                    foreach (var item in listScheduleLesoonByDate)
                    {
                        var listStudentEnrollmentMovingCa_ = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, data.ScheduleDate, data.Semester.ToString(), item.IdLesson);

                        var listStudentEnrollmentMovingCa = listStudentEnrollmentMovingCa_
                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                              .ToList();

                        IdStudentMoving.AddRange(listStudentEnrollmentMovingCa.Select(e => e.IdStudent).Distinct());

                    }

                    countStudent = IdStudentMoving.Distinct().Count();
                }



                if (PositionConstant.SubjectTeacher == param.CurrentPosition)
                {
                    var accessAttendance = listLessonTeacher
                                              .Where(e => e.Lesson.ClassIdGenerated == data.ClassID
                                                      && e.IdLesson == data.IdLesson)
                                              .Select(e => e.IdLesson)
                                              .ToList();

                    if (!accessAttendance.Any())
                        continue;
                }


                if (listAttendanceEntryBySchedule.Any())
                {
                    var studentExcludeEnrollment = studentEnrollmentMoving
                                                    .Where(e => !listAttendanceEntryBySchedule
                                                    .Contains(e.IdHomeroomStudent))
                                                    .ToList();

                    var listIdhomeroomStudentMoving = studentEnrollmentMoving.Select(e => e.IdHomeroomStudent).ToList();
                    var studentIncludeEnrollment = listAttendanceEntryBySchedule.Where(e => listIdhomeroomStudentMoving.Contains(e)).ToList();

                    if (PositionConstant.ClassAdvisor == param.CurrentPosition)
                    {
                        var termAbsent = listMappingAttendance.Where(e => e.IdLevel == homeroom.Level.Id).Select(e => e.AbsentTerms).FirstOrDefault();

                        if (termAbsent == AbsentTerm.Day)
                        {
                            listAnyAttendanceperDate.Add(new GetAnyAttendance
                            {
                                Date = data.ScheduleDate.Date,
                                IdHomeroom = homeroom.Homeroom.Id,
                                IdLevel = data.IdLevel,
                                HomeroomStudents = studentIncludeEnrollment,
                                CountMoving = countStudent,
                            });
                        }
                        else
                        {
                            if (!studentExcludeEnrollment.Any())
                            {
                                if (!listAnyAttendanceperDate.Any(x => x.Date == data.ScheduleDate.Date && x.IdHomeroom == homeroom.Homeroom.Id))
                                {
                                    listAnyAttendanceperDate.Add(new GetAnyAttendance
                                    {
                                        Date = data.ScheduleDate.Date,
                                        IdHomeroom = homeroom.Homeroom.Id,
                                        IdLevel = data.IdLevel
                                    });
                                }
                            }
                        }
                    }

                    if (studentExcludeEnrollment.Any())
                        attendance.Add(new UnresolvedAttendanceGroupV2Result
                        {
                            Date = data.ScheduleDate.Date,
                            ClassID = data.ClassID,
                            TermAbsent = listMappingAttendance.Where(e => e.IdLevel == homeroom.Level.Id).Select(e => e.AbsentTerms).FirstOrDefault(),
                            Homeroom = new ItemValueVm
                            {
                                Id = homeroom.Homeroom.Id,
                                Description = homeroom.Grade.Code + homeroom.ClassroomCode
                            },
                            Session = new ItemValueVm
                            {
                                Id = data.idSession,
                                Description = data.SessionID,
                            },
                            TotalStudent = listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any()
                                            ? studentExcludeEnrollment.Distinct().Count()
                                            : countStudent - listAttendanceEntryBySchedule.Count(),
                            IdLesson = data.IdLesson,
                        });
                }
                else
                {
                    attendance.Add(new UnresolvedAttendanceGroupV2Result
                    {
                        Date = data.ScheduleDate.Date,
                        ClassID = data.ClassID,
                        TermAbsent = listMappingAttendance.Where(e => e.IdLevel == homeroom.Level.Id).Select(e => e.AbsentTerms).FirstOrDefault(),
                        Homeroom = new ItemValueVm
                        {
                            Id = homeroom.Homeroom.Id,
                            Description = homeroom.Grade.Code + homeroom.ClassroomCode
                        },
                        Session = new ItemValueVm
                        {
                            Id = data.idSession,
                            Description = data.SessionID,
                        },
                        TotalStudent = listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any()
                                        ? studentEnrollmentMoving.Select(x => x.IdHomeroomStudent).Distinct().Count()
                                        : countStudent,
                        IdLesson = data.IdLesson,
                    });
                }
            }

            var result = new GetUnresolvedAttendanceV2Result();

            if (param.CurrentPosition == PositionConstant.SubjectTeacher)
            {
                result.IsShowingPopup = listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any();
                result.Attendances = result.IsShowingPopup
                                        ? attendance.Where(e => e.TermAbsent == AbsentTerm.Session && e.TotalStudent > 0).OrderBy(e => e.Date).ToList()
                                        : null;

                if (result.Attendances != null)
                {
                    if (result.Attendances.Any())
                    {
                        var termAbsent = result.Attendances.Select(x => x.TermAbsent).FirstOrDefault();

                        if (termAbsent == AbsentTerm.Session)
                        {
                            var attendances = new List<UnresolvedAttendanceGroupV2Result>();
                            foreach (var item in result.Attendances.Select(x => new { x.Date, x.Session.Id, x.ClassID }).Distinct().ToList())
                            {
                                attendances.Add(result.Attendances.Where(x => x.Date == item.Date && x.Session.Id == item.Id
                                && x.ClassID == item.ClassID).OrderBy(x => x.TotalStudent).FirstOrDefault());
                            }
                            result.Attendances = attendances;
                        }
                    }
                }
            }

            if (param.CurrentPosition == PositionConstant.ClassAdvisor)
            {
                foreach (var item in listAnyAttendanceperDate)
                {
                    var termAbsent = listMappingAttendance.Where(e => e.IdLevel == item.IdLevel).Select(e => e.AbsentTerms).FirstOrDefault();

                    if (termAbsent == AbsentTerm.Day)
                    {
                        var removeAttendance = attendance.Where(e => e.Date == item.Date && e.Homeroom.Id == item.IdHomeroom).ToList();
                        var countMoving = item.CountMoving;
                        var countStudents = listAnyAttendanceperDate.Where(e => e.Date == item.Date).SelectMany(e => e.HomeroomStudents).Distinct().Count();
                        var students = listAnyAttendanceperDate.Where(e => e.Date == item.Date).SelectMany(e => e.HomeroomStudents).Distinct().ToList();

                        if (countMoving == countStudents)
                        {
                            foreach (var itemRemove in removeAttendance)
                            {
                                attendance.Remove(itemRemove);
                            }
                        }
                        else
                        {
                            if (attendance.Any())
                                attendance.Where(e => e.Date == item.Date && e.Homeroom.Id == item.IdHomeroom).First().TotalStudent = countMoving - countStudents;
                        }
                    }
                    else
                    {
                        var removeAttendance = attendance.Where(e => e.Date == item.Date && e.Homeroom.Id == item.IdHomeroom).ToList();

                        foreach (var itemRemove in removeAttendance)
                        {
                            attendance.Remove(itemRemove);
                        }
                    }
                }

                result.IsShowingPopup = listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Day).Any();
                result.Attendances = result.IsShowingPopup
                                        ? attendance
                                            .Where(e => e.TermAbsent == AbsentTerm.Day && e.TotalStudent > 0)
                                            .GroupBy(e => new
                                            {
                                                e.Date,
                                                e.Homeroom.Id,
                                                e.Homeroom.Description,
                                                e.TotalStudent,
                                            })
                                            .Select(e => new UnresolvedAttendanceGroupV2Result
                                            {
                                                Date = e.Key.Date,
                                                Homeroom = new ItemValueVm
                                                {
                                                    Id = e.Key.Id,
                                                    Description = e.Key.Description
                                                },
                                                TotalStudent = e.Key.TotalStudent,
                                            }).OrderBy(e => e.Date).Distinct().ToList()
                                           : null;

                if (result.Attendances != null)
                {
                    if (result.Attendances.Any())
                    {
                        var termAbsent = result.Attendances.Select(x => x.TermAbsent).FirstOrDefault();

                        if (termAbsent == AbsentTerm.Day)
                        {
                            var attendances = new List<UnresolvedAttendanceGroupV2Result>();
                            foreach (var item in result.Attendances.Select(x => x.Date).Distinct().ToList())
                            {
                                attendances.Add(result.Attendances.Where(x => x.Date == item.Date).OrderBy(x => x.TotalStudent).FirstOrDefault());
                            }
                            result.Attendances = attendances;
                        }
                    }
                }
            }

            result.TotalUnsubmitted = result.Attendances == null ? 0 : result.Attendances.Select(e => e.TotalStudent).Sum();

            return result;
        }

        private async Task<List<string>> GetListIdLessonByLessonTeacherRedis(GetUnresolvedAttendanceV2Request param, List<string> listIdLessonRealization)
        {
            var data = await _dbContext.Entity<MsLessonTeacher>()
                     .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                     .Where(e => e.IdUser == param.IdUser
                             && e.Lesson.IdAcademicYear == param.IdAcademicYear
                             && e.IsAttendance || listIdLessonRealization.Contains(e.IdLesson))
                     .Select(e => e.IdLesson)
                     .ToListAsync(CancellationToken);

            return data;
        }

        private async Task<List<string>> GetListIdLessonRedis(List<string> listHomeroom)
        {
            return await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                    .Include(e => e.Lesson)
                                    .Where(e => listHomeroom.Contains(e.HomeroomStudent.IdHomeroom))
                                    .GroupBy(e => e.IdLesson)
                                    .Select(e => e.Key)
                                    .ToListAsync(CancellationToken);
        }

        private async Task<List<string>> GetListHomeroomTeacherRedis(GetUnresolvedAttendanceV2Request param)
        {
            return await _dbContext.Entity<MsHomeroomTeacher>()
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                            .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Where(e => e.IdBinusian == param.IdUser
                                    && e.Homeroom.IdAcademicYear == param.IdAcademicYear
                                    )
                            .Select(e => e.IdHomeroom)
                            .ToListAsync(CancellationToken);
        }

        private async Task<int?> GetCurrentPeriodSemesterRedis()
        {
            var date = _datetimeNow.ServerTime.Date;

            var data = (await _dbContext.Entity<MsPeriod>()
                    .Where(e => e.StartDate <= date && e.EndDate >= date)
                    .Select(e => new
                    {
                        e.Semester
                    })
                    .FirstOrDefaultAsync(CancellationToken))?.Semester;

            return data;
        }

        private async Task<List<TrScheduleRealization2>> GetScheduleRealizationChangedRedis(GetUnresolvedAttendanceV2Request param)
        {
            var status = "Subtituted";

            var list = await _dbContext.Entity<TrScheduleRealization2>()
                .Where(x => x.IdBinusian == param.IdUser && x.Status == status && x.IdBinusian != x.IdBinusianSubtitute)
                .ToListAsync(CancellationToken);

            return list;
        }

        private async Task<List<TrScheduleRealization2>> GetScheduleRealizationByUserRedis(string idUser)
        {
            var data = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.IdBinusianSubtitute == idUser)
                    .ToListAsync(CancellationToken);

            return data;
        }

        private async Task<List<ScheduleAttendanceResult>> GetListAttendanceEntryRedis(List<ScheduleLessonResult> listScheduleLesoon)
        {
            var data = await _dbContext.Entity<TrAttendanceEntryV2>()
                   .Where(e => listScheduleLesoon.Select(x => x.Id).ToList().Contains(e.IdScheduleLesson))
                   .Select(e => new ScheduleAttendanceResult
                   {
                       IdScheduleLesson = e.IdScheduleLesson,
                       IdHomeroomStudent = e.IdHomeroomStudent,
                   })
                   .ToListAsync(CancellationToken);

            return data;
        }

        private async Task<List<MsLessonTeacher>> GetListLessonTeacherRedis(GetUnresolvedAttendanceV2Request param, List<string> listIdLesson, List<string> listIdLessonRealization)
        {
            var data = await _dbContext.Entity<MsLessonTeacher>()
                             .Include(e => e.Lesson)
                             .Where(e => e.IdUser == param.IdUser
                                     && e.Lesson.IdAcademicYear == param.IdAcademicYear
                                     && e.IsAttendance
                                     && listIdLesson.Contains(e.IdLesson) || listIdLessonRealization.Contains(e.IdLesson))
                             .ToListAsync(CancellationToken);
            return data;
        }

        private async Task<List<(string IdLevel, AbsentTerm AbsentTerms)>> GetListMappingAttendanceRedis(List<string> listIdlevel)
        {
            var data = (await _dbContext.Entity<MsMappingAttendance>()
               .Where(e => listIdlevel.Contains(e.IdLevel))
               .Select(e => new
               {
                   e.IdLevel,
                   e.AbsentTerms
               })
               .ToListAsync(CancellationToken))
               .Select(e => (e.IdLevel, e.AbsentTerms)).ToList();

            return data;
        }

        private async Task<List<string>> GetListIdLessonByTeacherRedis(GetUnresolvedAttendanceV2Request param)
        {
            var data = await _dbContext.Entity<MsLessonTeacher>()
                 .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                 .Where(e => e.IdUser == param.IdUser
                         && e.Lesson.IdAcademicYear == param.IdAcademicYear
                         && e.IsAttendance)
                 .Select(e => e.IdLesson)
                 .ToListAsync(CancellationToken);

            return data;
        }

        private async Task<List<ScheduleLessonResult>> GetListScheduleLesoonDataRedis(GetUnresolvedAttendanceV2Request param, DateTime datetime, List<string> listIdLesson)
        {
            return await _dbContext.Entity<MsScheduleLesson>()
                                .Include(e => e.Subject)
                                .Include(e => e.Session)
                                .Include(e => e.Lesson)
                                .Include(e => e.AcademicYear)
                                .Where(e => listIdLesson.Contains(e.IdLesson)
                                        && e.IdAcademicYear == param.IdAcademicYear
                                        && (e.ScheduleDate.Date < datetime.Date || (e.ScheduleDate.Date == datetime.Date && e.StartTime <= datetime.TimeOfDay)))
                                .Select(e => new ScheduleLessonResult
                                {
                                    Id = e.Id,
                                    ScheduleDate = e.ScheduleDate,
                                    IdLesson = e.IdLesson,
                                    ClassID = e.ClassID,
                                    idSession = e.Session.Id,
                                    nameSession = e.Session.Name,
                                    IdGrade = e.IdGrade,
                                    IdLevel = e.IdLevel,
                                    SessionID = e.SessionID,
                                    StartTime = e.Session.StartTime,
                                    EndTime = e.Session.EndTime,
                                    IdDay = e.IdDay,
                                    IdSchool = e.AcademicYear.IdSchool,
                                    Semester = e.Lesson.Semester
                                })
                                .ToListAsync(CancellationToken);
        }

        private async Task<List<GetHomeroom>> GetTrHomeroomStudentEnrollmentRedis(GetUnresolvedAttendanceV2Request param)
        {
            var date = _datetimeNow.ServerTime.Date;
            return await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                           .Include(e => e.SubjectNew)
                           .Include(e => e.LessonNew)
                           .Where(x => x.StartDate.Date <= date && x.LessonOld.IdAcademicYear == param.IdAcademicYear)
                           .Select(e => new GetHomeroom
                           {
                               IdLesson = e.IdLessonNew,
                               IdStudent = e.HomeroomStudent.IdStudent,
                               IdHomeroomStudent = e.IdHomeroomStudent,
                               Homeroom = new ItemValueVm
                               {
                                   Id = e.HomeroomStudent.Homeroom.Id,
                               },
                               ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                               Grade = new CodeWithIdVm
                               {
                                   Id = e.HomeroomStudent.Homeroom.Grade.Id,
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
                               IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                               IsFromMaster = false,
                               EffectiveDate = e.StartDate,
                               IsDelete = e.IsDelete,
                               Datein = e.DateIn.Value,
                               IsShowHistory = e.IsShowHistory,
                           })
                           .ToListAsync(CancellationToken);
        }

        private async Task<List<GetHomeroom>> GetlistHomeroomStudentEnrollmentRedis(GetUnresolvedAttendanceV2Request param, List<string> listIdLesson)
        {
            return await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                               .Where(e => listIdLesson.Contains(e.IdLesson) && e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear)
                               .GroupBy(e => new GetHomeroom
                               {
                                   IdLesson = e.IdLesson,
                                   IdStudent = e.HomeroomStudent.IdStudent,
                                   IdHomeroomStudent = e.HomeroomStudent.Id,
                                   Homeroom = new ItemValueVm
                                   {
                                       Id = e.HomeroomStudent.IdHomeroom,
                                   },
                                   ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
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
                                   IdHomeroomStudentEnrollment = e.Id,
                               })
                               .Select(e => new GetHomeroom
                               {
                                   IdLesson = e.Key.IdLesson,
                                   IdStudent = e.Key.IdStudent,
                                   IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                   Homeroom = new ItemValueVm
                                   {
                                       Id = e.Key.Homeroom.Id,
                                   },
                                   ClassroomCode = e.Key.ClassroomCode,
                                   Grade = new CodeWithIdVm
                                   {
                                       Id = e.Key.Grade.Id,
                                       Code = e.Key.Grade.Code,
                                       Description = e.Key.Grade.Description
                                   },
                                   Level = new CodeWithIdVm
                                   {
                                       Id = e.Key.Level.Id,
                                       Code = e.Key.Level.Code,
                                       Description = e.Key.Level.Description
                                   },
                                   Semester = e.Key.Semester,
                                   IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                   IsFromMaster = true,
                                   IsDelete = false,
                                   IsShowHistory = false,
                               })
                               .ToListAsync(CancellationToken);
        }

        private async Task<List<MsPeriod>> GetlistPeriodRedis(GetUnresolvedAttendanceV2Request param)
        {
            return await _dbContext.Entity<MsPeriod>()
                       .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                       .ToListAsync(CancellationToken);
        }

        private async Task<List<TrStudentStatus>> GetListStudentStatusRedis(GetUnresolvedAttendanceV2Request param)
        {
            var data = await _dbContext.Entity<TrStudentStatus>()
                .Where(e => e.IdAcademicYear == param.IdAcademicYear && e.ActiveStatus)
                .Select(e => new TrStudentStatus
                {
                    IdStudent = e.IdStudent,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate == null
                        ? _datetimeNow.ServerTime.Date
                        : Convert.ToDateTime(e.EndDate),
                    Student = new MsStudent
                    {
                        IdBinusian = e.Student.IdBinusian
                    }
                })
                .ToListAsync(CancellationToken);

            return data;
        }

        private async Task<List<MsSchedule>> GetListScheduleRedis(GetUnresolvedAttendanceV2Request param, List<string> listIdLessonByLessonTeacher, List<string> listIdLessonRealization)
        {
            return await _dbContext.Entity<MsSchedule>()
                 .Include(e => e.Lesson)
                 .Include(e => e.Sessions)
                 .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear && listIdLessonByLessonTeacher.Contains(e.Lesson.Id) || listIdLessonRealization.Contains(e.IdLesson))
                 .Select(e => new MsSchedule { IdLesson = e.IdLesson, IdDay = e.IdDay, Sessions = e.Sessions, IdUser = e.IdUser }).ToListAsync(CancellationToken);
        }
    }

    public class GetAnyAttendance
    {
        public DateTime Date { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLevel { get; set; }
        public List<string> HomeroomStudents { get; set; }
        public int CountMoving { get; set; }
    }
}
