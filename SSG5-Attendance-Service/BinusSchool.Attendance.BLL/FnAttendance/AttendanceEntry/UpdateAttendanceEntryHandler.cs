using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Validator;
using BinusSchool.Attendance.FnAttendance.ClassSession;
using BinusSchool.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.ClassSession;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry
{
    public class UpdateAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private GetMapAttendanceDetailResult _mapAttendance;
        private string _currentPosition;
        private List<TrAttendanceEntry> _attendanceEntries = new List<TrAttendanceEntry>();
        private readonly IAttendanceDbContext _dbContext;
        private readonly GetMapAttendanceDetailHandler _mapAttendanceHandler;
        private readonly GetClassSessionHandler _classSessionHandler;
        private readonly IMachineDateTime _machineDateTime;
        private readonly ILogger<UpdateAttendanceEntryHandler> _logger;
        public UpdateAttendanceEntryHandler(IAttendanceDbContext dbContext, GetMapAttendanceDetailHandler mapAttendanceHandler, GetClassSessionHandler classSessionHandler, IMachineDateTime machineDateTime, ILogger<UpdateAttendanceEntryHandler> logger)
        {
            _dbContext = dbContext;
            _mapAttendanceHandler = mapAttendanceHandler;
            _classSessionHandler = classSessionHandler;
            _machineDateTime = machineDateTime;
            _logger = logger;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateAttendanceEntryRequest>();
            string idLevel = null;
            if (body.Id is null && body.ClassId is null)
                throw new BadRequestException("at least data homeroom / class id must be inputted");

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x
                => x.IsGenerated
                   && EF.Functions.DateDiffDay(x.ScheduleDate, body.Date) == 0);

            if (body.Id != null)
            {

                idLevel = await _dbContext.Entity<MsHomeroom>()
                                          .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade)
                                          .Where(x => x.Id == body.Id)
                                          .Select(x => x.GradePathwayClassroom.GradePathway.Grade.IdLevel)
                                          .FirstOrDefaultAsync(CancellationToken);

                predicate = predicate.And(x => x.IdHomeroom == body.Id);
            }
            else if (body.ClassId != null)
            {
                idLevel = await (from _gsl in _dbContext.Entity<TrGeneratedScheduleLesson>()
                                 join _l in _dbContext.Entity<MsLesson>() on _gsl.IdLesson equals _l.Id
                                 join _g in _dbContext.Entity<MsGrade>() on _l.IdGrade equals _g.Id
                                 join _a in _dbContext.Entity<MsAcademicYear>() on _l.IdAcademicYear equals _a.Id
                                 where
                                      _gsl.ClassID == body.ClassId
                                      && _gsl.ScheduleDate.Date == body.Date
                                      && _a.IdSchool == body.IdSchool
                                 select _g.IdLevel
                           ).FirstOrDefaultAsync(CancellationToken);
            }

            if (idLevel is null)
                throw new NotFoundException("data homeroom / class id is not found");

            _mapAttendance = await _mapAttendanceHandler.GetMapAttendanceDetail(idLevel, CancellationToken);
            if (_mapAttendance is null)
            {
                var level = await _dbContext.Entity<MsLevel>().Where(x => x.Id == idLevel).Select(x => x.Description).FirstOrDefaultAsync(CancellationToken);
                throw new BadRequestException($"Mapping attendance for level {level ?? idLevel} is not available.");
            }

            (await new UpdateAttendanceEntryValidator(_mapAttendance).ValidateAsync(body)).EnsureValid();
            _currentPosition = body.CurrentPosition;

            var idScheduleLessons = body.Entries.Select(x => x.IdGeneratedScheduleLesson);

            // absent term: Session, require ClassId & IdSession
            var currPredicate = _mapAttendance.Term == AbsentTerm.Day
                ? predicate.And(x => idScheduleLessons.Contains(x.Id))
                : predicate.And(x => idScheduleLessons.Contains(x.Id)).And(CreateSessionPredicate(body.ClassId, body.IdSession));

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                .Include(x => x.AttendanceEntries)
                    .ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                .OrderBy(x => x.StartTime);
            var scheduleLessons = await query.Where(currPredicate).ToListAsync(CancellationToken);

            // throw when any generated schedule lessson that not found
            var notFoundScheduleLessons = idScheduleLessons.Except(scheduleLessons.Select(x => x.Id));
            if (notFoundScheduleLessons.Any())
                throw new BadRequestException(
                    string.Format(Localizer["ExNotExist"], Localizer["Lesson"], "Id", string.Join(", ", notFoundScheduleLessons)));

            // make sure no multiple student from requested entries
            var duplicateEntries = scheduleLessons.GroupBy(x => x.GeneratedScheduleStudent.IdStudent).Where(x => x.Count() > 1)
                .Select(x => GetStudentName(x.First()));
            if (duplicateEntries.Any())
                throw new BadRequestException($"You entered multiple entry for student(s) {string.Join(", ", duplicateEntries)}.");

            var termSessionValidation = default(MapAttendanceDetailValidation);
            var termSessionNext = default(SessionOfClass);
            var idStudents = scheduleLessons.Select(x => x.GeneratedScheduleStudent.IdStudent).Distinct();

            if (_mapAttendance.Term == AbsentTerm.Day)
            {
                // get additional schedule lesson per day
                var scheduleLessonsPerDay = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                    .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                    .Include(x => x.AttendanceEntries)
                        .ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                    .Where(x => x.IdHomeroom == body.Id
                        && x.ScheduleDate.Date == body.Date.Date
                        && idStudents.Contains(x.GeneratedScheduleStudent.IdStudent)
                        && !idScheduleLessons.Contains(x.Id)) // exclude already queried schedule lesson
                    .ToListAsync(CancellationToken);

                scheduleLessons.AddRange(scheduleLessonsPerDay);
                var idHomeroomTacher = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Include(x => x.Homeroom)
                    .Where(x => x.Homeroom.Id == body.Id)
                    .Where(x => x.IsAttendance)
                    .Where(x => x.IdBinusian == AuthInfo.UserId)
                    .Select(x => x.IdBinusian).FirstOrDefaultAsync(CancellationToken);
                // if (idHomeroomTacher != AuthInfo.UserId)
                //     throw new UnauthorizedAccessException("you are not the homeroom teacher in this class");
            }
            else if (_mapAttendance.Term == AbsentTerm.Session)
            {
                // get all assignment for current user
                var assigments = new List<string>();
                var anyHomeroomAssignment = await _dbContext.Entity<MsLessonTeacher>()
                    .AnyAsync(x => x.IdUser == AuthInfo.UserId && x.IsAttendance);
                if (anyHomeroomAssignment)
                    assigments.Add(PositionConstant.ClassAdvisor);

                var idSubjects = scheduleLessons.Select(x => x.IdSubject).Distinct();
                var anySubjectAssignment = await _dbContext.Entity<MsLessonTeacher>()
                    .AnyAsync(x => x.IdUser == AuthInfo.UserId && idSubjects.Contains(x.Lesson.IdSubject) && x.IsAttendance);
                if (anySubjectAssignment)
                    assigments.Add(PositionConstant.SubjectTeacher);

                // check if current user can fill attendance
                termSessionValidation = _mapAttendance.TermSession.Validations.FirstOrDefault(x => x.AbsentBy.Code == body.CurrentPosition);
                // if (termSessionValidation is null || !assigments.Contains(termSessionValidation.AbsentBy.Code))
                //     throw new UnauthorizedAccessException("You don't have access to entry this attendance.");

                // check if current attendance have next session
                if (body.CopyToNextSession)
                {
                    var availClassSessions = await _classSessionHandler.GetClassAndSessions(new GetClassSessionRequest
                    {
                        IdHomeroom = body.Id,
                        IdUser = AuthInfo.UserId,
                        IdSchool = body.IdSchool,
                        Date = body.Date
                    });
                    var availSessions = availClassSessions.First(x => x.ClassId == body.ClassId).Sessions;
                    var (currentSession, currentSessionIndex) = availSessions
                        .SelectWhere(x => x.Id == body.IdSession, (x, y) => (currentSession: x, currentSessionIndex: y))
                        .First();
                    termSessionNext = availSessions.Skip(currentSessionIndex + 1).Take(1).FirstOrDefault();
                    if (termSessionNext is null)
                        throw new BadRequestException("Current attendance doesn't have next session.");

                    var nextPredicate = predicate
                        .And(x => !idScheduleLessons.Contains(x.Id))
                        .And(CreateSessionPredicate(body.ClassId, termSessionNext.Id))
                        .And(x => idStudents.Contains(x.GeneratedScheduleStudent.IdStudent));
                    var nextScheduleLessons = await query.Where(nextPredicate).ToListAsync(CancellationToken);

                    scheduleLessons.AddRange(nextScheduleLessons);
                }
            }
            var idGenerateSchedulelesson = body.Entries.Select(x => x.IdGeneratedScheduleLesson).Distinct();
            var idStudentByScheduleLesson = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Where(x => x.IsGenerated && idGenerateSchedulelesson.Contains(x.Id))
                .GroupBy(x => new
                {
                    x.Id,
                    x.GeneratedScheduleStudent.IdStudent
                })
                .Select(x => new
                {
                    x.Key.Id,
                    x.Key.IdStudent
                })
                .ToListAsync(CancellationToken);

            var idGeneratedScheduleLesson = new List<string>();

            foreach (var entry in body.Entries)
            {
                if (_mapAttendance.Term == AbsentTerm.Day)
                {
                    // update attenance entry per student
                    var idStudent = scheduleLessons.Find(x => x.Id == entry.IdGeneratedScheduleLesson).GeneratedScheduleStudent.IdStudent;
                    var scheduleLessonsPerStudent = scheduleLessons.Where(x => x.GeneratedScheduleStudent.IdStudent == idStudent);

                    foreach (var scheduleLesson in scheduleLessonsPerStudent)
                    {
                        // disable existing entry
                        //var entryStudent = scheduleLesson.AttendanceEntries.FirstOrDefault();
                        //if (entryStudent != null)
                        //    DisableAttendanceEntry(entryStudent);

                        var entryStudentAny = scheduleLesson.AttendanceEntries.OrderByDescending(x => x.DateUp).ThenByDescending(x => x.DateIn).ToList();
                        foreach (var item in entryStudentAny)
                        {
                            DisableAttendanceEntry(item);
                        }
                        if (!idGeneratedScheduleLesson.Contains(entry.IdGeneratedScheduleLesson))
                        {
                            idGeneratedScheduleLesson.Add(entry.IdGeneratedScheduleLesson);
                            CreateAttendanceEntry(scheduleLesson.Id, entry, null, false);
                        }

                    }
                }
                else if (_mapAttendance.Term == AbsentTerm.Session)
                {
                    var currScheduleLesson = scheduleLessons.Find(x => x.Id == entry.IdGeneratedScheduleLesson);
                    var nextScheduleLesson = default(TrGeneratedScheduleLesson);
                    if (termSessionNext != null)
                        nextScheduleLesson = scheduleLessons.Find(x => x.IdGeneratedScheduleStudent == currScheduleLesson.IdGeneratedScheduleStudent && x.IdSession == termSessionNext.Id);

                    var selectedAttendance = _mapAttendance.Attendances
                        .FirstOrDefault(x => x.IdAttendanceMapAttendance == entry.IdAttendanceMapAttendance);
                    if (selectedAttendance is null)
                        throw new BadRequestException($"Please select proper attendance for student {GetStudentName(currScheduleLesson)}.");
                    var attendanceValidation = termSessionValidation.Attendances.FirstOrDefault(x => x.Id == selectedAttendance.Id);
                    if (attendanceValidation is null)
                        throw new BadRequestException($"You can't fill attendance {selectedAttendance.Description} for student {GetStudentName(currScheduleLesson)}.");

                    var ignoredStudentIds = new List<string>();
                    var sessionCurrScheduleLesson = currScheduleLesson.SessionID;
                    foreach (var scheduleLesson in new[] { currScheduleLesson, nextScheduleLesson })
                    {
                        if (scheduleLesson != null && !ignoredStudentIds.Contains(scheduleLesson.GeneratedScheduleStudent.IdStudent))
                        {
                            var entryStudent = scheduleLesson.AttendanceEntries.OrderByDescending(x => x.DateUp).ThenByDescending(x => x.DateIn).FirstOrDefault();
                            // disable entry
                            if (entryStudent != null)
                            {
                                var continueEntry = true;
                                var existAttendanceMap = _mapAttendance.Attendances.First(x => x.IdAttendanceMapAttendance == entryStudent.IdAttendanceMappingAttendance);
                                var entryAttendanceMap = _mapAttendance.Attendances.First(x => x.IdAttendanceMapAttendance == entry.IdAttendanceMapAttendance);
                                var isNeedValidation = _mapAttendance.TermSession.Validations.FirstOrDefault(x => x.AbsentBy.Code == "ST")?.Attendances.FirstOrDefault(x => x.Id == entryAttendanceMap.Id)?.NeedToValidate;

                                if (entryStudent.Status == AttendanceEntryStatus.Pending && isNeedValidation.HasValue && isNeedValidation.Value)
                                {
                                    ignoredStudentIds.Add(entryStudent.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent);
                                    continueEntry = false;
                                }

                                if (continueEntry)
                                {
                                    var entryStudentAny = scheduleLesson.AttendanceEntries.OrderByDescending(x => x.DateUp).ThenByDescending(x => x.DateIn).ToList();
                                    foreach(var item in entryStudentAny)
                                    {
                                        DisableAttendanceEntry(item);
                                    }
                                    if (!idGeneratedScheduleLesson.Contains(scheduleLesson.Id))
                                    {
                                        idGeneratedScheduleLesson.Add(scheduleLesson.Id);
                                        CreateAttendanceEntry(scheduleLesson.Id, entry, attendanceValidation, scheduleLesson == nextScheduleLesson);
                                    }
                                }

                            }
                            else
                            {
                                if (!idGeneratedScheduleLesson.Contains(scheduleLesson.Id))
                                {
                                    idGeneratedScheduleLesson.Add(scheduleLesson.Id);
                                    CreateAttendanceEntry(scheduleLesson.Id, entry, attendanceValidation, scheduleLesson == nextScheduleLesson);
                                }
                            }
                        }
                    }
                }
            }
            await _dbContext.SaveChangesAsync(CancellationToken);
            try
            {
                #region Calculate Attendance Rate
                string idGrade = "";
                if (body.Id != null)
                {

                    idGrade = await _dbContext.Entity<MsHomeroom>()
                                              .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade)
                                              .Where(x => x.Id == body.Id)
                                              .Select(x => x.IdGrade)
                                              .FirstOrDefaultAsync(CancellationToken);
                }
                else if (body.ClassId != null)
                {
                    idGrade = await (from _gsl in _dbContext.Entity<TrGeneratedScheduleLesson>()
                                     join _l in _dbContext.Entity<MsLesson>() on _gsl.IdLesson equals _l.Id
                                     join _g in _dbContext.Entity<MsGrade>() on _l.IdGrade equals _g.Id
                                     where
                                          _gsl.ClassID == body.ClassId
                                          && _gsl.ScheduleDate.Date == body.Date
                                     select _g.Id
                               ).FirstOrDefaultAsync(CancellationToken);
                }
                var startDateAttendance = await _dbContext.Entity<MsPeriod>()
                    .Where(x => x.IdGrade == idGrade)
                    .Select(x => x.AttendanceStartDate).FirstOrDefaultAsync(CancellationToken);
                var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.Grade)
                                                .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                                                .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                                                .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                                .Where(x => idStudents.Contains(x.GeneratedScheduleStudent.IdStudent))
                                                .Where(x => x.ScheduleDate.Date >= startDateAttendance.Date)
                                                .Where(x => x.ScheduleDate.Date <= _machineDateTime.ServerTime.Date)
                                                .ToListAsync(CancellationToken);
                var mapping = await _dbContext.Entity<MsMappingAttendance>()
                                             .Include(x => x.Level).ThenInclude(x => x.Formulas)
                                             .Where(x => x.IdLevel == idLevel)
                                             .FirstOrDefaultAsync(CancellationToken);
                if (mapping is null)
                    throw new NotFoundException("mapping is not found for this level");
                if (!mapping.Level.Formulas.Any(x => x.IsActive))
                    throw new NotFoundException("formula is not found for this level");

                var studentBelow = schedules.GroupBy(x => new { Student = x.GeneratedScheduleStudent.Student, Homeroom = new { x.IdHomeroom, x.HomeroomName, x.Homeroom.Grade.Code } })
                .Select(x => new StudentBelowVm
                {
                    MinPercentage = mapping.Level.Formulas.First(y => y.IsActive).MinimumPercentageAttendanceRate,
                    IdStudent = x.Key.Student.Id,
                    YearLevel = x.Key.Homeroom.Code,
                    StudentName = $"{x.Key.Student.FirstName} {x.Key.Student.LastName}",
                    AttendanceRate = mapping.Level.Formulas.First(y => y.IsActive).AttendanceRate.Calculate(mapping.AbsentTerms, x),
                    CurrentDate = _machineDateTime.ServerTime
                })
                .Where(x => x.AttendanceRate < mapping.Level.Formulas.First(y => y.IsActive).MinimumPercentageAttendanceRate)
                .ToList();
                IDictionary<string, object> paramTemplateNotificationBelow = new Dictionary<string, object>();
                paramTemplateNotificationBelow.Add("data", JsonConvert.SerializeObject(studentBelow));
                if (KeyValues.TryGetValue("collector", out var collect2) && collect2 is ICollector<string> collector2)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD5")
                    {
                        IdRecipients = new string[] { "A" },
                        KeyValues = paramTemplateNotificationBelow
                    });
                    collector2.Add(message);
                }
                #endregion



                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();
                var idAttendanceEntry = _attendanceEntries.Where(x => x.Status == AttendanceEntryStatus.Submitted).Select(x => x.Id).ToList();
                paramTemplateNotification.Add("data", JsonConvert.SerializeObject(idAttendanceEntry));
                var scenario = _mapAttendance.Term switch
                {
                    AbsentTerm.Day => "ATD1",
                    AbsentTerm.Session => "ATD2",
                    _ => "ATD1"
                };
                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, scenario)
                    {
                        IdRecipients = new string[] { "A" },
                        KeyValues = paramTemplateNotification
                    });
                    collector.Add(message);
                }

                //Send notification scenario ATD10 if term is day
                if (_mapAttendance.Term == AbsentTerm.Day)
                {
                    IDictionary<string, object> paramATD10 = new Dictionary<string, object>();
                    paramATD10.Add("date", body.Date);
                    paramATD10.Add("idHomeroom", body.Id);
                    if (KeyValues.TryGetValue("collector", out var collect10) && collect10 is ICollector<string> collector10)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD10")
                        {
                            IdRecipients = new string[] { "A" },
                            KeyValues = paramATD10
                        });
                        collector10.Add(message);
                    }
                }

                //Send notification scenario ATD16 or ATD17 if checkbox send late email to parent is true
                if (body.SendLateEmailToParent)
                {
                    var lateEntryScheduleLessons = body.Entries.Where(x => _mapAttendance.Attendances.First(y => y.IdAttendanceMapAttendance == x.IdAttendanceMapAttendance).Code == "LT")
                                                               .Select(x => x.IdGeneratedScheduleLesson)
                                                               .ToList();

                    lateEntryScheduleLessons = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                               .Include(x => x.SendEmailAttendanceEntries)
                                                               .Where(x => lateEntryScheduleLessons.Contains(x.Id)
                                                                           && !x.SendEmailAttendanceEntries.Any())
                                                               .Select(x => x.Id)
                                                               .ToListAsync(CancellationToken);
                    if (lateEntryScheduleLessons.Any())
                    {
                        IDictionary<string, object> paramSendLateEmail = new Dictionary<string, object>();
                        paramSendLateEmail.Add("idGeneratedScheduleLessons", JsonConvert.SerializeObject(lateEntryScheduleLessons));

                        if (KeyValues.TryGetValue("collector", out var collect1617) && collect1617 is ICollector<string> collector1617)
                        {
                            var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, _mapAttendance.Term switch
                            {
                                AbsentTerm.Day => "ATD16",
                                AbsentTerm.Session => "ATD17",
                                _ => null
                            })
                            {
                                IdRecipients = new string[] { "A" },
                                KeyValues = paramSendLateEmail
                            });
                            collector1617.Add(message);
                        }
                    }
                }

                //Send notification scenario ATD18 or ATD19 if checkbox send absent email to parent is true
                if (body.SendAbsentEmailToParent)
                {
                    var absentEntryScheduleLessons = body.Entries.Where(x => _mapAttendance.Attendances.First(y => y.IdAttendanceMapAttendance == x.IdAttendanceMapAttendance).AbsenceCategory.HasValue
                                                                             || _mapAttendance.Attendances.First(y => y.IdAttendanceMapAttendance == x.IdAttendanceMapAttendance).AttendanceCategory == AttendanceCategory.Absent)
                                                                 .Select(x => x.IdGeneratedScheduleLesson)
                                                                 .ToList();

                    absentEntryScheduleLessons = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                                 .Include(x => x.SendEmailAttendanceEntries)
                                                                 .Where(x => absentEntryScheduleLessons.Contains(x.Id)
                                                                             && !x.SendEmailAttendanceEntries.Any())
                                                                 .Select(x => x.Id)
                                                                 .ToListAsync(CancellationToken);
                    if (absentEntryScheduleLessons.Any())
                    {
                        IDictionary<string, object> paramSendAbsentEmail = new Dictionary<string, object>();
                        paramSendAbsentEmail.Add("idGeneratedScheduleLessons", JsonConvert.SerializeObject(absentEntryScheduleLessons));

                        if (KeyValues.TryGetValue("collector", out var collect1819) && collect1819 is ICollector<string> collector1819)
                        {
                            var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, _mapAttendance.Term switch
                            {
                                AbsentTerm.Day => "ATD18",
                                AbsentTerm.Session => "ATD19",
                                _ => null
                            })
                            {
                                IdRecipients = new string[] { "A" },
                                KeyValues = paramSendAbsentEmail
                            });
                            collector1819.Add(message);
                        }
                    }
                }

                //Send notification scenario ATD20 or ATD21 if there are update
                var allScheduledLessons = body.Entries.Select(x => x.IdGeneratedScheduleLesson)
                                                      .ToList();

                var hasSentEmailScheduledLessons = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                                   .Include(x => x.SendEmailAttendanceEntries)
                                                                   .Include(x => x.AttendanceEntries)
                                                                   .Where(x => allScheduledLessons.Contains(x.Id)
                                                                               && x.SendEmailAttendanceEntries.Any())
                                                                   .IgnoreQueryFilters()
                                                                   .ToListAsync(CancellationToken);
                var needSendEmailUpdateScheduledLessons = hasSentEmailScheduledLessons.Where(x => x.AttendanceEntries.Count() > 1
                                                                                                 && x.AttendanceEntries.OrderByDescending(y => y.DateIn).First().IdAttendanceMappingAttendance
                                                                                                    != x.AttendanceEntries.OrderByDescending(y => y.DateIn).Skip(1).First().IdAttendanceMappingAttendance)
                                                                                      .Select(x => x.Id)
                                                                                      .ToList();

                if (needSendEmailUpdateScheduledLessons.Any())
                {
                    IDictionary<string, object> paramSendUpdateEmail = new Dictionary<string, object>();
                    paramSendUpdateEmail.Add("idGeneratedScheduleLessons", JsonConvert.SerializeObject(needSendEmailUpdateScheduledLessons));

                    if (KeyValues.TryGetValue("collector", out var collect2021) && collect2021 is ICollector<string> collector2021)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, _mapAttendance.Term switch
                        {
                            AbsentTerm.Day => "ATD20",
                            AbsentTerm.Session => "ATD21",
                            _ => null
                        })
                        {
                            IdRecipients = new string[] { "A" },
                            KeyValues = paramSendUpdateEmail
                        });
                        collector2021.Add(message);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            return base.OnException(ex);
        }

        #region Private Method

        private void CreateAttendanceEntry(string idScheduleLesson, UpdateAttendanceEntryStudent entry, MapAttendanceValidation validation, bool isCopy)
        {
            if (isCopy)
            {
                var attendanceMap = _mapAttendance.Attendances.First(x => x.IdAttendanceMapAttendance == entry.IdAttendanceMapAttendance);
                if (attendanceMap.Code == "LT")
                {
                    entry.IdAttendanceMapAttendance = _mapAttendance.Attendances.First(x => x.Code == "PR").IdAttendanceMapAttendance;
                    entry.LateInMinute = null;
                }
            }
            if (entry.IdAttendanceMapAttendance != null) //new rule for save
            {
                var newEntry = new TrAttendanceEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGeneratedScheduleLesson = idScheduleLesson,
                    IdAttendanceMappingAttendance = entry.IdAttendanceMapAttendance,
                    LateTime = !string.IsNullOrEmpty(entry.LateInMinute) ? TimeSpan.Parse(entry.LateInMinute) : default,
                    FileEvidence = entry.File,
                    Notes = entry.Note,
                    PositionIn = _currentPosition
                };

                if (_mapAttendance.Term == AbsentTerm.Day)
                    newEntry.Status = AttendanceEntryStatus.Submitted;
                else if (_mapAttendance.Term == AbsentTerm.Session)
                    newEntry.Status = !validation.NeedToValidate // attendance with no validation is always Submitted
                        ? AttendanceEntryStatus.Submitted
                        : _currentPosition == PositionConstant.ClassAdvisor // position CA can make Pending attendance to Submitted
                            ? AttendanceEntryStatus.Submitted
                            : AttendanceEntryStatus.Pending;
                _dbContext.Entity<TrAttendanceEntry>().Add(newEntry);
                _attendanceEntries.Add(newEntry);
                foreach (var studentWorkhabit in entry.IdWorkhabits)
                {
                    var newStudentWorkhabit = new TrAttendanceEntryWorkhabit
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAttendanceEntry = newEntry.Id,
                        IdMappingAttendanceWorkhabit = studentWorkhabit,
                    };
                    _dbContext.Entity<TrAttendanceEntryWorkhabit>().Add(newStudentWorkhabit);
                }
            }

        }

        private void DisableAttendanceEntry(TrAttendanceEntry entry)
        {
            if (entry.IsActive)
            {
                entry.IsActive = false;
                _dbContext.Entity<TrAttendanceEntry>().Update(entry);

                foreach (var studentWorkhabit in entry.AttendanceEntryWorkhabits)
                {
                    studentWorkhabit.IsActive = false;
                    _dbContext.Entity<TrAttendanceEntryWorkhabit>().Update(studentWorkhabit);
                }
            }
        }

        private Expression<Func<TrGeneratedScheduleLesson, bool>> CreateSessionPredicate(string classId, string idSession)
        {
            return PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.ClassID == classId && x.IdSession == idSession);
        }

        private string GetStudentName(TrGeneratedScheduleLesson scheduleLesson)
        {
            return NameUtil.GenerateFullName(
                scheduleLesson.GeneratedScheduleStudent.Student.FirstName,
                scheduleLesson.GeneratedScheduleStudent.Student.MiddleName,
                scheduleLesson.GeneratedScheduleStudent.Student.LastName);
        }

        #endregion

        #region StudentBelowModel
        public class StudentBelowVm
        {
            public string IdStudent { get; set; }
            public DateTime CurrentDate { get; set; }
            public string StudentName { get; set; }
            public string YearLevel { get; set; }
            public double MinPercentage { get; set; }
            public double AttendanceRate { get; set; }
            public string IdParent { get; set; }
            public string ParentName { get; set; }
            public string ParentEmail { get; set; }
        }
        #endregion
    }
}
