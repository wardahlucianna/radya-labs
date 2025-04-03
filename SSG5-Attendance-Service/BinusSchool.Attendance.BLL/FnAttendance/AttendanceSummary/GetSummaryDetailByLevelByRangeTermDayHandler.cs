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
using BinusSchool.Attendance.FnAttendance.Utils;
namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailByLevelByRangeTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetAvailabilityPositionByUserHandler _availabilityPositionByUser;
        public GetSummaryDetailByLevelByRangeTermDayHandler(
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
            var param = Request.ValidateParams<GetSummaryDetailByRangeRequest>(nameof(GetSummaryDetailByRangeRequest.IdLevel),
                                                                               nameof(GetSummaryDetailByRangeRequest.StartDate),
                                                                               nameof(GetSummaryDetailByRangeRequest.EndDate));
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
                    var res = CheckPositionUserUtil.PrincipalAndVicePrincipal(positionUser.OtherPositions);
                    if (res.Count > 0)
                    {
                        var _idLevels = JsonConvert.DeserializeObject<List<string>>(res["idLevel"].ToString());
                        predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => _idLevels.Contains(x.Grade.IdLevel));
                    }
                }
                if (param.SelectedPosition == PositionConstant.VicePrincipal)
                {
                    var res = CheckPositionUserUtil.PrincipalAndVicePrincipal(positionUser.OtherPositions);
                    if (res.Count > 0)
                    {
                        var _idLevels = JsonConvert.DeserializeObject<List<string>>(res["idLevel"].ToString());
                        predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => _idLevels.Contains(x.Grade.IdLevel));
                    }
                }
                if (param.SelectedPosition == PositionConstant.LevelHead)
                {
                    var res = CheckPositionUserUtil.LevelHead(positionUser.OtherPositions);
                    if (res.Count > 0)
                    {
                        var _idLevels = JsonConvert.DeserializeObject<List<string>>(res["idLevel"].ToString());
                        var _idGrades = JsonConvert.DeserializeObject<List<string>>(res["idGrade"].ToString());
                        predicateHomeroom = predicateHomeroom.And(x => _idGrades.Contains(x.IdGrade));
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => _idGrades.Contains(g.Id)));
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectHead)
                {
                    var res = CheckPositionUserUtil.SubjectHeadAndSubjectHeadAsisstant(positionUser.OtherPositions);
                    if (res.Count > 0)
                    {
                        var _idLevels = JsonConvert.DeserializeObject<List<string>>(res["idLevel"].ToString());
                        var _idGrades = JsonConvert.DeserializeObject<List<string>>(res["idGrade"].ToString());
                        var _idDepartments = JsonConvert.DeserializeObject<List<string>>(res["idDepartment"].ToString());
                        var _idSubjects = JsonConvert.DeserializeObject<List<string>>(res["idSubject"].ToString());
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => _idGrades.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => _idGrades.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => _idSubjects.Contains(x.IdSubject));
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
                {
                    var res = CheckPositionUserUtil.SubjectHeadAndSubjectHeadAsisstant(positionUser.OtherPositions);
                    if (res.Count > 0)
                    {
                        var _idLevels = JsonConvert.DeserializeObject<List<string>>(res["idLevel"].ToString());
                        var _idGrades = JsonConvert.DeserializeObject<List<string>>(res["idGrade"].ToString());
                        var _idDepartments = JsonConvert.DeserializeObject<List<string>>(res["idDepartment"].ToString());
                        var _idSubjects = JsonConvert.DeserializeObject<List<string>>(res["idSubject"].ToString());
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => _idGrades.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => _idGrades.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => _idSubjects.Contains(x.IdSubject));
                    }
                }
                if (param.SelectedPosition == PositionConstant.HeadOfDepartment)
                {

                    var res = CheckPositionUserUtil.HeadOfDepartment(positionUser.OtherPositions);
                    if (res.Count > 0)
                    {
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        var _idDepartments = JsonConvert.DeserializeObject<List<string>>(res["idDepartment"].ToString());
                        var departments = await _dbContext.Entity<MsDepartment>()
                           .Include(x => x.DepartmentLevels)
                               .ThenInclude(x => x.Level)
                                   .ThenInclude(x => x.Grades)
                           .Include(x => x.Subjects)
                           .Where(x => _idDepartments.Contains(x.Id))
                           .Select(x => x)
                           .ToListAsync(CancellationToken);
                        var subjectByDepartments = departments.Where(x => x.Subjects.Any(y => _idDepartments.Contains(y.IdDepartment))).SelectMany(x => x.Subjects).ToList();
                        foreach (var department in departments)
                        {
                            if (department.Type == DepartmentType.Level)
                            {

                                foreach (var departmentLevel in department.DepartmentLevels)
                                {
                                    var gradePerLevel = subjectByDepartments.Where(x => x.Grade.IdLevel == departmentLevel.IdLevel);
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
                            ,
                            level = level
                        };
            var data = await query.FirstOrDefaultAsync(CancellationToken);
            var idHomerooms = data.homeroom.Select(x => x.Id).ToList();

            var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                            .Include(x => x.Homeroom.Grade)
                                            .Where(x => x.IsGenerated
                                                        && x.ScheduleDate.Date >= param.StartDate
                                                        && x.ScheduleDate.Date <= param.EndDate
                                                        && idHomerooms.Contains(x.IdHomeroom))
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
                throw new NotFoundException($"{dataUser.FirstName} {dataUser.LastName} has no assign to {dataGrade.Description}");
            }
            var query2 = from ma in _dbContext.Entity<MsMappingAttendance>().Where(x => x.IdLevel == data.level.Id)
                         let sme = _dbContext.Entity<MsSchoolMappingEA>()
                                             .Where(x => x.IdSchool == data.level.AcademicYear.IdSchool)
                                             .First()
                         select new
                         {
                             MappingAttendance = ma
                             ,SchoolMappingEA = sme
                         };
            var dataQuery2 = await query2.FirstOrDefaultAsync(CancellationToken);

            if (dataQuery2.MappingAttendance is null)
                throw new NotFoundException("mapping is not found for this level");
            if (dataQuery2.SchoolMappingEA is null)
                throw new NotFoundException("EA mapping is not found for this school");

            result.IsEAGrouped = dataQuery2.SchoolMappingEA.IsGrouped;
            result.IsUseDueToLateness = dataQuery2.MappingAttendance.IsUseDueToLateness;
            result.Term = dataQuery2.MappingAttendance.AbsentTerms;
            result.Data = schedules.GroupBy(x => new {  x.IdHomeroom, x.HomeroomName })
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
                                                  .GroupBy(y => new { y.ScheduleDate.Date, y.IdHomeroom, y.HomeroomName })
                                                  .Count(),
                                       Unsubmitted = x.Where(y => !y.AttendanceEntries.Any()
                                                                  && y.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                                      .GroupBy(y => new { y.ScheduleDate.Date, y.IdHomeroom, y.HomeroomName })
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
