using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetStudentAttendanceSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetStudentAttendanceSummaryHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentAttendanceSummaryRequest>(nameof(GetStudentAttendanceSummaryRequest.IdSchool),
                                                                                   nameof(GetStudentAttendanceSummaryRequest.IdStudent));

            var studentDetail = await _dbContext.Entity<MsStudent>()
                                                .Include(x => x.HomeroomStudents)
                                                    .ThenInclude(x => x.Homeroom)
                                                        .ThenInclude(x => x.Grade)
                                                            .ThenInclude(x => x.Level)
                                                .Include(x => x.HomeroomStudents)
                                                    .ThenInclude(x => x.Homeroom)
                                                        .ThenInclude(x => x.GradePathwayClassroom)
                                                            .ThenInclude(x => x.Classroom)
                                                .Where(x => x.Id == param.IdStudent)
                                                .SingleOrDefaultAsync(CancellationToken);
            if (studentDetail is null)
                throw new NotFoundException("Student is not found");

            var currentAcademic = await _dbContext.Entity<MsPeriod>()
                                                  .Include(x => x.Grade)
                                                       .ThenInclude(x => x.Level)
                                                           .ThenInclude(x => x.AcademicYear)
                                                  .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool
                                                              && _dateTime.ServerTime.Date >= x.StartDate.Date
                                                              && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                                  .Select(x => new CurrentAcademicYearResult
                                                  {
                                                      Id = x.Grade.Level.AcademicYear.Id,
                                                      Code = x.Grade.Level.AcademicYear.Code,
                                                      Description = x.Grade.Level.AcademicYear.Description,
                                                      Semester = x.Semester
                                                  }).FirstOrDefaultAsync();
            if (currentAcademic is null)
                throw new NotFoundException("Current academic year is not defined");

            var currentStudentHomeroom = studentDetail.HomeroomStudents.Where(x => x.Homeroom.IdAcademicYear == currentAcademic.Id
                                                                                   && x.Homeroom.Semester == currentAcademic.Semester)
                                                                       .Select(x => x.Homeroom)
                                                                       .FirstOrDefault();
            if (currentStudentHomeroom is null)
            {
                //handle kemungkinan student hanya di salah satu semester saja
                var semester = currentAcademic.Semester == 1 ? 2 : 1; 
                currentStudentHomeroom = studentDetail.HomeroomStudents.Where(x => x.Homeroom.IdAcademicYear == currentAcademic.Id
                                                                                   && x.Homeroom.Semester == semester)
                                                                       .Select(x => x.Homeroom)
                                                                       .FirstOrDefault();
                if (currentStudentHomeroom is null)
                    throw new NotFoundException("Student's current homeroom is not defined");
            }

            var mapping = await _dbContext.Entity<MsMappingAttendance>()
                                          .Include(x => x.Level)
                                                .ThenInclude(x => x.Formulas)
                                          .Where(x => x.IdLevel == currentStudentHomeroom.Grade.IdLevel)
                                          .Select(x => new
                                          {
                                              Attendances = x.AttendanceMappingAttendances.Select(y => new
                                              {
                                                  y.IdAttendance,
                                                  y.Attendance.Description
                                              }),
                                              x.AbsentTerms,
                                              x.IsUseWorkhabit,
                                              Workhabits = x.IsUseWorkhabit ?
                                                           x.MappingAttendanceWorkhabits.Select(y => new
                                                           {
                                                               y.Id,
                                                               y.IdWorkhabit,
                                                               y.Workhabit.Description
                                                           }) : null,
                                              x.Level.Formulas
                                          })
                                          .FirstOrDefaultAsync(CancellationToken);
            if (mapping is null)
                throw new NotFoundException($"mapping is not found for level {currentStudentHomeroom.Grade.Level.Description}");
            if (!mapping.Formulas.Any(x => x.IsActive))
                throw new NotFoundException($"formula is not found for level {currentStudentHomeroom.Grade.Level.Description}");

            var studentSchedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                  .Include(x => x.GeneratedScheduleStudent)
                                                       .ThenInclude(x => x.Student)
                                                  .Include(x => x.AttendanceEntries)
                                                       .ThenInclude(x => x.AttendanceEntryWorkhabits)
                                                           .ThenInclude(x => x.MappingAttendanceWorkhabit)
                                                  .Include(x => x.AttendanceEntries)
                                                       .ThenInclude(x => x.AttendanceMappingAttendance)
                                                           .ThenInclude(x => x.Attendance)
                                                   .Where(x => x.IsGenerated
                                                               && x.IdHomeroom == currentStudentHomeroom.Id
                                                               && x.GeneratedScheduleStudent.IdStudent == param.IdStudent)
                                                   .ToListAsync(CancellationToken);

            var summary = await _dbContext.Entity<TrAttendanceSummary>()
                                          .Include(x => x.AttendanceSummaryMappingAtds).ThenInclude(x=> x.Attendance)
                                          .Include(x => x.AttendanceSummaryWorkhabits)
                                          .Where(x => x.IdStudent == param.IdStudent)
                                          .OrderByDescending(x=> x.Date)
                                          .FirstOrDefaultAsync();                                        

            var result = new GetStudentAttendanceSummaryResult
            {
                BinusianId = studentDetail.Id,
                StudentName = NameUtil.GenerateFullName(studentDetail.FirstName, studentDetail.MiddleName, studentDetail.LastName),
                Homeroom = $"{currentStudentHomeroom.Grade.Code} {currentStudentHomeroom.GradePathwayClassroom.Classroom.Code}",
                Term = mapping.AbsentTerms,
                UseWorkhabit = mapping.IsUseWorkhabit,
                AttendanceRate = mapping.Formulas.First(y => y.IsActive).AttendanceRate.Calculate(mapping.AbsentTerms, studentSchedules),
                // AttendanceRate = mapping.Formulas.First(y => y.IsActive).AttendanceRate.Calculate(mapping.AbsentTerms, summary),
                TotalDay = mapping.AbsentTerms == AbsentTerm.Day ? studentSchedules.GroupBy(x => x.ScheduleDate).Count() : studentSchedules.Count(),
                // TotalDay = mapping.AbsentTerms == AbsentTerm.Day ? summary.TotalDays : summary.TotalSession,
                Attendances = new List<AttendanceStudent>(),
                Workhabits = new List<WorkhabitStudent>(),
                ValidDate = _dateTime.ServerTime
                // ValidDate = summary.DateUp.HasValue ? summary.DateUp.Value : summary.DateIn.Value  
            };

            foreach (var attendance in mapping.Attendances)
            {
                result.Attendances.Add(new AttendanceStudent
                {
                    IdAttendance = attendance.IdAttendance,
                    AttendanceName = attendance.Description,
                    Count = mapping.AbsentTerms == AbsentTerm.Day ?
                           studentSchedules.Where(x => x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.Attendance.Id == attendance.IdAttendance && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count() :
                           studentSchedules.Count(x => x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.Attendance.Id == attendance.IdAttendance && y.Status == AttendanceEntryStatus.Submitted))
                    // Count = mapping.AbsentTerms == AbsentTerm.Day ?
                    //         summary.AttendanceSummaryMappingAtds.Where(x => x.IdAttendance == attendance.IdAttendance).Select(x => x.CountAsDay).FirstOrDefault() :
                    //         summary.AttendanceSummaryMappingAtds.Where(x => x.IdAttendance == attendance.IdAttendance).Select(x => x.CountAsSession).FirstOrDefault()
                });
            }

            if (result.UseWorkhabit)
            {
                foreach (var workhabit in mapping.Workhabits)
                {
                    result.Workhabits.Add(new WorkhabitStudent
                    {
                        IdWorkhabit = workhabit.Id,
                        WorkhabitName = workhabit.Description,
                        Count = mapping.AbsentTerms == AbsentTerm.Day ?
                               studentSchedules.Where(x => x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.Id == workhabit.Id) && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count() :
                               studentSchedules.Count(x => x.AttendanceEntries.Any(y => y.AttendanceEntryWorkhabits.Any(z => z.MappingAttendanceWorkhabit.Id == workhabit.Id) && y.Status == AttendanceEntryStatus.Submitted))
                        // Count = mapping.AbsentTerms == AbsentTerm.Day ?
                        //     summary.AttendanceSummaryWorkhabits.Where(x => x.IdWorkHabit == workhabit.IdWorkhabit).Select(x => x.CountAsDay).FirstOrDefault() :
                        //     summary.AttendanceSummaryWorkhabits.Where(x => x.IdWorkHabit == workhabit.IdWorkhabit).Select(x => x.CountAsSession).FirstOrDefault()

                    });
                }
            }
            return Request.CreateApiResult2(result as object);
        }
    }
}
