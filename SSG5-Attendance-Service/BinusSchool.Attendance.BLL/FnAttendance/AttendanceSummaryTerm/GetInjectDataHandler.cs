using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetInjectDataHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetInjectDataHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailRequest>();
            List<TrAttendanceSummaryTerm> DataAttendanceSummaryTerm = new List<TrAttendanceSummaryTerm>();
            TrAttendanceSummaryTerm Item = default;

            var GetStudentHomeroom = await _dbContext.Entity<MsHomeroomStudent>()
                                    .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                                    .Where(e => e.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                                    .Select(e => new
                                    {
                                        IdAcademicYear = e.Homeroom.Grade.Level.IdAcademicYear,
                                        IdSchool = e.Homeroom.Grade.Level.AcademicYear.IdSchool,
                                        IdGrade = e.Homeroom.IdGrade,
                                        IdLevel = e.Homeroom.Grade.IdLevel,
                                        IdStudent = e.IdStudent,
                                        IdHomeroom = e.IdHomeroom,
                                        Semester = e.Homeroom.Semester
                                    })
                                    .Distinct().ToListAsync(CancellationToken);

            var GetPeriod = await _dbContext.Entity<MsPeriod>()
                                    .ToListAsync(CancellationToken);

            var GetAttendance = await _dbContext.Entity<MsAttendance>()
                                    .Where(e=>e.IdAcademicYear==param.IdAcademicYear)
                                    .ToListAsync(CancellationToken);

            var GetWorkhobit = await _dbContext.Entity<MsMappingAttendanceWorkhabit>()
                                    .Include(e=>e.MappingAttendance).ThenInclude(e=>e.Level)
                                    .Include(e=>e.Workhabit)
                                   .Where(e => e.MappingAttendance.Level.IdAcademicYear == param.IdAcademicYear)
                                   .ToListAsync(CancellationToken);

            var GetSemester = GetStudentHomeroom.Select(e => e.Semester).Distinct().ToList();

            foreach (var ItemSemester in GetSemester)
            {
                var GetStudentHomeroomBySemeter = GetStudentHomeroom.Where(e => e.Semester == ItemSemester).ToList();

                foreach (var ItemStudent in GetStudentHomeroomBySemeter)
                {
                    var GetPeriodByIdGrade = GetPeriod.Where(e => e.IdGrade == ItemStudent.IdGrade && e.Semester==ItemSemester).ToList();
                    foreach (var ItemPeriod in GetPeriodByIdGrade)
                    {
                        var SubtringTerm = ItemPeriod.Description.Replace("Term ", "");

                        if (SubtringTerm != "1" && SubtringTerm != "2" && SubtringTerm != "3" && SubtringTerm != "4")
                        {
                            continue;
                        }

                        #region Default
                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.Default,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultRatePresenceName,
                            Total = 90,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);

                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.Default,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultRateAbsenceName,
                            Total = 80,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);

                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.Default,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultTotalSessionName,
                            Total = 300,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);

                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.Default,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultTotalDayName,
                            Total = 150,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);
                        #endregion

                        #region Attendance Category
                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.AttendanceCategory,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultExcusedName,
                            Total = 10,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);

                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.AttendanceCategory,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultUnexcusedName,
                            Total = 20,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);
                        #endregion

                        #region Attendance Status
                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.AttendanceStatus,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultSubmittedName,
                            Total = 120,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);

                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.AttendanceStatus,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultPendingName,
                            Total = 10,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);

                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.AttendanceStatus,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultUnsubmittedName,
                            Total = 20,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);
                        #endregion

                        #region Excused Absence Category
                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.ExcusedAbsenceCategory,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultAssignBySchoolName,
                            Total = 30,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);

                        Item = new TrAttendanceSummaryTerm
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = ItemStudent.IdStudent,
                            IdPeriod = ItemPeriod.Id,
                            IdAcademicYear = ItemStudent.IdAcademicYear,
                            IdSchool = ItemStudent.IdSchool,
                            IdLevel = ItemStudent.IdLevel,
                            IdGrade = ItemStudent.IdGrade,
                            IdHomeroom = ItemStudent.IdHomeroom,
                            AttendanceWorkhabitType = TrAttendanceSummaryTermType.ExcusedAbsenceCategory,
                            IdAttendanceWorkhabit = null,
                            AttendanceWorkhabitName = SummaryTermConstant.DefaultPersonalName,
                            Total = 40,
                            Semester = ItemPeriod.Semester,
                            Term = Convert.ToInt32(SubtringTerm),
                        };

                        DataAttendanceSummaryTerm.Add(Item);
                        #endregion

                        #region attendance
                        foreach (var ItemAttendance in GetAttendance)
                        {
                            Item = new TrAttendanceSummaryTerm
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdStudent = ItemStudent.IdStudent,
                                IdPeriod = ItemPeriod.Id,
                                IdAcademicYear = ItemStudent.IdAcademicYear,
                                IdSchool = ItemStudent.IdSchool,
                                IdLevel = ItemStudent.IdLevel,
                                IdGrade = ItemStudent.IdGrade,
                                IdHomeroom = ItemStudent.IdHomeroom,
                                AttendanceWorkhabitType = TrAttendanceSummaryTermType.Attendance,
                                IdAttendanceWorkhabit = ItemAttendance.Id,
                                AttendanceWorkhabitName = ItemAttendance.Description,
                                Total = 10,
                                Semester = ItemPeriod.Semester,
                                Term = Convert.ToInt32(SubtringTerm),
                            };

                            DataAttendanceSummaryTerm.Add(Item);

                        }
                        #endregion

                        #region WorkHobit
                        var GetWorkhabitByIdLevel = GetWorkhobit.Where(e => e.MappingAttendance.IdLevel == ItemStudent.IdLevel).ToList();
                        foreach (var ItemWorkhabit in GetWorkhabitByIdLevel)
                        {
                            Item = new TrAttendanceSummaryTerm
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdStudent = ItemStudent.IdStudent,
                                IdPeriod = ItemPeriod.Id,
                                IdAcademicYear = ItemStudent.IdAcademicYear,
                                IdSchool = ItemStudent.IdSchool,
                                IdLevel = ItemStudent.IdLevel,
                                IdGrade = ItemStudent.IdGrade,
                                IdHomeroom = ItemStudent.IdHomeroom,
                                AttendanceWorkhabitType = TrAttendanceSummaryTermType.Workhabit,
                                IdAttendanceWorkhabit = ItemWorkhabit.Id,
                                AttendanceWorkhabitName = ItemWorkhabit.Workhabit.Description,
                                Total = 1,
                                Semester = ItemPeriod.Semester,
                                Term = Convert.ToInt32(SubtringTerm),
                            };

                            DataAttendanceSummaryTerm.Add(Item);

                        }
                        #endregion
                    }
                }
            }

            _dbContext.Entity<TrAttendanceSummaryTerm>().AddRange(DataAttendanceSummaryTerm);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
