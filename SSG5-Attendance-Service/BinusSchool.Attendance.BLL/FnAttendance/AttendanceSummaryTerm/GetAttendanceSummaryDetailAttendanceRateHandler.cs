using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
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
    public class GetAttendanceSummaryDetailAttendanceRateHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;
        public GetAttendanceSummaryDetailAttendanceRateHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailAttendanceRateRequest>();
            var _columns = new[] { "subject", "classSessionToDate", "totalDaysToDate", "present", "unexcusedAbsence", "excusedAbsence", "late", "presenceInClass", "punctuality" };

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisTrHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetTrHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisAttendanceEntry = await AttendanceSummaryRedisCacheHandler.GetAttendanceEntry(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisScheduleLesson = await AttendanceSummaryRedisCacheHandler.GetScheduleLesson(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisFormula = await AttendanceSummaryRedisCacheHandler.GetFormula(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisStudentStatus = await AttendanceSummaryRedisCacheHandler.GetStudentStatus(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            #endregion


            var listScheduleLesson = redisScheduleLesson
                              .Where(e => e.IdLevel == param.IdLevel
                                        && e.IdGrade == param.IdGrade
                                        && (e.ScheduleDate.Date >= param.StartDate.Date && e.ScheduleDate.Date <= param.EndDate.Date))
                            .ToList();

            List<RedisAttendanceSummaryScheduleLessonResult> newListScheduleLesson = new List<RedisAttendanceSummaryScheduleLessonResult>();
            foreach (var itemScheduleLesson in listScheduleLesson)
            {
                var listStatusStudentByDate = redisStudentStatus.Where(e => e.StartDate.Date <= itemScheduleLesson.ScheduleDate.Date).Select(e => e.IdStudent).ToList();

                //moving student
                var listStudentEnrolmentBySchedule = redisHomeroomStudentEnrollment
                                    .Where(e => e.IdStudent == param.IdStudent && e.Semester == itemScheduleLesson.Semester)
                                    .ToList();

                var listTrStudentEnrolmentBySchedule = redisTrHomeroomStudentEnrollment
                                            .Where(e => e.IdStudent == param.IdStudent && e.Semester == itemScheduleLesson.Semester)
                                            .ToList();

                var listStudentEnrollmentUnion = listStudentEnrolmentBySchedule.Union(listTrStudentEnrolmentBySchedule)
                                                   .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                   .ToList();

                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, itemScheduleLesson.ScheduleDate, itemScheduleLesson.Semester.ToString(), itemScheduleLesson.IdLesson);

                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                              .ToList();

                if (studentEnrollmentMoving.Any())
                {
                    newListScheduleLesson.Add(itemScheduleLesson);
                }
            }

            var ListSubjct = newListScheduleLesson.Select(e => new
            {
                idSubject = e.Subject.Id,
                SubjectName = e.Subject.Description
            }).Distinct().ToList();

            var listMappingAttendance = redisMappingAttendance
                               .Where(e => e.IdLevel == param.IdLevel)
                               .Select(e => new RedisAttendanceSummaryMappingAttendanceResult
                               {
                                   IdLevel = e.IdLevel,
                                   AbsentTerms = e.AbsentTerms
                               })
                               .ToList();

            List<GetAttendanceSummaryDetailAttendanceRateResult> listSubject = new List<GetAttendanceSummaryDetailAttendanceRateResult>();

            var listAttendanceEntry = redisAttendanceEntry
                                       .Where(e => (e.ScheduleDate.Date >= param.StartDate.Date && e.ScheduleDate.Date <= param.EndDate.Date)
                                                && e.IdStudent == param.IdStudent
                                                && e.Status == AttendanceEntryStatus.Submitted
                                            )
                                        .ToList();

            if (listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any())
            {
                foreach (var itemSubject in ListSubjct)
                {
                    var listAttendanceEntryPerSubject = listAttendanceEntry
                                       .Where(e => e.Subject.Id == itemSubject.idSubject)
                                       .GroupBy(e => new
                                       {
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
                    var listAttendanceEntryBySubject = listAttendanceEntryPerSubject.Where(e => e.IdSubject == itemSubject.idSubject).ToList();
                    var listScheduleLessonBySubject = newListScheduleLesson.Where(e => e.Subject.Id == itemSubject.idSubject).ToList();


                    var ClassSessionToDate = listScheduleLessonBySubject.Count();
                    var Present = listAttendanceEntryBySubject.Where(e => e.AttendanceCode == "PR").Count();
                    var Late = listAttendanceEntryBySubject.Where(e => e.AttendanceCode == "LT").Count();
                    var LateArrival = listAttendanceEntryBySubject.Where(e => e.AttendanceCode == "LTA").Count();
                    var UnexcusedAbsence = listAttendanceEntryBySubject.Where(e => e.AbsenceCategory == AbsenceCategory.Unexcused).Count();
                    var ExcusedAbsence = listAttendanceEntryBySubject.Where(e => e.AbsenceCategory == AbsenceCategory.Excused).Count();

                    var listAttendanceCalculateRequest = listAttendanceEntryPerSubject
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

                    var PresenceInClass = ClassSessionToDate == 0 ? 0 : FormulaUtil.CalculateAttendanceTerm(redisFormula.AttendanceRate,
                                                                            AbsentTerm.Session,
                                                                            listAttendanceCalculateRequest
                                                                            .ToList());
                    var Punctuality = ClassSessionToDate == 0
                                    ? 0
                                    : Math.Round(((double)ClassSessionToDate - ((double)UnexcusedAbsence + (double)ExcusedAbsence) - (double)Late) / ((double)ClassSessionToDate - ((double)UnexcusedAbsence + (double)ExcusedAbsence)) * 100, 2);

                    var newSubject = new GetAttendanceSummaryDetailAttendanceRateResult
                    {
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

                var PresenceInClass = TotalDaysToDate == 0 ? 0 : FormulaUtil.CalculateAttendanceTerm(redisFormula.PresenceInClass,
                                                                        AbsentTerm.Day,
                                                                        listAttendanceCalculateRequest
                                                                        .ToList());
                var Punctuality = TotalDaysToDate == 0
                                ? 0
                                : Math.Round(((double)TotalDaysToDate - ((double)UnexcusedAbsence + (double)ExcusedAbsence) - (double)Late) / ((double)TotalDaysToDate - ((double)UnexcusedAbsence + (double)ExcusedAbsence)) * 100, 2);

                var newSubject = new GetAttendanceSummaryDetailAttendanceRateResult
                {
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
            if (!string.IsNullOrEmpty(param.Search))
                querySubject = querySubject.Where(e => e.Subject.Contains(param.Search));

            switch (param.OrderBy)
            {
                case "subject":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Subject)
                        : querySubject.OrderBy(x => x.Subject);
                    break;
                case "classSessionToDate":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.ClassSessionToDate)
                        : querySubject.OrderBy(x => x.ClassSessionToDate);
                    break;
                case "totalDaysToDate":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.TotalDaysToDate)
                        : querySubject.OrderBy(x => x.TotalDaysToDate);
                    break;
                case "present":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Present)
                        : querySubject.OrderBy(x => x.Present);
                    break;
                case "unexcusedAbsence":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.UnexcusedAbsence)
                        : querySubject.OrderBy(x => x.UnexcusedAbsence);
                    break;
                case "excusedAbsence":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.ExcusedAbsence)
                        : querySubject.OrderBy(x => x.ExcusedAbsence);
                    break;
                case "late":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Late)
                        : querySubject.OrderBy(x => x.Late);
                    break;
                case "presenceInClass":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.PresenceInClass)
                        : querySubject.OrderBy(x => x.PresenceInClass);
                    break;
                case "punctuality":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Punctuality)
                        : querySubject.OrderBy(x => x.Punctuality);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = querySubject
                    .ToList();
            }
            else
            {
                items = querySubject
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : querySubject.Select(x => x.Subject).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
