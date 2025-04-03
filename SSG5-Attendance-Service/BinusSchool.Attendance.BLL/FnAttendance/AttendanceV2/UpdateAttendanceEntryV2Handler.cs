using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateAttendanceEntryV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public UpdateAttendanceEntryV2Handler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateAttendanceEntryV2Request>();

            var idScheduleLesson = body.Entries.Select(e => e.IdScheduleLesson).FirstOrDefault();

            var scheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                              .Where(e => e.Id == idScheduleLesson)
                              .Select(e => new
                              {
                                  e.ScheduleDate,
                                  e.ClassID,
                                  e.StartTime,
                                  e.IdLesson,
                                  e.IdDay,
                                  e.IdWeek,
                                  e.IdSession,
                                  e.Id,
                                  e.IdSubject,
                                  level = e.Level.Code,
                                  e.IdLevel,
                                  e.IdAcademicYear
                              })
                              .FirstOrDefaultAsync(CancellationToken);

            if (scheduleLesson == null)
                throw new BadRequestException("Schedule lesson is not found");

            var idTeacher = await _dbContext.Entity<MsSchedule>()
                              .Where(e => e.IdLesson == scheduleLesson.IdLesson
                                      && e.IdDay == scheduleLesson.IdDay
                                      && e.IdSession == scheduleLesson.IdSession
                                      && e.IdWeek == scheduleLesson.IdWeek
                                      )
                              .Select(e => e.IdUser)
                              .FirstOrDefaultAsync(CancellationToken);

            if (idTeacher == null)
            {
                var lessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                  .Where(e => e.IdLesson == scheduleLesson.IdLesson)
                  .Select(e => e.IdUser)
                  .FirstOrDefaultAsync(CancellationToken) ?? throw new BadRequestException("id teacher is not found");

                idTeacher = lessonTeacher;
            }


            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                               .Where(e => e.IdLevel == scheduleLesson.IdLevel)
                               .Select(e => new
                               {
                                   e.AbsentTerms,
                                   e.IsUseWorkhabit
                               })
                               .FirstOrDefaultAsync(CancellationToken);

            if (mappingAttendance == null)
                throw new BadRequestException("Mapping attendance is not found");

            var listMappingAttendanceAbsent = await _dbContext.Entity<MsListMappingAttendanceAbsent>()
                                .Include(e => e.MsAttendance)
                                .Include(e => e.MsAbsentMappingAttendance).ThenInclude(e=>e.TeacherPosition).ThenInclude(e=>e.LtPosition)
                               .Where(e => e.MsAttendance.IdAcademicYear == scheduleLesson.IdAcademicYear)
                               .ToListAsync(CancellationToken);

            var listAttendanceMappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                .Include(e => e.Attendance)
                               .Where(e => e.Attendance.IdAcademicYear == scheduleLesson.IdAcademicYear && e.MappingAttendance.IdLevel== scheduleLesson.IdLevel)
                               .ToListAsync(CancellationToken);

            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                               .Include(x=> x.HomeroomStudent)
                               .Where(e => e.IdScheduleLesson == scheduleLesson.Id)
                               .ToListAsync(CancellationToken);

            var listHomeroom = new List<string>();
            if (PositionConstant.ClassAdvisor == body.CurrentPosition)
            {
                if (mappingAttendance.AbsentTerms == AbsentTerm.Session)
                {
                    listHomeroom = await _dbContext.Entity<MsHomeroomTeacher>()
                                    .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                    .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                    .Where(e => e.IdBinusian == body.IdUser
                                            && e.Homeroom.IdAcademicYear == scheduleLesson.IdAcademicYear
                                            )
                                    .Select(e => e.IdHomeroom)
                                    .ToListAsync(CancellationToken);
                    if (listHomeroom.Any())
                        listAttendanceEntry = listAttendanceEntry.Where(x=> listHomeroom.Contains(x.HomeroomStudent.IdHomeroom)).ToList();
                }
            }
            var listAttendanceEntryByPosition = listAttendanceEntry
                                                .Where(e => e.PositionIn == body.CurrentPosition 
                                                && e.Status != AttendanceEntryStatus.Pending
                                                && !e.IsFromAttendanceAdministration)
                                                .ToList();
            listAttendanceEntryByPosition.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(listAttendanceEntryByPosition);

            var listAttendanceWorkhabit = await _dbContext.Entity<TrAttendanceEntryWorkhabitV2>()
                                .Include(e => e.AttendanceEntry)
                               .Where(e => e.AttendanceEntry.IdScheduleLesson == scheduleLesson.Id)
                               .ToListAsync(CancellationToken);

            listAttendanceWorkhabit.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrAttendanceEntryWorkhabitV2>().UpdateRange(listAttendanceWorkhabit);

            List<TrAttendanceEntryV2> newAttendanceEntry = new List<TrAttendanceEntryV2>();
            List<TrAttendanceEntryWorkhabitV2> newAttendanceEntryWorkhabit = new List<TrAttendanceEntryWorkhabitV2>();
            if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
            {
                var listIdHomeroomStudent = body.Entries.Select(e => e.IdHomeroomStudent).ToList();

                var listIdHomeroom = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                              .Where(e => listIdHomeroomStudent.Contains(e.IdHomeroomStudent))
                              .Select(e => e.HomeroomStudent.IdHomeroom)
                              .Distinct()
                              .ToListAsync(CancellationToken);

                var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(x => x.Homeroom).ThenInclude(x => x.Grade)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(x => x.Homeroom)
                                                        .ThenInclude(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                                                    .Where(e => listIdHomeroom.Contains(e.HomeroomStudent.IdHomeroom))
                                                     .GroupBy(e => new
                                                     {
                                                         e.IdLesson,
                                                         e.IdHomeroomStudent,
                                                         e.HomeroomStudent.IdHomeroom,
                                                         e.HomeroomStudent.IdStudent,
                                                         idGrade = e.HomeroomStudent.Homeroom.Grade.Id,
                                                         gradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                                                         classroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                                         e.Id,
                                                         classId = e.Lesson.ClassIdGenerated,
                                                         e.HomeroomStudent.Homeroom.Semester,
                                                         e.Lesson.IdSubject,
                                                         subjectName = e.Lesson.Subject.Description
                                                     })
                                                      .Select(e => new GetHomeroom
                                                      {
                                                          IdLesson = e.Key.IdLesson,
                                                          Homeroom = new ItemValueVm
                                                          {
                                                              Id = e.Key.IdHomeroom,
                                                          },
                                                          Grade = new CodeWithIdVm
                                                          {
                                                              Id = e.Key.idGrade,
                                                              Code = e.Key.gradeCode,
                                                          },
                                                          ClassroomCode = e.Key.classroomCode,
                                                          ClassId = e.Key.classId,
                                                          IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                                          IdStudent = e.Key.IdStudent,
                                                          Semester = e.Key.Semester,
                                                          IdHomeroomStudentEnrollment = e.Key.Id,
                                                          IsFromMaster = true,
                                                          IsDelete = false,
                                                          IdSubject = e.Key.IdSubject,
                                                          SubjectName = e.Key.subjectName,
                                                      })
                                                      .ToListAsync(CancellationToken);

                var listIdLessonHomeroom = await _dbContext.Entity<MsLessonPathway>()
                              .Where(e => listIdHomeroom.Contains(e.HomeroomPathway.IdHomeroom))
                              .Select(e => e.IdLesson)
                              .Distinct()
                              .ToListAsync(CancellationToken);

                var scheduleLessonAllDay = await _dbContext.Entity<MsScheduleLesson>()
                              .Include(e => e.Lesson)
                              .Where(e => e.ScheduleDate.Date == scheduleLesson.ScheduleDate.Date &&
                                            listIdLessonHomeroom.Contains(e.IdLesson))
                              .Select(e => new
                              {
                                  e.Id,
                                  e.IdLesson,
                                  e.IdDay,
                                  e.IdSession,
                                  e.IdWeek,
                                  e.Lesson.Semester,
                                  e.ScheduleDate
                              })
                              .ToListAsync(CancellationToken);

                var lessonTeacherAll = await _dbContext.Entity<MsLessonTeacher>()
                  .Where(e => scheduleLessonAllDay.Select(x => x.IdLesson).ToList().Contains(e.IdLesson))
                  .ToListAsync(CancellationToken);

                var listIdLesson = scheduleLessonAllDay.Select(e => e.IdLesson).ToList();
                var listIdDay = scheduleLessonAllDay.Select(e => e.IdDay).ToList();

                var schedule = await _dbContext.Entity<MsSchedule>()
                                .Where(e => listIdLesson.Contains(e.IdLesson)
                                            && listIdDay.Contains(e.IdDay))
                                .Select(e => new
                                {
                                    e.IdUser,
                                    e.IdLesson,
                                    e.IdSession,
                                    e.IdDay,
                                    e.IdWeek
                                })
                                .ToListAsync(CancellationToken);

                #region delete attendance
                var lisIdSchdeule = scheduleLessonAllDay.Select(e => e.Id).ToList();

                var listAttendanceEntryAllDays = await _dbContext.Entity<TrAttendanceEntryV2>()
                            .Include(e => e.AttendanceEntryWorkhabitV2s)
                           .Where(e => lisIdSchdeule.Contains(e.IdScheduleLesson) && e.IsFromAttendanceAdministration == false)
                           .ToListAsync(CancellationToken);

                listAttendanceEntryAllDays.ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(listAttendanceEntryAllDays);

                var listAttendanceWorkhabitAlldays = listAttendanceEntryAllDays.SelectMany(e => e.AttendanceEntryWorkhabitV2s).ToList();
                listAttendanceWorkhabitAlldays.ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrAttendanceEntryWorkhabitV2>().UpdateRange(listAttendanceWorkhabitAlldays);
                #endregion

                foreach (var itemScheduleLesson in scheduleLessonAllDay)
                {
                    foreach (var itemAttendaceEntry in body.Entries)
                    {
                        var idAttendance = listAttendanceMappingAttendance
                                            .Where(e => e.Id == itemAttendaceEntry.IdAttendanceMapAttendance)
                                            .Select(e => e.IdAttendance)
                                            .FirstOrDefault();

                        var IsNeedValidation = GetNeedValidationAtdEntryOnly(listAttendanceMappingAttendance, listMappingAttendanceAbsent, itemAttendaceEntry.IdAttendanceMapAttendance, body.CurrentPosition);

                        var IdAttendanceEntry = Guid.NewGuid().ToString();
                        idTeacher = schedule
                                    .Where(e => e.IdLesson == itemScheduleLesson.IdLesson
                                       && e.IdDay == itemScheduleLesson.IdDay
                                       && e.IdSession == itemScheduleLesson.IdSession
                                       && e.IdWeek == itemScheduleLesson.IdWeek
                                        )
                                .Select(e => e.IdUser)
                                .FirstOrDefault();

                        if (idTeacher == null)
                        {
                            idTeacher = lessonTeacherAll
                              .Where(e => e.IdLesson == scheduleLesson.IdLesson)
                              .Select(e => e.IdUser)
                              .FirstOrDefault();
                        }
                        newAttendanceEntry.Add(new TrAttendanceEntryV2
                        {
                            IdAttendanceEntry = IdAttendanceEntry,
                            IdScheduleLesson = itemScheduleLesson.Id,
                            IdAttendanceMappingAttendance = itemAttendaceEntry.IdAttendanceMapAttendance,
                            LateTime = !string.IsNullOrEmpty(itemAttendaceEntry.LateInMinute) ? TimeSpan.Parse(itemAttendaceEntry.LateInMinute) : default,
                            FileEvidence = itemAttendaceEntry.File,
                            Notes = itemAttendaceEntry.Note,
                            Status = IsNeedValidation ? AttendanceEntryStatus.Pending : AttendanceEntryStatus.Submitted,
                            IsFromAttendanceAdministration = false,
                            PositionIn = body.CurrentPosition,
                            IdBinusian = idTeacher,
                            IdHomeroomStudent = itemAttendaceEntry.IdHomeroomStudent
                        });

                        if (mappingAttendance.IsUseWorkhabit)
                        {
                            foreach (var IdWorkhabit in itemAttendaceEntry.IdWorkhabits)
                            {
                                newAttendanceEntryWorkhabit.Add(new TrAttendanceEntryWorkhabitV2
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdAttendanceEntry = IdAttendanceEntry,
                                    IdMappingAttendanceWorkhabit = IdWorkhabit
                                });
                            }
                        }
                    }
                }

                _dbContext.Entity<TrAttendanceEntryV2>().AddRange(newAttendanceEntry);
                _dbContext.Entity<TrAttendanceEntryWorkhabitV2>().AddRange(newAttendanceEntryWorkhabit);
            }
            else
            {
                if (listAttendanceEntry.Any())
                {
                    foreach (var itemAttendaceEntry in listAttendanceEntry)
                    {
                        var updateAttendanceEntry = itemAttendaceEntry;
                        if (updateAttendanceEntry.IsFromAttendanceAdministration)
                            continue;

                        var exsis = body.Entries.Where(e => e.IdHomeroomStudent == updateAttendanceEntry.IdHomeroomStudent).Any();

                        if (!exsis)
                            continue;

                        var exsisAttendanceEntry = body.Entries
                                            .Where(e => e.IdHomeroomStudent == updateAttendanceEntry.IdHomeroomStudent && e.IdAttendanceMapAttendance == updateAttendanceEntry.IdAttendanceMappingAttendance)
                                            .Any();

                        if (exsisAttendanceEntry)
                            continue;

                        if (updateAttendanceEntry.Status != AttendanceEntryStatus.Submitted && body.CurrentPosition == "CA")
                        {
                            var IsNeedValidation = GetNeedValidationAtdEntryOnly(listAttendanceMappingAttendance, listMappingAttendanceAbsent, body.Entries.Where(e => e.IdHomeroomStudent == updateAttendanceEntry.IdHomeroomStudent).Select(e => e.IdAttendanceMapAttendance).FirstOrDefault(), body.CurrentPosition);

                            updateAttendanceEntry.Status = IsNeedValidation
                                            ? AttendanceEntryStatus.Pending
                                            : AttendanceEntryStatus.Submitted;
                            updateAttendanceEntry.PositionIn = "CA";
                        }

                        var LateInMinute = body.Entries.Where(e => e.IdHomeroomStudent == updateAttendanceEntry.IdHomeroomStudent).Select(e => e.LateInMinute).FirstOrDefault();
                        var LateTime = !string.IsNullOrEmpty(LateInMinute) ? TimeSpan.Parse(LateInMinute) : default;

                        updateAttendanceEntry.IdAttendanceMappingAttendance = body.Entries.Where(e => e.IdHomeroomStudent == updateAttendanceEntry.IdHomeroomStudent).Select(e => e.IdAttendanceMapAttendance).FirstOrDefault();
                        updateAttendanceEntry.LateTime = LateTime;
                        updateAttendanceEntry.FileEvidence = body.Entries.Where(e => e.IdHomeroomStudent == updateAttendanceEntry.IdHomeroomStudent).Select(e => e.File).FirstOrDefault();
                        updateAttendanceEntry.Notes = body.Entries.Where(e => e.IdHomeroomStudent == updateAttendanceEntry.IdHomeroomStudent).Select(e => e.Note).FirstOrDefault();
                        updateAttendanceEntry.PositionIn = body.CurrentPosition;

                        _dbContext.Entity<TrAttendanceEntryV2>().Update(updateAttendanceEntry);

                    }
                }

                var listIdHomeroomStudentByAttendanceEntry = listAttendanceEntry.Select(e => e.IdHomeroomStudent).ToList();
                var newAttendanceEntryByBody = body.Entries.Where(e => !listIdHomeroomStudentByAttendanceEntry.Contains(e.IdHomeroomStudent)).ToList();

                if (newAttendanceEntryByBody.Any() || listAttendanceEntryByPosition.Any())
                {
                    //var listAttendanceEntryV2 = await _dbContext.Entity<TrAttendanceEntryV2>()
                    //          .Where(e => e.IdScheduleLesson == scheduleLesson.Id)
                    //          .ToListAsync(CancellationToken);
                    //var coba1 = listAttendanceEntryV2.Select(e => e.PositionIn).ToList();

                    foreach (var itemAttendaceEntry in body.Entries)
                    {
                        var listAttendanceEntryByIdStudent = listAttendanceEntry.Where(e => e.IdHomeroomStudent == itemAttendaceEntry.IdHomeroomStudent).FirstOrDefault();

                        if (listAttendanceEntryByIdStudent == null)
                        {
                            var IsNeedValidation = GetNeedValidationAtdEntryOnly(listAttendanceMappingAttendance, listMappingAttendanceAbsent, itemAttendaceEntry.IdAttendanceMapAttendance, body.CurrentPosition);

                            var IdAttendanceEntry = Guid.NewGuid().ToString();

                            newAttendanceEntry.Add(new TrAttendanceEntryV2
                            {
                                IdAttendanceEntry = IdAttendanceEntry,
                                IdScheduleLesson = itemAttendaceEntry.IdScheduleLesson,
                                IdAttendanceMappingAttendance = itemAttendaceEntry.IdAttendanceMapAttendance,
                                LateTime = !string.IsNullOrEmpty(itemAttendaceEntry.LateInMinute) ? TimeSpan.Parse(itemAttendaceEntry.LateInMinute) : default,
                                FileEvidence = itemAttendaceEntry.File,
                                Notes = itemAttendaceEntry.Note,
                                Status = IsNeedValidation
                                                ? AttendanceEntryStatus.Pending
                                                : AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = false,
                                PositionIn = body.CurrentPosition,
                                IdBinusian = idTeacher,
                                IdHomeroomStudent = itemAttendaceEntry.IdHomeroomStudent
                            });

                            if (mappingAttendance.IsUseWorkhabit)
                            {
                                foreach (var IdWorkhabit in itemAttendaceEntry.IdWorkhabits)
                                {
                                    newAttendanceEntryWorkhabit.Add(new TrAttendanceEntryWorkhabitV2
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdAttendanceEntry = IdAttendanceEntry,
                                        IdMappingAttendanceWorkhabit = IdWorkhabit
                                    });
                                }
                            }
                        }
                        else if (listAttendanceEntryByIdStudent.PositionIn == body.CurrentPosition)
                        {
                            var exsisPosition = listAttendanceEntryByPosition.Where(e => e.IdAttendanceEntry == listAttendanceEntryByIdStudent.IdAttendanceEntry).Any();

                            if (!exsisPosition)
                                continue;

                            var IsNeedValidation = GetNeedValidationAtdEntryOnly(listAttendanceMappingAttendance, listMappingAttendanceAbsent, itemAttendaceEntry.IdAttendanceMapAttendance, body.CurrentPosition);

                            var IdAttendanceEntry = Guid.NewGuid().ToString();

                            newAttendanceEntry.Add(new TrAttendanceEntryV2
                            {
                                IdAttendanceEntry = IdAttendanceEntry,
                                IdScheduleLesson = itemAttendaceEntry.IdScheduleLesson,
                                IdAttendanceMappingAttendance = itemAttendaceEntry.IdAttendanceMapAttendance,
                                LateTime = !string.IsNullOrEmpty(itemAttendaceEntry.LateInMinute) ? TimeSpan.Parse(itemAttendaceEntry.LateInMinute) : default,
                                FileEvidence = itemAttendaceEntry.File,
                                Notes = itemAttendaceEntry.Note,
                                Status = IsNeedValidation
                                                ? AttendanceEntryStatus.Pending
                                                : AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = false,
                                PositionIn = body.CurrentPosition,
                                IdBinusian = idTeacher,
                                IdHomeroomStudent = itemAttendaceEntry.IdHomeroomStudent
                            });

                            if (mappingAttendance.IsUseWorkhabit)
                            {
                                foreach (var IdWorkhabit in itemAttendaceEntry.IdWorkhabits)
                                {
                                    newAttendanceEntryWorkhabit.Add(new TrAttendanceEntryWorkhabitV2
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdAttendanceEntry = IdAttendanceEntry,
                                        IdMappingAttendanceWorkhabit = IdWorkhabit
                                    });
                                }
                            }
                        }
                    }

                    if (body.CopyToNextSession)
                    {
                        var scheduleLessonNextSession = await _dbContext.Entity<MsScheduleLesson>()
                                  .Include(e => e.AttendanceEntryV2s).ThenInclude(e => e.AttendanceEntryWorkhabitV2s)
                                  .Include(e => e.AttendanceEntryV2s).ThenInclude(e => e.HomeroomStudent)
                                  .Where(e => e.ScheduleDate.Date == scheduleLesson.ScheduleDate.Date
                                                && e.ClassID == scheduleLesson.ClassID
                                                && e.StartTime >= scheduleLesson.StartTime
                                                && e.AcademicYear.Id == scheduleLesson.IdAcademicYear
                                                && e.Id != scheduleLesson.Id)
                                  .ToListAsync(CancellationToken);

                        var listAttendanceEntryAdm = await _dbContext.Entity<TrAttendanceEntryV2>()
                                                       .Where(e => scheduleLessonNextSession.Select(x=> x.Id).Contains(e.IdScheduleLesson) && e.IsFromAttendanceAdministration)
                                                       .Select(e => new { e.IdHomeroomStudent, e.IdScheduleLesson})
                                                       .ToListAsync(CancellationToken);

                        var listAttendanceNextSession = scheduleLessonNextSession.SelectMany(e => e.AttendanceEntryV2s).ToList();
                        var listAttendanceNextSessionCurrentPosition = listAttendanceNextSession.ToList();

                        listAttendanceNextSessionCurrentPosition = listAttendanceNextSessionCurrentPosition.Where(x => x.IsFromAttendanceAdministration == false).ToList();
                        if (PositionConstant.ClassAdvisor == body.CurrentPosition)
                        {
                            if (mappingAttendance.AbsentTerms == AbsentTerm.Session)
                            {
                                if (listHomeroom.Any())
                                    listAttendanceNextSessionCurrentPosition = listAttendanceNextSessionCurrentPosition.Where(x => listHomeroom.Contains(x.HomeroomStudent.IdHomeroom)).ToList();
                            }
                        }

                        listAttendanceNextSessionCurrentPosition.ForEach(e => e.IsActive = false);
                        _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(listAttendanceNextSessionCurrentPosition);

                        var listAttendanceNextSessionWorkhabit = listAttendanceNextSessionCurrentPosition.SelectMany(e => e.AttendanceEntryWorkhabitV2s).ToList();
                        listAttendanceNextSessionWorkhabit.ForEach(e => e.IsActive = false);
                        _dbContext.Entity<TrAttendanceEntryWorkhabitV2>().UpdateRange(listAttendanceNextSessionWorkhabit);

                        foreach (var itemScheduleLesson in scheduleLessonNextSession)
                        {
                            foreach (var itemAttendaceEntry in body.Entries)
                            {
                                if (listAttendanceEntryAdm.Any(x=> x.IdHomeroomStudent == itemAttendaceEntry.IdHomeroomStudent && x.IdScheduleLesson == itemAttendaceEntry.IdScheduleLesson))
                                    continue;

                                var IsNeedValidation = GetNeedValidationAtdEntryOnly(listAttendanceMappingAttendance, listMappingAttendanceAbsent, itemAttendaceEntry.IdAttendanceMapAttendance, body.CurrentPosition);

                                var IdAttendanceMappingAttendance = itemAttendaceEntry.IdAttendanceMapAttendance;
                                var getAttendanceMappingAttendance = listAttendanceMappingAttendance.Where(e => e.Id == itemAttendaceEntry.IdAttendanceMapAttendance).FirstOrDefault();
                                if (getAttendanceMappingAttendance.Attendance.Code == "LT")
                                    IdAttendanceMappingAttendance = listAttendanceMappingAttendance.Where(e => e.Attendance.Code == "PR").Select(e=>e.Id).FirstOrDefault();

                                var IdAttendanceEntry = Guid.NewGuid().ToString();
                                newAttendanceEntry.Add(new TrAttendanceEntryV2
                                {
                                    IdAttendanceEntry = IdAttendanceEntry,
                                    IdScheduleLesson = itemScheduleLesson.Id,
                                    IdAttendanceMappingAttendance = IdAttendanceMappingAttendance,
                                    LateTime = !string.IsNullOrEmpty(itemAttendaceEntry.LateInMinute) ? TimeSpan.Parse(itemAttendaceEntry.LateInMinute) : default,
                                    FileEvidence = itemAttendaceEntry.File,
                                    Notes = itemAttendaceEntry.Note,
                                    Status = IsNeedValidation
                                                    ? AttendanceEntryStatus.Pending
                                                    : AttendanceEntryStatus.Submitted,
                                    IsFromAttendanceAdministration = false,
                                    PositionIn = body.CurrentPosition,
                                    IdBinusian = idTeacher,
                                    IdHomeroomStudent = itemAttendaceEntry.IdHomeroomStudent
                                });

                                if (mappingAttendance.IsUseWorkhabit)
                                {
                                    foreach (var IdWorkhabit in itemAttendaceEntry.IdWorkhabits)
                                    {
                                        newAttendanceEntryWorkhabit.Add(new TrAttendanceEntryWorkhabitV2
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdAttendanceEntry = IdAttendanceEntry,
                                            IdMappingAttendanceWorkhabit = IdWorkhabit
                                        });
                                    }
                                }
                            }
                        }
                    }

                    _dbContext.Entity<TrAttendanceEntryV2>().AddRange(newAttendanceEntry);
                    _dbContext.Entity<TrAttendanceEntryWorkhabitV2>().AddRange(newAttendanceEntryWorkhabit);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region send email
            var listIdScheduleLessonEmail = newAttendanceEntry.Select(e => e.IdScheduleLesson).Distinct().ToList();

            if (KeyValues.ContainsKey("listIdScheduleLessonEmail"))
                KeyValues.Remove("listIdScheduleLessonEmail");

            KeyValues.Add("listIdScheduleLessonEmail", listIdScheduleLessonEmail);

            if (mappingAttendance.AbsentTerms == AbsentTerm.Session)
            {
                var NotificationAtd2 = ATD2Notification(KeyValues, AuthInfo);
            }
            else
            {
                var NotificationAtd1 = ATD1Notification(KeyValues, AuthInfo);
                var NotificationAtd10 = ATD10Notification(KeyValues, AuthInfo);

                if (scheduleLesson.level == "PYP")
                {
                    var NotificationAtd16 = ATD16Notification(KeyValues, AuthInfo);
                }
            }
            #endregion


            return Request.CreateApiResult2();
        }

        public static bool GetNeedValidation(List<MsAttendanceMappingAttendance> listAttendanceMappingAttendance, List<MsListMappingAttendanceAbsent> listMappingAttendanceAbsent, string IdAttendanceMapAttendance)
        {
            var idAttendance = listAttendanceMappingAttendance
                                           .Where(e => e.Id == IdAttendanceMapAttendance)
                                           .Select(e => e.IdAttendance)
                                           .FirstOrDefault();

            var IsNeedValidation = listMappingAttendanceAbsent
                                .Where(e => e.IdAttendance == idAttendance)
                                .Select(e => e.IsNeedValidation)
                                .FirstOrDefault();

            return IsNeedValidation;
        }
        public static bool GetNeedValidationAtdEntryOnly(List<MsAttendanceMappingAttendance> listAttendanceMappingAttendance, List<MsListMappingAttendanceAbsent> listMappingAttendanceAbsent, string IdAttendanceMapAttendance, string currentPosition)
        {
            bool IsNeedValidation = false;

            if (currentPosition == "ST")
            {
                var getAttendanceMappingAttendance = listAttendanceMappingAttendance
                                                  .Where(e => e.Id == IdAttendanceMapAttendance)
                                                  .FirstOrDefault();

                IsNeedValidation = listMappingAttendanceAbsent
                                    .Where(e => e.IdAttendance == getAttendanceMappingAttendance.IdAttendance
                                                && e.MsAbsentMappingAttendance.TeacherPosition.LtPosition.Code == currentPosition
                                                && e.MsAbsentMappingAttendance.IdMappingAttendance == getAttendanceMappingAttendance.IdMappingAttendance)
                                    .Select(e => e.IsNeedValidation)
                                    .FirstOrDefault();

                if (IsNeedValidation == null)
                    IsNeedValidation = false;
            }

            return IsNeedValidation;
        }
        public static string ATD10Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD10")
                {
                    IdRecipients = null,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string ATD16Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD16")
                {
                    IdRecipients = null,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string ATD2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD2")
                {
                    IdRecipients = null,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string ATD1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD1")
                {
                    IdRecipients = null,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
