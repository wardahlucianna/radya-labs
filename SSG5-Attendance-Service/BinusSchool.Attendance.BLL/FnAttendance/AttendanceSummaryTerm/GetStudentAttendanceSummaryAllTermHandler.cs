using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2.Validator;
using BinusSchool.Attendance.FnAttendance.Utils;
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
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetStudentAttendanceSummaryAllTermHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetStudentAttendanceSummaryAllTermHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentAttendanceSummaryAllTermRequest>(
                nameof(GetStudentAttendanceSummaryAllTermRequest.IdStudent),
                nameof(GetStudentAttendanceSummaryAllTermRequest.IdGrade),
                nameof(GetStudentAttendanceSummaryAllTermRequest.Semester));

            if (!string.IsNullOrEmpty(param.Term))
            {
                param.Term = new String(param.Term.Where(Char.IsDigit).ToArray());
            }

            var result = await GetAttandenceAllTerm(param);

            return Request.CreateApiResult2(result as object);
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<GetStudentAttendanceSummaryAllTermResult>> GetAttandenceAllTerm(GetStudentAttendanceSummaryAllTermRequest param)
        {
            var IdStudent = ProtectedString.CheckIdStudentAndDate(param.IdStudent);

            var GetLevelAY = await _dbContext.Entity<MsGrade>()
                        .Include(x => x.Level)
                        .Where(a => a.Id == param.IdGrade)
                        .Select(a => new {
                            a.IdLevel,
                            levelName = a.Level.Description,
                            a.Level.IdAcademicYear
                        }).FirstOrDefaultAsync();

            var GetMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                       .Include(e => e.Level)
                                       .Where(x => x.IdLevel == GetLevelAY.IdLevel)
                                       .FirstOrDefaultAsync(CancellationToken);

            var GetFormula = await _dbContext.Entity<MsFormula>()
                            .Include(e => e.Level)
                            .Where(x => x.IdLevel == GetLevelAY.IdLevel)
                            .FirstOrDefaultAsync(CancellationToken);

            var GetAbsentMappingAttendance = await _dbContext.Entity<MsAttendance>()
                           .Where(x => x.IdAcademicYear == GetLevelAY.IdAcademicYear)
                           .ToListAsync(CancellationToken);

            var GetExcusedAbsen = GetAbsentMappingAttendance.Where(e => e.AbsenceCategory == AbsenceCategory.Excused && e.Code != "SE").Select(e => e.Id).ToList();
            var GetUnexcusedAbsen = GetAbsentMappingAttendance.Where(e => e.AbsenceCategory == AbsenceCategory.Unexcused).Select(e => e.Id).ToList();
            //var GetUaEa = GetAbsentMappingAttendance.Where(e => e.AbsenceCategory != null).Select(e => e.Id).ToList();
            var GetLate = GetAbsentMappingAttendance.Where(e => e.Code == "LT").Select(e => e.Id).ToList();

            var GetAbsence = GetAbsentMappingAttendance.Where(e => e.AttendanceCategory == AttendanceCategory.Absent).Select(e => e.Id).ToList();
            var GetPresence = GetAbsentMappingAttendance.Where(e => e.AbsenceCategory != null).Select(e => e.Id).ToList();
            var GetSick = GetAbsentMappingAttendance.Where(e => e.Code == "SE").Select(e => e.Id).ToList();

            //add new steven 15/05/23
            var IdPresent = GetAbsentMappingAttendance.Where(e => e.Code == "PR").Select(e => e.Id).ToList();

            var GetPeriod = await _dbContext.Entity<MsPeriod>()
              .Include(e => e.Grade).ThenInclude(e => e.Level)
              .Where(x => x.Grade.Level.IdAcademicYear == GetLevelAY.IdAcademicYear)
              .ToListAsync(CancellationToken);

            var Query = _dbContext.Entity<TrAttendanceSummaryTerm>()
                .Include(e => e.Level)
                .Include(e => e.AcademicYear)
                .Include(e => e.Grade)
                .Include(e => e.Student)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Where(x => x.IdGrade == param.IdGrade
                            && x.IdStudent == IdStudent
                            && x.Semester <= (param.Semester == 0 ? x.Semester : param.Semester)
                            && Convert.ToInt32(x.Term) <= (string.IsNullOrEmpty(param.Term) ? Convert.ToInt32(x.Term) : Convert.ToInt32(param.Term)));

            var DataAttendanceSummaryTerm = await Query.ToListAsync(CancellationToken);

            var DataAttendanceSummaryTermByStudent = DataAttendanceSummaryTerm.Select(x => new
            {
                IdStudent = x.IdStudent,
                StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                Semester = x.Semester,
                Term = (!string.IsNullOrEmpty(param.Term) ? x.Term.ToString() : "")
            }).Distinct().ToList().OrderBy(x => x.Semester).ThenBy(x => x.Term);

            var DataAttendanceSummary = DataAttendanceSummaryTermByStudent
            .Select(e => new GetStudentAttendanceSummaryAllTermResult
            {
                Student = new NameValueVm
                {
                    Id = e.IdStudent,
                    Name = e.StudentName
                },
                AttendanceRate = FormulaUtil.CalculateNew(GetFormula.AttendanceRate,
                                                            GetMappingAttendance.AbsentTerms,
                                                            DataAttendanceSummaryTerm
                                                            .Where(x => x.IdStudent == e.IdStudent
                                                            && x.Semester == e.Semester
                                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString()))
                                                            .ToList()) >= 0
                                                            ? FormulaUtil.CalculateNew(GetFormula.AttendanceRate,
                                                                GetMappingAttendance.AbsentTerms,
                                                                DataAttendanceSummaryTerm
                                                                .Where(x => x.IdStudent == e.IdStudent
                                                                 && x.Semester == e.Semester
                                                                 && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString()))
                                                                .ToList())
                                                            : 0,
                ClassSession = GetMappingAttendance.AbsentTerms == AbsentTerm.Day
                                    ? DataAttendanceSummaryTerm
                                        .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalDayName
                                            && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default
                                            && x.IdStudent == e.IdStudent
                                            && x.Semester == e.Semester
                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString()))
                                        .Select(x => x.Total)
                                        .Sum()
                                    : DataAttendanceSummaryTerm
                                        .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultTotalSessionName
                                            && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Default
                                            && x.IdStudent == e.IdStudent
                                            && x.Semester == e.Semester
                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString()))
                                        .Select(x => x.Total)
                                        .Sum(),
                UnexcusedAbsent = DataAttendanceSummaryTerm
                                        .Where(x => GetUnexcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                            && x.IdStudent == e.IdStudent
                                            && x.Semester == e.Semester
                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                            && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                        .Select(x => x.Total).Sum(),
                UnexcusedAbsentInDays = DataAttendanceSummaryTerm
                                        .Where(x => GetUnexcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                            && x.IdStudent == e.IdStudent
                                            && x.Semester == e.Semester
                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                            && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                        .Select(x => x.TotalInDays ?? 0).Sum(),
                Lateness = DataAttendanceSummaryTerm
                            .Where(x => GetLate.Contains(x.IdAttendanceWorkhabit)
                                && x.IdStudent == e.IdStudent
                                && x.Semester == e.Semester
                                && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                            .Select(x => x.Total).Sum(),
                PresenceCount = DataAttendanceSummaryTerm
                            .Where(x => IdPresent.Contains(x.IdAttendanceWorkhabit)
                                && x.IdStudent == e.IdStudent
                                && x.Semester == e.Semester
                                && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                            .Select(x => x.Total).Sum(),
                SickCount = DataAttendanceSummaryTerm
                            .Where(x => GetSick.Contains(x.IdAttendanceWorkhabit)
                                && x.IdStudent == e.IdStudent
                                && x.Semester == e.Semester
                                && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                            .Select(x => x.Total).Sum(),
                SickCountInDays = DataAttendanceSummaryTerm
                            .Where(x => GetSick.Contains(x.IdAttendanceWorkhabit)
                                && x.IdStudent == e.IdStudent
                                && x.Semester == e.Semester
                                && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                            .Select(x => x.TotalInDays ?? 0).Sum(),
                PresenceRate = FormulaUtil.CalculateNew(GetFormula.PresenceInClass,
                                                            GetMappingAttendance.AbsentTerms,
                                                            DataAttendanceSummaryTerm
                                                            .Where(x => x.IdStudent == e.IdStudent
                                                            && x.Semester == e.Semester
                                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString()))
                                                            .ToList()) >= 0
                                                            ? FormulaUtil.CalculateNew(GetFormula.PresenceInClass,
                                                                GetMappingAttendance.AbsentTerms,
                                                                DataAttendanceSummaryTerm
                                                                .Where(x => x.IdStudent == e.IdStudent
                                                                && x.Semester == e.Semester
                                                                && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString()))
                                                                .ToList())
                                                            : 0,

                ExcusedAbsence = GetAbsentMappingAttendance
                                .Where(e => e.ExcusedAbsenceCategory != null)
                                .Any()
                                ? new List<ExcusedAbsence>()
                                    {
                                            new ExcusedAbsence()
                                            {
                                                Category = ExcusedAbsenceCategory.Personal,
                                                Count = DataAttendanceSummaryTerm
                                                        .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultPersonalName
                                                            && x.IdStudent == e.IdStudent
                                                            && x.Semester == e.Semester
                                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                                            && x.AttendanceWorkhabitType== TrAttendanceSummaryTermType.ExcusedAbsenceCategory)
                                                        .Select(x => x.Total).Sum()
                                            },
                                            new ExcusedAbsence()
                                            {
                                                Category = ExcusedAbsenceCategory.AssignBySchool,
                                                Count = DataAttendanceSummaryTerm
                                                        .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultAssignBySchoolName
                                                            && x.IdStudent == e.IdStudent
                                                            && x.Semester == e.Semester
                                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                                            && x.AttendanceWorkhabitType== TrAttendanceSummaryTermType.ExcusedAbsenceCategory)
                                                        .Select(x => x.Total).Sum()
                                            }
                                    }
                                : new List<ExcusedAbsence>()
                                {
                                         new ExcusedAbsence()
                                            {
                                                Category = null,
                                                Count = DataAttendanceSummaryTerm
                                                        .Where(x => GetExcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                                            && x.IdStudent == e.IdStudent
                                                            && x.Semester == e.Semester
                                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                                            && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                                        .Select(x => x.Total).Sum()
                                            }
                                },
                ExcusedAbsenceInDays = GetAbsentMappingAttendance
                                .Where(e => e.ExcusedAbsenceCategory != null)
                                .Any()
                                ? new List<ExcusedAbsence>()
                                    {
                                            new ExcusedAbsence()
                                            {
                                                Category = ExcusedAbsenceCategory.Personal,
                                                Count = DataAttendanceSummaryTerm
                                                        .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultPersonalName
                                                            && x.IdStudent == e.IdStudent
                                                            && x.Semester == e.Semester
                                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                                            && x.AttendanceWorkhabitType== TrAttendanceSummaryTermType.ExcusedAbsenceCategory)
                                                        .Select(x => x.TotalInDays??0).Sum()
                                            },
                                            new ExcusedAbsence()
                                            {
                                                Category = ExcusedAbsenceCategory.AssignBySchool,
                                                Count = DataAttendanceSummaryTerm
                                                        .Where(x => x.AttendanceWorkhabitName == SummaryTermConstant.DefaultAssignBySchoolName
                                                            && x.IdStudent == e.IdStudent
                                                            && x.Semester == e.Semester
                                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                                            && x.AttendanceWorkhabitType== TrAttendanceSummaryTermType.ExcusedAbsenceCategory)
                                                        .Select(x => x.TotalInDays??0).Sum()
                                            }
                                    }
                                : new List<ExcusedAbsence>()
                                {
                                         new ExcusedAbsence()
                                            {
                                                Category = null,
                                                Count = DataAttendanceSummaryTerm
                                                        .Where(x => GetExcusedAbsen.Contains(x.IdAttendanceWorkhabit)
                                                            && x.IdStudent == e.IdStudent
                                                            && x.Semester == e.Semester
                                                            && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                                            && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                                                        .Select(x => x.TotalInDays??0).Sum()
                                            }
                                },
                StartDate = string.IsNullOrEmpty(param.Term)
                            ? GetPeriod.Where(x => x.IdGrade == param.IdGrade && x.Semester == (param.Semester == 0 ? x.Semester : param.Semester)).Min(e => e.StartDate)
                            : GetPeriod.Where(x => x.IdGrade == param.IdGrade && x.Semester == (param.Semester == 0 ? x.Semester : param.Semester) && x.Code.Contains(param.Term)).Min(e => e.StartDate),
                EndDate = string.IsNullOrEmpty(param.Term)
                            ? GetPeriod.Where(x => x.IdGrade == param.IdGrade && x.Semester == (param.Semester == 0 ? x.Semester : param.Semester)).Max(e => e.EndDate)
                            : GetPeriod.Where(x => x.IdGrade == param.IdGrade && x.Semester == (param.Semester == 0 ? x.Semester : param.Semester) && x.Code.Contains(param.Term)).Max(e => e.EndDate),
                Absence = DataAttendanceSummaryTerm
                            .Where(x => GetAbsence.Contains(x.IdAttendanceWorkhabit)
                                && x.IdStudent == e.IdStudent
                                && x.Semester == e.Semester
                                && x.Term.ToString() == (e.Term != "" ? e.Term : x.Term.ToString())
                                && x.AttendanceWorkhabitType == TrAttendanceSummaryTermType.Attendance)
                            .Select(x => x.Total).Sum(),
                Semester = e.Semester,
                Term = e.Term ?? ""

            })
            .ToList();

            var query = DataAttendanceSummary.Distinct();

            return query;
        }
    }
}
