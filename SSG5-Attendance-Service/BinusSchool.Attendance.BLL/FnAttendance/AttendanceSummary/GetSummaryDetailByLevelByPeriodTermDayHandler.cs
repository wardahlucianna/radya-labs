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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailByLevelByPeriodTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetAvailabilityPositionByUserHandler _availabilityPositionByUser;
        public GetSummaryDetailByLevelByPeriodTermDayHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime,
            GetAvailabilityPositionByUserHandler availabilityPositionByUser)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _availabilityPositionByUser = availabilityPositionByUser;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSummaryDetailByPeriodRequest>(nameof(GetSummaryDetailByPeriodRequest.IdLevel),
                                                                               nameof(GetSummaryDetailByPeriodRequest.Semester));
            #region Get Position User
            List<string> avaiablePosition = new List<string>();
            var positionUser = await _availabilityPositionByUser.GetAvailablePositionDetail(param.IdUser, param.IdAcademicYear);
            // if (positionUser.ClassAdvisors.Count == 0 && positionUser.SubjectTeachers.Count == 0 && positionUser.OtherPositions.Count == 0)
            //     throw new BadRequestException($"You dont have any position.");
            var predicateLevel = PredicateBuilder.Create<MsLevel>(x => 1 == 1);
            var predicateLevelPrincipalAndVicePrincipal = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
            var predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
            var predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
            var predicateStudentGrade = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
            List<string> idLevelPrincipalAndVicePrincipal = new List<string>();
            if (positionUser.OtherPositions.Count > 0)
            {
                if (param.SelectedPosition == PositionConstant.Principal)
                {
                    if (positionUser.OtherPositions.Any(y => y.Id == PositionConstant.Principal)) //check P Or VP
                    {
                        if (positionUser.OtherPositions.Where(y => y.Id == PositionConstant.Principal).ToList() != null && positionUser.OtherPositions.Where(y => y.Code == PositionConstant.Principal).Count() > 0)
                        {
                            var Principal = positionUser.OtherPositions.Where(x => x.Code == PositionConstant.Principal).ToList();

                            foreach (var item in Principal)
                            {
                                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);
                                idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
                            }
                            predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => idLevelPrincipalAndVicePrincipal.Contains(x.Grade.IdLevel));
                        }
                    }
                }
                if (param.SelectedPosition == PositionConstant.VicePrincipal)
                {
                    if (positionUser.OtherPositions.Any(y => y.Id == PositionConstant.VicePrincipal))
                    {
                        if (positionUser.OtherPositions.Where(y => y.Id == PositionConstant.VicePrincipal).ToList() != null && positionUser.OtherPositions.Where(y => y.Code == PositionConstant.VicePrincipal).Count() > 0)
                        {
                            var Principal = positionUser.OtherPositions.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();
                            List<string> IdLevels = new List<string>();
                            foreach (var item in Principal)
                            {
                                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);
                                IdLevels.Add(_levelLH.Id);
                            }
                            predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => IdLevels.Contains(x.Grade.IdLevel));
                        }
                    }
                }
                if (param.SelectedPosition == PositionConstant.LevelHead)
                {
                    if (positionUser.OtherPositions.Where(y => y.Id == PositionConstant.LevelHead).ToList() != null)
                    {
                        var LevelHead = positionUser.OtherPositions.Where(x => x.Id == PositionConstant.LevelHead).ToList();
                        List<string> IdGrade = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            _dataNewLH.TryGetValue("Grade", out var _gradeLH);
                            IdGrade.Add(_gradeLH.Id);
                        }
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectHead)
                {
                    if (positionUser.OtherPositions.Where(y => y.Id == PositionConstant.SubjectHead).ToList() != null)
                    {
                        var LevelHead = positionUser.OtherPositions.Where(x => x.Id == PositionConstant.SubjectHead).ToList();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Level", out var _leveltSH);
                            _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                            IdGrade.Add(_gradeSH.Id);
                            IdSubject.Add(_subjectSH.Id);
                        }
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
                {
                    if (positionUser.OtherPositions.Where(y => y.Id == PositionConstant.SubjectHeadAssitant).ToList() != null)
                    {
                        var LevelHead = positionUser.OtherPositions.Where(x => x.Id == PositionConstant.SubjectHeadAssitant).ToList();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Level", out var _leveltSH);
                            _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                            IdGrade.Add(_gradeSH.Id);
                            IdSubject.Add(_subjectSH.Id);
                        }
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
                if (param.SelectedPosition == PositionConstant.HeadOfDepartment)
                {
                    if (positionUser.OtherPositions.Where(y => y.Id == PositionConstant.HeadOfDepartment).ToList() != null)
                    {
                        var HOD = positionUser.OtherPositions.Where(x => x.Id == PositionConstant.HeadOfDepartment).ToList();
                        List<string> idDepartment = new List<string>();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        foreach (var item in HOD)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            idDepartment.Add(_departmentSH.Id);
                        }
                        var departments = await _dbContext.Entity<MsDepartment>()
                            .Include(x => x.DepartmentLevels)
                                .ThenInclude(x => x.Level)
                                    .ThenInclude(x => x.Grades)
                            .Where(x => idDepartment.Contains(x.Id))
                            .Select(x => x)
                            .ToListAsync(CancellationToken);
                        var idDepartments = departments.Select(x => x.Id);
                        var subjectByDepartments = await _dbContext.Entity<MsSubject>()
                            .Include(x => x.Department)
                            .Where(x => idDepartments.Contains(x.IdDepartment))
                            .Select(x => new
                            {
                                x.Id,
                                x.IdGrade,
                                x.Grade.IdLevel
                            }
                            )
                            .ToListAsync(CancellationToken);
                        foreach (var department in departments)
                        {
                            if (department.Type == DepartmentType.Level)
                            {

                                foreach (var departmentLevel in department.DepartmentLevels)
                                {
                                    var gradePerLevel = subjectByDepartments.Where(x => x.IdLevel == departmentLevel.IdLevel);
                                    foreach (var grade in gradePerLevel)
                                    {
                                        IdGrade.Add(grade.IdGrade);
                                        IdSubject.Add(grade.Id);
                                    }
                                }
                            }
                            else
                            {
                                //var gradeByAcademicYearDepartment = gradeInAy.Where(x => x.idAcademicYear == department.IdAcademicYear).ToList();
                                foreach (var item in subjectByDepartments)
                                {
                                    IdGrade.Add(item.IdGrade);
                                    IdSubject.Add(item.Id);
                                }
                            }
                        }
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
            }
            else
            {
                if (param.SelectedPosition == PositionConstant.ClassAdvisor)
                {
                    var dataHomeroom = positionUser.ClassAdvisors.Select(x => x.Id).ToList();
                    if (positionUser.ClassAdvisors.Where(x => x.Id == PositionConstant.ClassAdvisor).FirstOrDefault() != null)
                        predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => dataHomeroom.Contains(x.Id) && x.HomeroomTeachers.Any(ht => ht.IdBinusian == param.IdUser));
                }

                if (param.SelectedPosition == PositionConstant.SubjectTeacher)
                {
                    var dataLesson = positionUser.SubjectTeachers.Select(x => x.Id).ToList();
                    if (positionUser.SubjectTeachers.Where(x => x.Id == PositionConstant.ClassAdvisor).FirstOrDefault() != null)
                    {
                        predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => dataLesson.Contains(x.IdLesson) && x.IdUser == param.IdUser);
                        predicateHomeroom = predicateHomeroom.And(x => x.HomeroomPathways.Any(y => y.LessonPathways.Any(z => dataLesson.Contains(z.IdLesson))));
                    }

                }
            }
            #endregion
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicateLevel = predicateLevel.And(x => x.Id == param.IdLevel);
            var query = from h in _dbContext.Entity<MsHomeroom>()
                        let hr = _dbContext
                                    .Entity<MsHomeroom>()
                                            .Include(x => x.HomeroomTeachers).ThenInclude(x => x.Staff)
                                            .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade)
                                            .Where(x => x.GradePathwayClassroom.GradePathway.Grade.IdLevel == param.IdLevel
                                                        && x.Semester == param.Semester
                                                        && (string.IsNullOrEmpty(param.IdGrade) || x.GradePathwayClassroom.GradePathway.IdGrade == param.IdGrade)
                                                        && (string.IsNullOrEmpty(param.IdHomeroom) || x.Id == param.IdHomeroom))
                                            .Where(predicateHomeroom)
                                            .ToList()
                        //let scheduleLesson = _dbContext.Entity<TrGeneratedScheduleLesson>()
                        //                    .Include(x => x.GeneratedScheduleStudent)
                        //                    .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                        //                    .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                        //                    .Include(x => x.Homeroom.Grade)
                        //                    .Where(x => x.IsGenerated)
                        //                    .Where(x => hr.Contains(x.IdHomeroom))
                        //                    .Where(predicateLesson)
                        //                    .Where(predicateStudentGrade)
                        //                    .ToList()
                        let level = _dbContext.Entity<MsLevel>()
                                        .Include(x => x.AcademicYear)
                                        .Where(predicateLevel)
                                        .FirstOrDefault()
                        select new
                        {
                            homeroom = hr
                            //,scheduleLesson = scheduleLesson
                            ,level = level
                        };
            var data = await query.FirstOrDefaultAsync(CancellationToken);
            var idHomerooms = data.homeroom.Select(x => x.Id).ToList();
            var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                            .Include(x => x.Homeroom.Grade)
                                            .Where(x => x.IsGenerated && idHomerooms.Contains(x.IdHomeroom))
                                            .Where(predicateLesson)
                                            .Where(predicateStudentGrade)
                                            .ToListAsync(CancellationToken);
            var idLevels = schedules.Select(x => x.Homeroom.Grade.IdLevel).Distinct().ToList();
            var result = new GetSummaryDetailResult<SummaryByLevelResult>
            {
                AcademicYear = new CodeWithIdVm
                {
                    Id = data.level.AcademicYear.Id,
                    Code = data.level.AcademicYear.Code,
                    Description = data.level.AcademicYear.Description
                },
                Level = new CodeWithIdVm
                {
                    Id = data.level.Id,
                    Code = data.level.Code,
                    Description = data.level.Description
                }
            };
            if (result is null)
            {
                var dataGrade = await _dbContext.Entity<MsGrade>().Where(x => x.Id == param.IdGrade).FirstOrDefaultAsync(CancellationToken);
                var dataUser = await _dbContext.Entity<MsStaff>().Where(x => x.IdBinusian == param.IdUser).FirstOrDefaultAsync(CancellationToken);
                throw new NotFoundException($"{dataUser.FirstName} {dataUser.LastName} has no assign to Grade {dataGrade.Description}");
            }


            var query2 = from ma in _dbContext.Entity<MsMappingAttendance>().Where(x => x.IdLevel == data.level.Id)
                         let sme = _dbContext.Entity<MsSchoolMappingEA>()
                                             .Where(x => x.IdSchool == data.level.AcademicYear.IdSchool)
                                             .First()
                         select new
                         {
                             MappingAttendance = ma
                             ,
                             SchoolMappingEA = sme
                         };
            var dataQuery2 = await query2.FirstOrDefaultAsync(CancellationToken);

            if (dataQuery2.MappingAttendance is null)
                throw new NotFoundException("mapping is not found for this level");


            if (dataQuery2.SchoolMappingEA is null)
                throw new NotFoundException("EA mapping is not found for this school");

            result.IsEAGrouped = dataQuery2.SchoolMappingEA.IsGrouped;
            result.IsUseDueToLateness = dataQuery2.MappingAttendance.IsUseDueToLateness;
            result.Term = dataQuery2.MappingAttendance.AbsentTerms;

            result.Data = schedules.GroupBy(x => new { x.IdHomeroom, x.HomeroomName })
                                   .Select(x => new SummaryByLevelResult
                                   {
                                       Grade = new CodeWithIdVm
                                       {
                                           Id = data.homeroom.First(y => y.Id == x.Key.IdHomeroom).GradePathwayClassroom.GradePathway.Grade.Id,
                                           Code = data.homeroom.First(y => y.Id == x.Key.IdHomeroom).GradePathwayClassroom.GradePathway.Grade.Code,
                                           Description = data.homeroom.First(y => y.Id == x.Key.IdHomeroom).GradePathwayClassroom.GradePathway.Grade.Description
                                       },
                                       Homeroom = new ItemValueVm
                                       {
                                           Id = x.Key.IdHomeroom,
                                           Description = x.Key.HomeroomName
                                       },
                                       Teacher = data.homeroom.First(y => y.Id == x.Key.IdHomeroom).HomeroomTeachers
                                                                                  .Where(y => y.IsAttendance)
                                                                                  .Select(y => new ItemValueVm
                                                                                  {
                                                                                      Id = y.IdBinusian,
                                                                                      Description = $"{y.Staff.FirstName} {y.Staff.LastName}"
                                                                                  }).FirstOrDefault(),
                                       Pending = x.Where(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Pending))
                                                  .GroupBy(y => new { y.ScheduleDate.Date , y.IdHomeroom , y.HomeroomName})
                                                  .Select(y => y.Key.Date)
                                                  .Count(),
                                       Unsubmitted = x.Where(y => !y.AttendanceEntries.Any()
                                                                  && y.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                                  .GroupBy(y => new { y.ScheduleDate.Date, y.IdHomeroom, y.HomeroomName })
                                                  .Select(y => y.Key.Date)
                                                  .Count()
                                   })
                                   .ToList();

            //get last updated data
            if (schedules.Any(x => x.AttendanceEntries.Any()))
            {
                var lastCreated = schedules.Max(x => x.AttendanceEntries.Max(y => y.DateIn));
                var lastUpdated = schedules.Max(x => x.AttendanceEntries.Max(y => y.DateUp));
                if (lastCreated.HasValue && lastUpdated.HasValue)
                    result.LastUpdated = lastCreated > lastUpdated ? lastCreated : lastUpdated;
                else if (lastCreated.HasValue)
                    result.LastUpdated = lastCreated;
                else if (lastUpdated.HasValue)
                    result.LastUpdated = lastUpdated;
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
