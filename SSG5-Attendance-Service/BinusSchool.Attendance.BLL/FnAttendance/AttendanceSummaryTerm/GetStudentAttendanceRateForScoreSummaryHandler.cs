using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BinusSchool.Common.Constants;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Attendance.FnAttendance.Utils;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetStudentAttendanceRateForScoreSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<GetAttendanceSummaryDetailSchoolDayHandler2> _logger;

        public GetStudentAttendanceRateForScoreSummaryHandler(
            IAttendanceSummaryService attendanceSummaryService,
            IAttendanceDbContext dbContext,
            ILogger<GetAttendanceSummaryDetailSchoolDayHandler2> logger)
        {
            _attendanceSummaryService = attendanceSummaryService;
            _dbContext = dbContext;
            _logger = logger;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                return await ExecuteAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurs");
                throw;
            }
        }

        private async Task<ApiErrorResult<object>> ExecuteAsync()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailAttendanceRateRequest>();

            // ms mapping attendance
            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                .Where(e => e.Level.IdAcademicYear == param.IdAcademicYear && e.IdLevel == param.IdLevel)
                .Select(e => new
                {
                    e.Id,
                    e.IdLevel,
                    e.AbsentTerms,
                    e.IsNeedValidation,
                    e.IsUseWorkhabit,
                    e.IsUseDueToLateness,
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (mappingAttendance is null)
                throw new Exception("Data mapping is missing");

            // Get Formula
            var getFormula = await _attendanceSummaryService.GetFormulaAsync(
                        param.IdAcademicYear, param.IdLevel, CancellationToken);

            //Get Student Status
            var listIdStudent = new List<string> { param.IdStudent }.ToArray();
            var studentStatuses = await _attendanceSummaryService.GetStudentStatusesAsync(listIdStudent, param.IdAcademicYear, CancellationToken);

            // Get Student Enrollment
            var getStudentEnrollment = await _attendanceSummaryService.GetStudentEnrolledByStudentAsync(param.IdAcademicYear,
                        param.IdStudent,
                        DateTime.MinValue, CancellationToken);

            // Get Entry Student
            var getAttendanceEntries = await _attendanceSummaryService.GetAttendanceEntryByStudentAsync(param.IdAcademicYear, param.IdStudent, param.StartDate, param.EndDate, CancellationToken);
            if (!getAttendanceEntries.Any())
                throw new Exception("No Data Entries");

            // Get Schedule lesson
            var getScheduleLesson = await _attendanceSummaryService.GetScheduleLessonByGradeAsync(
                        param.IdAcademicYear, param.IdGrade, CancellationToken);

            //start logic

            //filter schedule lesson with idGrade
            var listScheduleLesson = getScheduleLesson
                  .Where(e => e.IdGrade == param.IdGrade)
                .ToList();

            var newListScheduleLesson = new List<ScheduleLessonResult>();

            foreach (var itemScheduleLesson in listScheduleLesson)
            {
                //filter student status
                if (!studentStatuses.Any(e => e.StartDt.Date <= itemScheduleLesson.ScheduleDate.Date && e.EndDt >= itemScheduleLesson.ScheduleDate.Date && e.IdStudent == param.IdStudent))
                    continue;

                var studentEnrolled = getStudentEnrollment.Where(e => e.IdHomeroomStudent == getAttendanceEntries.Select(x => x.IdHomeroomStudent).FirstOrDefault()).FirstOrDefault();

                var passed = false;
                //logic moving
                foreach (var current in studentEnrolled.Items)
                {
                    if (passed)
                        break;

                    if (string.IsNullOrWhiteSpace(current.IdLesson))
                        continue;

                    if (current.Ignored || current.IdLesson != itemScheduleLesson.IdLesson)
                        continue;

                    //when current id lesson is same as above and the date of the current moving still satisfied
                    //then set to passed, other than that will be excluded
                    if (current.StartDt.Date <= itemScheduleLesson.ScheduleDate.Date &&
                        itemScheduleLesson.ScheduleDate.Date < current.EndDt.Date)
                        passed = true;
                }

                if (!passed)
                    continue;

                newListScheduleLesson.Add(itemScheduleLesson);
            }

            // get List Subject
            var ListSubjct = newListScheduleLesson.Select(e => new
            {
                idSubject = e.Subject.Id,
                SubjectName = e.Subject.Description
            }).Distinct().ToList();

            // list Attendance entry only submitted
            var listAttendanceEntry = getAttendanceEntries
                           .Where(e => e.Status == AttendanceEntryStatus.Submitted
                                )
                            .ToList();

            var listSubject = new List<GetStudentAttendanceRateForScoreSummaryResult>();
            if (mappingAttendance.AbsentTerms == AbsentTerm.Session)
            {
                foreach (var itemSubject in ListSubjct)
                {
                    var listAttendanceEntryBySubject = listAttendanceEntry
                                       .Where(e => e.Subject.Id == itemSubject.idSubject && newListScheduleLesson.Select(x => x.Id).Contains(e.IdScheduleLesson))
                                       .GroupBy(e => new
                                       {
                                           idScheduleLesson = e.IdScheduleLesson,
                                           ScheduleDate = e.ScheduleDate,
                                           ClassID = e.ClassID,
                                           IdStudent = e.IdStudent,
                                           IdSession = e.Session.Id,
                                           SubjectID = e.Subject.SubjectID,
                                           NameSession = e.Session.Name,
                                           IdSubject = e.Subject.Id,
                                           AttendanceCode = e.Attendance.Code,
                                           AbsenceCategory = e.Attendance.AbsenceCategory,
                                           AttendanceName = e.Attendance.Description
                                       })
                                        .Select(e => e.Key)
                                        .ToList();
                    var listScheduleLessonBySubject = newListScheduleLesson.Where(e => e.Subject.Id == itemSubject.idSubject).ToList();


                    var ClassSessionToDate = listScheduleLessonBySubject.Count();
                    var Present = listAttendanceEntryBySubject.Where(e => e.AttendanceCode == "PR").Count();
                    var Late = listAttendanceEntryBySubject.Where(e => e.AttendanceCode == "LT").Count();
                    var LateArrival = listAttendanceEntryBySubject.Where(e => e.AttendanceCode == "LTA").Count();
                    var UnexcusedAbsence = listAttendanceEntryBySubject.Where(e => e.AbsenceCategory == AbsenceCategory.Unexcused).Count();
                    var ExcusedAbsence = listAttendanceEntryBySubject.Where(e => e.AbsenceCategory == AbsenceCategory.Excused).Count();

                    var listAttendanceCalculateRequest = listAttendanceEntryBySubject
                    .Where(e => e.IdSubject == itemSubject.idSubject)
                    .Select(e => new GetAttendanceCalculateRequest
                    {
                        AttendanceName = e.AttendanceName,
                        Total = 1
                    }).ToList();

                    listAttendanceCalculateRequest.Add(new GetAttendanceCalculateRequest
                    {
                        AttendanceName = SummaryTermConstant.DefaultTotalSessionName,
                        Total = ClassSessionToDate
                    });

                    var PresenceInClass = ClassSessionToDate == 0 ? 0 : FormulaUtil.CalculateAttendanceTerm(getFormula.AttendanceRate,
                                                                            AbsentTerm.Session,
                                                                            listAttendanceCalculateRequest
                                                                            .ToList());
                    var Punctuality = ClassSessionToDate == 0
                                    ? 0
                                    : Math.Round(((double)ClassSessionToDate - ((double)UnexcusedAbsence + (double)ExcusedAbsence) - (double)Late) / ((double)ClassSessionToDate - ((double)UnexcusedAbsence + (double)ExcusedAbsence)) * 100, 2);

                    var newSubject = new GetStudentAttendanceRateForScoreSummaryResult
                    {
                        IdSubject = itemSubject.idSubject,
                        Subject = itemSubject.SubjectName,
                        ClassSessionToDate = ClassSessionToDate,
                        TotalDaysToDate = 0,
                        Present = Present,
                        UnexcusedAbsence = UnexcusedAbsence,
                        ExcusedAbsence = ExcusedAbsence,
                        Late = Late,
                        PresenceInClass = PresenceInClass,
                        Punctuality = Punctuality,
                    };

                    listSubject.Add(newSubject);
                }
            }
            else
            {
                var listAttendanceEntryBySubjectDay = listAttendanceEntry
                                         .GroupBy(e => new
                                         {
                                             e.ScheduleDate,
                                             AttendanceCode = e.Attendance.Code,
                                             AbsenceCategory = e.Attendance.AbsenceCategory,
                                             AttendanceName = e.Attendance.Description
                                         })
                                         .Select(e => e.Key).ToList();

                var listScheduleLessonBySubjectDay = listScheduleLesson
                                                        .GroupBy(e => new
                                                        {
                                                            e.ScheduleDate,
                                                        })
                                                        .Select(e => e.Key).ToList();

                var TotalDaysToDate = listScheduleLessonBySubjectDay.Count();
                var Present = listAttendanceEntryBySubjectDay.Where(e => e.AttendanceCode == "PR").Count();
                var Late = listAttendanceEntryBySubjectDay.Where(e => e.AttendanceCode == "LT").Count();
                var LateArrival = listAttendanceEntryBySubjectDay.Where(e => e.AttendanceCode == "LTA").Count();
                var UnexcusedAbsence = listAttendanceEntryBySubjectDay.Where(e => e.AbsenceCategory == AbsenceCategory.Unexcused).Count();
                var ExcusedAbsence = listAttendanceEntryBySubjectDay.Where(e => e.AbsenceCategory == AbsenceCategory.Excused).Count();

                var listAttendanceCalculateRequest = listAttendanceEntryBySubjectDay.Select(e => new GetAttendanceCalculateRequest
                {
                    AttendanceName = e.AttendanceName,
                    Total = 1
                }).ToList();

                listAttendanceCalculateRequest.Add(new GetAttendanceCalculateRequest
                {
                    AttendanceName = SummaryTermConstant.DefaultTotalDayName,
                    Total = TotalDaysToDate
                });

                var PresenceInClass = TotalDaysToDate == 0 ? 0 : FormulaUtil.CalculateAttendanceTerm(getFormula.PresenceInClass,
                                                                        AbsentTerm.Day,
                                                                        listAttendanceCalculateRequest
                                                                        .ToList());
                var Punctuality = TotalDaysToDate == 0
                                ? 0
                                : Math.Round(((double)TotalDaysToDate - ((double)UnexcusedAbsence + (double)ExcusedAbsence) - (double)Late) / ((double)TotalDaysToDate - ((double)UnexcusedAbsence + (double)ExcusedAbsence)) * 100, 2);

                var newSubject = new GetStudentAttendanceRateForScoreSummaryResult
                {
                    IdSubject = null,
                    Subject = null,
                    ClassSessionToDate = 0,
                    TotalDaysToDate = TotalDaysToDate,
                    Present = Present,
                    UnexcusedAbsence = UnexcusedAbsence,
                    ExcusedAbsence = ExcusedAbsence,
                    Late = Late,
                    PresenceInClass = PresenceInClass,
                    Punctuality = Punctuality,
                };

                listSubject.Add(newSubject);
            }

            var querySubject = listSubject.Distinct();

            return Request.CreateApiResult2(querySubject as object);
        }
    }
}
