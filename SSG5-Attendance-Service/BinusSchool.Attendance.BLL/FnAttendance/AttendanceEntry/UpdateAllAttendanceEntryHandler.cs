using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Validator;
using BinusSchool.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry
{
    public class UpdateAllAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private GetMapAttendanceDetailResult _mapAttendance;
        private string _currentPosition;

        private readonly IAttendanceDbContext _dbContext;
        private readonly GetMapAttendanceDetailHandler _mapAttendanceHandler;

        public UpdateAllAttendanceEntryHandler(IAttendanceDbContext dbContext, GetMapAttendanceDetailHandler mapAttendanceHandler)
        {
            _dbContext = dbContext;
            _mapAttendanceHandler = mapAttendanceHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateAllAttendanceEntryRequest>();
            string idLevel = null;

            _currentPosition = body.CurrentPosition;
            // if (_currentPosition != PositionConstant.SubjectTeacher)
            //     throw new BadRequestException("Only subject teacher that can check All Present");

            if (body.Id is null && body.ClassId is null)
                throw new BadRequestException("at least data homeroom / class id must be inputted");

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x
                => x.IsGenerated
                   && x.ScheduleDate.Date == body.Date.Date);

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
                                 where
                                      _gsl.ClassID == body.ClassId
                                      && _gsl.ScheduleDate.Date == body.Date
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

            (await new UpdateAllAttendanceEntryValidator(_mapAttendance).ValidateAsync(body)).EnsureValid();


            if (_mapAttendance.Term == AbsentTerm.Session)
                predicate = predicate.And(x => x.ClassID == body.ClassId && x.IdSession == body.IdSession);

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                .Include(x => x.AttendanceEntries)
                    .ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                .OrderBy(x => x.StartTime);
            var scheduleLessons = await query.Where(predicate).ToListAsync(CancellationToken);

            var termSessionValidation = default(MapAttendanceDetailValidation);
            if (_mapAttendance.Term == AbsentTerm.Day)
            {
                var haveSubjectAssignment = scheduleLessons.Any(x => x.IdUser == AuthInfo.UserId);

                // homeroom teacher & subject teacher can submit (auto valid) attendance
                if (!haveSubjectAssignment)
                    throw new UnauthorizedAccessException("You don't have access as subject teacher");
            }
            else if (_mapAttendance.Term == AbsentTerm.Session)
            {
                // get all assignment for current user
                var assignments = await _dbContext.Entity<MsHomeroomTeacher>().Where(x => x.IdBinusian == AuthInfo.UserId)
                    .Select(x => x.TeacherPosition.Code).ToListAsync(CancellationToken);
                var idSubjects = scheduleLessons.Select(x => x.IdSubject).Distinct();
                var anySubjectAssignment = await _dbContext.Entity<MsLessonTeacher>()
                    .AnyAsync(x => x.IdUser == AuthInfo.UserId && idSubjects.Contains(x.Lesson.IdSubject));
                if (anySubjectAssignment)
                    assignments.Add(PositionConstant.SubjectTeacher);

                // check if current user can fill attendance
                termSessionValidation = _mapAttendance.TermSession.Validations.FirstOrDefault(x => x.AbsentBy.Code == body.CurrentPosition);
                // if (termSessionValidation is null || !assignments.Contains(termSessionValidation.AbsentBy.Code))
                //     throw new UnauthorizedAccessException("You don't have access to entry this attendance.");
            }
            // get student from generated schedule
            var idStudents = scheduleLessons.Select(x => x.GeneratedScheduleStudent.IdStudent).Distinct();
            // get present entry
            var entryPresent = _mapAttendance.Attendances.FirstOrDefault(x => x.Code == "PR");
            if (entryPresent is null)
                throw new NotFoundException("This school doesn't have Present attendance.");

            foreach (var idStudent in idStudents)
            {
                if (_mapAttendance.Term == AbsentTerm.Day)
                {
                    var scheduleLessonsPerStudent = scheduleLessons.Where(x => x.GeneratedScheduleStudent.IdStudent == idStudent);

                    foreach (var scheduleLesson in scheduleLessonsPerStudent)
                    {
                        // disable existing entry
                        var entryStudent = scheduleLesson.AttendanceEntries.FirstOrDefault();
                        if (entryStudent != null)
                            DisableAttendanceEntry(entryStudent);

                        if (entryStudent != null)
                        {
                            if (entryStudent.IsFromAttendanceAdministration == false)
                                CreateAttendanceEntry(scheduleLesson.Id, entryPresent.IdAttendanceMapAttendance);

                        }
                        else
                        {
                            CreateAttendanceEntry(scheduleLesson.Id, entryPresent.IdAttendanceMapAttendance);
                        }
                    }
                }
                else if (_mapAttendance.Term == AbsentTerm.Session)
                {
                    var attendanceValidation = termSessionValidation?.Attendances.FirstOrDefault(x => x.Id == entryPresent.Id);
                    if (attendanceValidation is null)
                        throw new BadRequestException(
                            $"You can't fill attendance {entryPresent.Description} for session {scheduleLessons.Find(x => x.IdSession == body.IdSession)?.SessionID}.");

                    var scheduleLessonsPerStudent = scheduleLessons.Where(x => x.GeneratedScheduleStudent.IdStudent == idStudent);

                    foreach (var scheduleLesson in scheduleLessonsPerStudent)
                    {
                        var entryStudent = scheduleLesson.AttendanceEntries.FirstOrDefault();
                        var existAttendanceValidation = false;

                        if (entryStudent != null)
                        {
                            var existAttendance = _mapAttendance.Attendances.FirstOrDefault(x => x.IdAttendanceMapAttendance == entryStudent.IdAttendanceMappingAttendance);
                            existAttendanceValidation = termSessionValidation?.Attendances.FirstOrDefault(x => x.Id == existAttendance?.Id)?.NeedToValidate ?? false;
                        }

                        var disableExistEntry = (existAttendanceValidation, entryStudent) switch
                        {
                            (false, { PositionIn: PositionConstant.SubjectTeacher }) => true,
                            (true, { PositionIn: PositionConstant.SubjectTeacher }) => false,
                            (_, { PositionIn: PositionConstant.ClassAdvisor }) => false,
                            (_, { }) => true,
                            _ => false
                        };
                        var createNewEntry = (existAttendanceValidation, entryStudent) switch
                        {
                            (false, { PositionIn: PositionConstant.SubjectTeacher }) => true,
                            (_, null) => true,
                            _ => false
                        };

                        if (disableExistEntry)
                            DisableAttendanceEntry(entryStudent);

                        if (entryStudent != null)
                        {
                            if (entryStudent.IsFromAttendanceAdministration == false)
                                if (createNewEntry)
                                    CreateAttendanceEntry(scheduleLesson.Id, entryPresent.IdAttendanceMapAttendance, attendanceValidation.NeedToValidate);
                        }
                        else
                        {
                            if (createNewEntry)
                                CreateAttendanceEntry(scheduleLesson.Id, entryPresent.IdAttendanceMapAttendance, attendanceValidation.NeedToValidate);
                        }

                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

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

            return Request.CreateApiResult2();
        }

        #region Private Method

        private void CreateAttendanceEntry(string idScheduleLesson, string idAttendanceMapAttendance, bool needToValidate = false)
        {
            var newEntry = new TrAttendanceEntry
            {
                Id = Guid.NewGuid().ToString(),
                IdGeneratedScheduleLesson = idScheduleLesson,
                IdAttendanceMappingAttendance = idAttendanceMapAttendance,
                LateTime = default,
                FileEvidence = null,
                Notes = null,
                PositionIn = _currentPosition
            };
            if (_mapAttendance.Term == AbsentTerm.Day)
                newEntry.Status = AttendanceEntryStatus.Submitted;
            else if (_mapAttendance.Term == AbsentTerm.Session)
                newEntry.Status = needToValidate ? AttendanceEntryStatus.Pending : AttendanceEntryStatus.Submitted;

            _dbContext.Entity<TrAttendanceEntry>().Add(newEntry);
        }

        private void DisableAttendanceEntry(TrAttendanceEntry entry)
        {
            if (entry.IsFromAttendanceAdministration == false)
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

        #endregion
    }
}
