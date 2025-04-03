using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.EventAttendanceEntry.Validator;
using BinusSchool.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateEventAttendanceEntryV2Handler : FunctionsHttpSingleHandler
    {
        private GetMapAttendanceDetailResult _mapAttendance;

        private readonly IAttendanceDbContext _dbContext;
        private readonly GetMapAttendanceDetailHandler _mapAttendanceHandler;
        private readonly IFeatureManagerSnapshot _featureManager;
        private readonly IMachineDateTime _datetimeNow;
        public UpdateEventAttendanceEntryV2Handler(
            IAttendanceDbContext dbContext,
            GetMapAttendanceDetailHandler mapAttendanceHandler,
            IFeatureManagerSnapshot featureManager
            , IMachineDateTime datetimeNow)
        {
            _dbContext = dbContext;
            _mapAttendanceHandler = mapAttendanceHandler;
            _featureManager = featureManager;
            _datetimeNow = datetimeNow;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateEventAttendanceEntryV2Request>();

            _mapAttendance = await _mapAttendanceHandler.GetMapAttendanceDetail(body.IdLevel, CancellationToken);
            if (_mapAttendance is null)
            {
                var level = await _dbContext.Entity<MsLevel>().Where(x => x.Id == body.IdLevel).Select(x => x.Description).SingleOrDefaultAsync(CancellationToken);
                throw new BadRequestException($"Mapping attendance for level {level ?? body.IdLevel} is not available.");
            }

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var checkStudent = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                    .Include(x => x.EventIntendedForAttendanceStudent).ThenInclude(x => x.EventIntendedForAtdPICStudents)
                    .Include(x => x.EventIntendedForAttendanceStudent).ThenInclude(x => x.EventIntendedFor)
                        .ThenInclude(x => x.Event).ThenInclude(x => x.EventDetails).ThenInclude(x => x.UserEvents).ThenInclude(x => x.UserEventAttendance2s)
                    .Where(x => x.Id == body.IdEventCheck)
                    .FirstOrDefaultAsync(CancellationToken);

                var eventType = checkStudent.EventIntendedForAttendanceStudent.Type;
                var idAcademicYear = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.IdAcademicYear;

                if (checkStudent is null)
                    throw new BadRequestException("Event check is not found");
                if (!checkStudent.EventIntendedForAttendanceStudent.EventIntendedForAtdPICStudents.Any(x => x.IdUser == body.IdUser))
                    throw new BadRequestException("You're not PIC of this event");

                List<string> IdEventIntendedForAtdCheckStudent = new List<string>();
                var IsRepeat = checkStudent.EventIntendedForAttendanceStudent.IsRepeat;
                if (!IsRepeat)
                {
                    IdEventIntendedForAtdCheckStudent = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                    .Where(x => x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.Id == checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.Id && x.CheckName == checkStudent.CheckName)
                    .Select(e => e.Id)
                    .ToListAsync(CancellationToken);
                }
                else
                {
                    IdEventIntendedForAtdCheckStudent.Add(checkStudent.Id);
                }

                var StartDate = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Select(e => e.StartDate).FirstOrDefault();
                var EndDate = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Select(e => e.EndDate).FirstOrDefault();

                var GetUserEventAttendance2 = await _dbContext.Entity<TrUserEventAttendance2>()
                                                .Where(x => IdEventIntendedForAtdCheckStudent.Contains(x.IdEventIntendedForAtdCheckStudent))
                                                .ToListAsync(CancellationToken);

                var idUserEvents2 = body.Entries.Select(x => x.IdUserEvent);
                var userEvents2 = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails
                    .SelectMany(x => x.UserEvents)
                    .Where(x => idUserEvents2.Contains(x.Id));

                // throw when any user event that not found
                var notFoundUserEvents2 = idUserEvents2.Except(userEvents2.Select(x => x.Id)).ToArray();
                if (notFoundUserEvents2.Length != 0)
                    throw new BadRequestException($"Some user event is not found: {string.Join(", ", notFoundUserEvents2)}");

                // make sure no multiple student from requested entries
                var duplicateEntries2 = userEvents2.GroupBy(x => x.IdUser).Where(x => x.Count() > 1)
                    .Select(x => x.First().User.DisplayName)
                    .ToArray();
                if (duplicateEntries2.Length != 0)
                    throw new BadRequestException($"You entered multiple entry for student(s) {string.Join(", ", duplicateEntries2)}.");

                foreach (var item in userEvents2)
                {
                    var hasConflict = checkStudent.IsPrimary && await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                        .Where(x
                            => x.StartDate.Date == checkStudent.StartDate.Date
                            && x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Any(y => y.UserEvents.Any(z => z.IdUser == item.Id)))
                        .AnyAsync(CancellationToken);

                    var entry = body.Entries.First(x => x.IdUserEvent == item.Id);

                    var attendance = item.UserEventAttendance2s?.FirstOrDefault(x => x.IdEventIntendedForAtdCheckStudent == checkStudent.Id);

                    if (GetUserEventAttendance2.Any())
                    {
                        GetUserEventAttendance2.ForEach(x => x.IsActive = false);
                        _dbContext.Entity<TrUserEventAttendance2>().UpdateRange(GetUserEventAttendance2);
                    }

                    foreach (var ItemEventIntendedForAtdCheckStudent in IdEventIntendedForAtdCheckStudent)
                    {
                        var userEventAttendance = new TrUserEventAttendance2
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUserEvent = entry.IdUserEvent,
                            IdEventIntendedForAtdCheckStudent = ItemEventIntendedForAtdCheckStudent,
                            IdAttendanceMappingAttendance = entry.IdAttendanceMapAttendance,
                            LateTime = !string.IsNullOrEmpty(entry.LateInMinute) ? TimeSpan.Parse(entry.LateInMinute) : default,
                            FileEvidence = entry.File,
                            Notes = entry.Note,
                            DateEvent = checkStudent.StartDate,
                            HasBeenResolved = !hasConflict
                        };
                        _dbContext.Entity<TrUserEventAttendance2>().Add(userEventAttendance);

                        foreach (var studentWorkhabit in entry.IdWorkhabits)
                        {
                            var workhabit = new TrUserEventAttendanceWorkhabit2
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdUserEventAttendance = userEventAttendance.Id,
                                IdMappingAttendanceWorkhabit = studentWorkhabit,
                            };
                            _dbContext.Entity<TrUserEventAttendanceWorkhabit2>().Add(workhabit);
                        }
                    }

                    var IdAcademicYear = await _dbContext.Entity<MsLevel>()
                                    .Include(e => e.AcademicYear)
                                    .Where(e => e.Id == body.IdLevel)
                                    .Select(e => e.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);

                    var listPeriod = await _dbContext.Entity<MsPeriod>()
                     .Where(e => e.Grade.Level.IdAcademicYear == IdAcademicYear)
                     .ToListAsync(CancellationToken);

                    var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                              .Include(e => e.Student)
                              .Where(e => e.IdAcademicYear == IdAcademicYear && e.ActiveStatus)
                              .Select(e => new
                              {
                                  e.IdStudent,
                                  e.StartDate,
                                  EndDate = e.EndDate == null
                                             ? listPeriod.Select(e=>e.AttendanceEndDate).Max()
                                             : Convert.ToDateTime(e.EndDate),
                                  e.Student.IdBinusian
                              })
                              .ToListAsync(CancellationToken);

                    var listMappingAttendanceAbsent = await _dbContext.Entity<MsListMappingAttendanceAbsent>()
                           .Include(e => e.MsAttendance)
                          .Where(e => e.MsAttendance.IdAcademicYear == IdAcademicYear)
                          .ToListAsync(CancellationToken);

                    var listAttendanceMappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                        .Include(e => e.Attendance)
                                       .Where(e => e.Attendance.IdAcademicYear == IdAcademicYear)
                                       .ToListAsync(CancellationToken);

                    if (eventType == EventIntendedForAttendanceStudent.Mandatory)
                    {
                        // action for replace attendance entry
                        if (!hasConflict && checkStudent.IsPrimary)
                        {

                            var listEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                .Where(e => (e.HomeroomStudent.IdStudent == item.IdUser)
                                               && e.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear)
                               .GroupBy(e => new
                               {
                                   e.IdLesson,
                                   e.IdHomeroomStudent,
                                   e.HomeroomStudent.Homeroom.Semester,
                                   IdHomeroomStudentEnrollment=e.Id,
                                   e.HomeroomStudent.Homeroom.IdGrade,
                                   e.Lesson.ClassIdGenerated,
                                   e.HomeroomStudent.IdStudent
                               })
                               .Select(e => new GetHomeroom
                               {
                                   IdLesson = e.Key.IdLesson,
                                   IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                   Semester = e.Key.Semester,
                                   IsFromMaster = true,
                                   IsDelete = false,
                                   IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                   Grade = new CodeWithIdVm
                                   {
                                       Id = e.Key.IdGrade
                                   },
                                   ClassId = e.Key.ClassIdGenerated,
                                   IdStudent = e.Key.IdStudent,
                                   IsShowHistory = false
                               })
                               .ToListAsync(CancellationToken);

                            listEnrollment.ForEach(e =>
                            {
                                e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                                e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                            });

                            var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                         .Include(e => e.SubjectNew)
                                         .Include(e => e.LessonNew)
                                         .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom)
                                         .Where(x => x.StartDate.Date <= checkStudent.StartDate.Date
                                                   && x.LessonNew.IdAcademicYear == idAcademicYear
                                                   && x.HomeroomStudent.IdStudent == item.IdUser)
                                         .Select(e => new GetHomeroom
                                         {
                                             IdLesson = e.IdLessonNew,
                                             IdHomeroomStudent = e.IdHomeroomStudent,
                                             Semester = e.HomeroomStudent.Homeroom.Semester,
                                             IsFromMaster = true,
                                             IsDelete = false,
                                             IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                                             EffectiveDate = e.StartDate,
                                             Grade = new CodeWithIdVm
                                             {
                                                 Id = e.HomeroomStudent.Homeroom.IdGrade
                                             },
                                             ClassId = e.LessonNew.ClassIdGenerated,
                                             IdStudent = e.HomeroomStudent.IdStudent,
                                             IsShowHistory = e.IsSendEmail,
                                             Datein = e.DateIn.Value
                                         })
                                         .ToListAsync(CancellationToken);

                            var listStudentEnrollmentUnion = listEnrollment.Union(getTrHomeroomStudentEnrollment)
                                               .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                               .ToList();

                            var listIdLesson = listStudentEnrollmentUnion.Select(e => e.IdLesson).Distinct().ToList();
                            List<MsScheduleLesson> listScheduleLesson = new List<MsScheduleLesson>();
                            if (!IsRepeat)
                            {
                                listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                    .Include(e=>e.Lesson)
                                    .Where(x=> listIdLesson.Contains(x.IdLesson)
                                        && (x.ScheduleDate.Date >= StartDate.Date && x.ScheduleDate.Date <= EndDate.Date)
                                        && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                    .ToListAsync(CancellationToken);
                            }
                            else
                            {
                                listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                    .Include(e => e.Lesson)
                                    .Where(x=> listIdLesson.Contains(x.IdLesson)
                                        && x.ScheduleDate.Date == checkStudent.StartDate.Date
                                        && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                    .ToListAsync(CancellationToken);

                            }

                            listIdLesson = listScheduleLesson.Select(e => e.IdLesson).ToList();
                            var listIdSession = listScheduleLesson.Select(e => e.IdSession).ToList();
                            var listIdDay = listScheduleLesson.Select(e => e.IdDay).ToList();
                            var listIdScheduleLesson = listScheduleLesson.Select(e => e.Id).ToList();

                            var schedule = await _dbContext.Entity<MsSchedule>()
                            .Where(e => listIdLesson.Contains(e.IdLesson)
                                        && listIdSession.Contains(e.IdSession)
                                        && listIdDay.Contains(e.IdDay))
                            .Select(e => new 
                            {
                                IdUser = e.IdUser,
                                IdLesson = e.IdLesson,
                                IdSession = e.IdSession,
                                IdDay = e.IdDay,
                                IdWeek = e.IdWeek
                            })
                            .ToListAsync(CancellationToken);

                            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                                        .Include(e=>e.HomeroomStudent)
                                        .Where(x => listIdScheduleLesson.Contains(x.IdScheduleLesson))
                                        .ToListAsync(CancellationToken);

                            var idLevel = listScheduleLesson.Select(e => e.IdLevel).FirstOrDefault();

                            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                .Where(e => e.IdLevel == idLevel)
                                .Select(e => new
                                {
                                    e.AbsentTerms,
                                    e.IsNeedValidation,
                                    e.IsUseWorkhabit
                                })
                                .FirstOrDefaultAsync(CancellationToken);

                            foreach (var scheduleLesson in listScheduleLesson)
                            {
                                var lessonExistInEntry = listAttendanceEntry.Where(e => e.IdScheduleLesson == scheduleLesson.Id && e.HomeroomStudent.IdStudent==item.IdUser).ToList();

                                var IdTecher = schedule
                                                .Where(e => e.IdLesson == scheduleLesson.IdLesson
                                                   && e.IdDay == scheduleLesson.IdDay
                                                   && e.IdSession == scheduleLesson.IdSession
                                                   && e.IdWeek == scheduleLesson.IdWeek
                                                    )
                                            .Select(e => e.IdUser)
                                            .FirstOrDefault();

                                foreach (var lesson in lessonExistInEntry)
                                {
                                    lesson.IsActive = false;
                                }
                                _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(lessonExistInEntry);

                                //moving
                                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, scheduleLesson.ScheduleDate, scheduleLesson.Lesson.Semester.ToString(), scheduleLesson.IdLesson);

                                var listIdStudent = listStudentStatus
                                        .Where(e => e.StartDate.Date <= scheduleLesson.ScheduleDate.Date
                                                    && e.EndDate.Date >= scheduleLesson.ScheduleDate.Date 
                                                    && e.IdStudent==item.IdUser)
                                        .Select(e => e.IdStudent).ToList();

                                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                                                .Where(e => listIdStudent.Contains(e.IdStudent))
                                                                .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                                                .ToList();
                                    
                                var listIdHomeroomStudent = studentEnrollmentMoving
                                                                .Where(e => e.IdLesson == scheduleLesson.IdLesson)
                                                                .Select(e => e.IdHomeroomStudent)
                                                                .Distinct().ToList();

                                foreach (var idHomeroomStudent in listIdHomeroomStudent)
                                {
                                    var IsNeedValidation = UpdateAttendanceEntryV2Handler.GetNeedValidation(listAttendanceMappingAttendance, listMappingAttendanceAbsent, entry.IdAttendanceMapAttendance);

                                    var attendanceEntry = new TrAttendanceEntryV2
                                    {
                                        IdAttendanceEntry = Guid.NewGuid().ToString(),
                                        IdScheduleLesson = scheduleLesson.Id,
                                        IdAttendanceMappingAttendance = entry.IdAttendanceMapAttendance,
                                        IdHomeroomStudent = idHomeroomStudent,
                                        IdBinusian = IdTecher,
                                        Status = IsNeedValidation
                                                ? AttendanceEntryStatus.Pending
                                                : AttendanceEntryStatus.Submitted,
                                        IsFromAttendanceAdministration = false,
                                        LateTime = !string.IsNullOrEmpty(entry.LateInMinute) ? TimeSpan.Parse(entry.LateInMinute) : default,
                                        FileEvidence = entry.File,
                                        Notes = entry.Note,
                                        PositionIn = "PIC"
                                    };
                                    _dbContext.Entity<TrAttendanceEntryV2>().Add(attendanceEntry);

                                    foreach (var studentWorkhabit in entry.IdWorkhabits)
                                    {
                                        var attendanceEntryWorkhabit = new TrAttendanceEntryWorkhabitV2
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdAttendanceEntry = attendance.Id,
                                            IdMappingAttendanceWorkhabit = studentWorkhabit,
                                        };
                                        _dbContext.Entity<TrAttendanceEntryWorkhabitV2>().Add(attendanceEntryWorkhabit);
                                    }
                                }
                            }
                        }
                    }

                    if (eventType == EventIntendedForAttendanceStudent.Excuse)
                    {
                        var IdAttendanceMappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                    .Include(e => e.Attendance)
                                    .Include(e => e.MappingAttendance)
                                    .Where(e => e.Attendance.IdAcademicYear == IdAcademicYear && e.Attendance.Code == "EA" && e.MappingAttendance.IdLevel == body.IdLevel)
                                    .Select(e => e.Id)
                                    .FirstOrDefaultAsync(CancellationToken);

                        // action for replace attendance entry
                        if (!hasConflict && checkStudent.IsPrimary)
                        {
                            var listEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                .Where(e => (e.HomeroomStudent.IdStudent == item.IdUser)
                                               && e.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear)
                               .GroupBy(e => new
                               {
                                   e.IdLesson,
                                   e.IdHomeroomStudent,
                                   e.HomeroomStudent.Homeroom.Semester,
                                   IdHomeroomStudentEnrollment = e.Id,
                                   e.HomeroomStudent.Homeroom.IdGrade,
                                   e.HomeroomStudent.IdStudent
                               })
                               .Select(e => new GetHomeroom
                               {
                                   IdLesson = e.Key.IdLesson,
                                   IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                   Semester = e.Key.Semester,
                                   IsFromMaster = true,
                                   IsDelete = false,
                                   IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                   Grade = new CodeWithIdVm
                                   {
                                       Id = e.Key.IdGrade
                                   },
                                   IdStudent = e.Key.IdStudent
                               })
                               .ToListAsync(CancellationToken);

                            listEnrollment.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min());

                            var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                         .Include(e => e.SubjectNew)
                                         .Include(e => e.LessonNew)
                                         .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                                         .Where(x => x.StartDate.Date <= checkStudent.StartDate.Date
                                                   && x.LessonNew.IdAcademicYear == idAcademicYear
                                                   && x.HomeroomStudent.IdStudent == item.IdUser)
                                         .Select(e => new GetHomeroom
                                         {
                                             IdLesson = e.IdLessonNew,
                                             IdHomeroomStudent = e.IdHomeroomStudent,
                                             Semester = e.HomeroomStudent.Homeroom.Semester,
                                             IsFromMaster = true,
                                             IsDelete = false,
                                             IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                                             EffectiveDate = e.StartDate,
                                             Grade = new CodeWithIdVm
                                             {
                                                 Id = e.HomeroomStudent.Homeroom.IdGrade
                                             },
                                             IdStudent = e.HomeroomStudent.IdStudent
                                         })
                                         .ToListAsync(CancellationToken);

                            var listStudentEnrollmentUnion = listEnrollment.Union(getTrHomeroomStudentEnrollment)
                                               .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                               .ToList();

                            var listIdLesson = listStudentEnrollmentUnion.Select(e => e.IdLesson).Distinct().ToList();

                            List<MsScheduleLesson> listScheduleLesson = new List<MsScheduleLesson>();
                            if (!IsRepeat)
                            {
                                listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                    .Include(e => e.Lesson)
                                    .Where(x => listIdLesson.Contains(x.IdLesson)
                                        && (x.ScheduleDate.Date >= StartDate.Date && x.ScheduleDate.Date <= EndDate.Date)
                                        && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                    .ToListAsync(CancellationToken);
                            }
                            else
                            {
                                listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                    .Include(e => e.Lesson)
                                    .Where(x => listIdLesson.Contains(x.IdLesson)
                                        && x.ScheduleDate.Date == checkStudent.StartDate.Date
                                        && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                    .ToListAsync(CancellationToken);

                            }

                            listIdLesson = listScheduleLesson.Select(e => e.IdLesson).ToList();
                            var listIdSession = listScheduleLesson.Select(e => e.IdSession).ToList();
                            var listIdDay = listScheduleLesson.Select(e => e.IdDay).ToList();
                            var listIdScheduleLesson = listScheduleLesson.Select(e => e.Id).ToList();

                            var schedule = await _dbContext.Entity<MsSchedule>()
                            .Where(e => listIdLesson.Contains(e.IdLesson)
                                        && listIdSession.Contains(e.IdSession)
                                        && listIdDay.Contains(e.IdDay))
                            .Select(e => new
                            {
                                IdUser = e.IdUser,
                                IdLesson = e.IdLesson,
                                IdSession = e.IdSession,
                                IdDay = e.IdDay,
                                IdWeek = e.IdWeek
                            })
                            .ToListAsync(CancellationToken);

                            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                                        .Include(e=>e.HomeroomStudent)
                                        .Where(x => listIdScheduleLesson.Contains(x.IdScheduleLesson))
                                        .ToListAsync(CancellationToken);


                            var idLevel = listScheduleLesson.Select(e => e.IdLevel).FirstOrDefault();

                            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                .Where(e => e.IdLevel == idLevel)
                                .Select(e => new
                                {
                                    e.AbsentTerms,
                                    e.IsNeedValidation,
                                    e.IsUseWorkhabit
                                })
                                .FirstOrDefaultAsync(CancellationToken);

                            foreach (var scheduleLesson in listScheduleLesson)
                            {
                                //overwrite data in attendance entry if exist
                                var lessonExistInEntry = listAttendanceEntry.Where(e => e.IdScheduleLesson == scheduleLesson.Id && e.HomeroomStudent.IdStudent == item.IdUser).ToList();

                                var IdTecher = schedule
                                                .Where(e => e.IdLesson == scheduleLesson.IdLesson
                                                   && e.IdDay == scheduleLesson.IdDay
                                                   && e.IdSession == scheduleLesson.IdSession
                                                   && e.IdWeek == scheduleLesson.IdWeek
                                                    )
                                            .Select(e => e.IdUser)
                                            .FirstOrDefault();

                                foreach (var lesson in lessonExistInEntry)
                                {
                                    lesson.IsActive = false;
                                }
                                _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(lessonExistInEntry);

                                //moving
                                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, scheduleLesson.ScheduleDate, scheduleLesson.Lesson.Semester.ToString(), scheduleLesson.IdLesson);

                                var listIdStudent = listStudentStatus
                                        .Where(e => e.StartDate.Date <= scheduleLesson.ScheduleDate.Date
                                                    && e.EndDate.Date >= scheduleLesson.ScheduleDate.Date)
                                        .Select(e => e.IdStudent).ToList();

                                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                                                .Where(e => listIdStudent.Contains(e.IdStudent))
                                                                .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                                                .ToList();

                                var listIdHomeroomStudent = studentEnrollmentMoving
                                                                .Where(e => e.IdLesson == scheduleLesson.IdLesson)
                                                                .Select(e => e.IdHomeroomStudent)
                                                                .Distinct().ToList();

                                foreach (var idHomeroomStudent in listIdHomeroomStudent)
                                {
                                    var IsNeedValidation = UpdateAttendanceEntryV2Handler.GetNeedValidation(listAttendanceMappingAttendance, listMappingAttendanceAbsent, entry.IdAttendanceMapAttendance);

                                    var attendanceEntry = new TrAttendanceEntryV2
                                    {
                                        IdAttendanceEntry = Guid.NewGuid().ToString(),
                                        IdScheduleLesson = scheduleLesson.Id,
                                        IdAttendanceMappingAttendance = entry.IdAttendanceMapAttendance,
                                        IdHomeroomStudent = idHomeroomStudent,
                                        IdBinusian = IdTecher,
                                        Status = IsNeedValidation
                                                ? AttendanceEntryStatus.Pending
                                                : AttendanceEntryStatus.Submitted,
                                        IsFromAttendanceAdministration = false,
                                        LateTime = !string.IsNullOrEmpty(entry.LateInMinute) ? TimeSpan.Parse(entry.LateInMinute) : default,
                                        FileEvidence = entry.File,
                                        Notes = entry.Note,
                                        PositionIn = "PIC"
                                    };
                                    _dbContext.Entity<TrAttendanceEntryV2>().Add(attendanceEntry);

                                    foreach (var studentWorkhabit in entry.IdWorkhabits)
                                    {
                                        var attendanceEntryWorkhabit = new TrAttendanceEntryWorkhabitV2
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdAttendanceEntry = attendance.Id,
                                            IdMappingAttendanceWorkhabit = studentWorkhabit,
                                        };
                                        _dbContext.Entity<TrAttendanceEntryWorkhabitV2>().Add(attendanceEntryWorkhabit);
                                    }
                                }
                            }
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                return Request.CreateApiResult2();
            }

            return Request.CreateApiResult2();
        }


    }

    public class ScheduleLesson
    {
        public string Id { get; set; }
        public string IdLesson { get; set; }
        public string IdDay { get; set; }
        public string IdSession { get; set; }
        public string IdWeek { get; set; }
        public string IdLevel { get; set; }
        public int Semester { get; set; }
    }

    public class Schedule
    {
        public string IdUser { get; set; }
        public string IdLesson { get; set; }
        public string IdDay { get; set; }
        public string IdSession { get; set; }
        public string IdWeek { get; set; }
    } 
    
}
