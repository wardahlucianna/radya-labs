using System;
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
    public class GetSummaryDetailBySubjectByRangeHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetSummaryDetailByRangeRequest.IdAcademicYear),
            nameof(GetSummaryDetailByRangeRequest.IdLevel),
            nameof(GetSummaryDetailByRangeRequest.StartDate),
            nameof(GetSummaryDetailByRangeRequest.EndDate)
        });

        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetAvailabilityPositionByUserHandler _positionByUserHandler;
        public GetSummaryDetailBySubjectByRangeHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime, GetAvailabilityPositionByUserHandler positionByUserHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _positionByUserHandler = positionByUserHandler;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSummaryDetailByRangeRequest>(_requiredParams.Value);

            #region Get Position User

            var positions = await _positionByUserHandler.GetAvailablePositionDetail(param.IdUser ?? AuthInfo.UserId, param.IdAcademicYear);
            // if (positions.OtherPositions.Count == 0 && positions.ClassAdvisors.Count == 0 && positions.SubjectTeachers.Count == 0)
            //     throw new BadRequestException("You dont have any position.");
            // if (positions.OtherPositions.All(x => x.Id != param.SelectedPosition) && positions.ClassAdvisors.Count == 0 && positions.SubjectTeachers.Count == 0)
            //     throw new BadRequestException($"You dont assign as {param.SelectedPosition}.");

            var predicateLevel = PredicateBuilder.True<MsLevel>();
            var predicateLevelPrincipalAndVicePrincipal = PredicateBuilder.True<MsHomeroom>();
            var predicateHomeroom = PredicateBuilder.True<MsHomeroom>();
            var predicateLesson = PredicateBuilder.True<TrGeneratedScheduleLesson>();
            var predicateStudentGrade = PredicateBuilder.True<TrGeneratedScheduleLesson>();
            var idLevelPrincipalAndVicePrincipal = new List<string>();

            // get selected other positions other than Class Advisor & Subject Teacher
            var selectedOtherPositions = Array.Empty<OtherPositionResult>();
            if (param.SelectedPosition != PositionConstant.ClassAdvisor && param.SelectedPosition != PositionConstant.SubjectTeacher)
                selectedOtherPositions = positions.OtherPositions.Where(x => x.Id == param.SelectedPosition).ToArray();

            if (selectedOtherPositions.Length != 0)
            {
                if (param.SelectedPosition == PositionConstant.Principal)
                {
                    foreach (var item in selectedOtherPositions)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
                    }
                    predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => idLevelPrincipalAndVicePrincipal.Contains(x.Grade.IdLevel));
                }
                else if (param.SelectedPosition == PositionConstant.VicePrincipal)
                {
                    List<string> IdLevels = new List<string>();
                    foreach (var item in selectedOtherPositions)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        IdLevels.Add(_levelLH.Id);
                    }
                    predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => IdLevels.Contains(x.Grade.IdLevel));
                }
                else if (param.SelectedPosition == PositionConstant.LevelHead)
                {
                    List<string> IdGrade = new List<string>();
                    foreach (var item in selectedOtherPositions)
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
                else if (param.SelectedPosition == PositionConstant.SubjectHead)
                {
                    List<string> IdGrade = new List<string>();
                    List<string> IdSubject = new List<string>();
                    foreach (var item in selectedOtherPositions)
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
                else if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
                {
                    List<string> IdGrade = new List<string>();
                    List<string> IdSubject = new List<string>();
                    foreach (var item in selectedOtherPositions)
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
                else if (param.SelectedPosition == PositionConstant.HeadOfDepartment)
                {
                    List<string> idDepartment = new List<string>();
                    List<string> IdGrade = new List<string>();
                    List<string> IdSubject = new List<string>();
                    foreach (var item in selectedOtherPositions)
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
                    //var IdAcademicYear = departments.Select(x => x.IdAcademicYear).Distinct().ToList();
                    //var gradeInAy = await _dbContext.Entity<MsGrade>()
                    //           .Include(x => x.MsLevel)
                    //           .Where(x => IdAcademicYear.Contains(x.MsLevel.IdAcademicYear))
                    //           .Select(x => new
                    //           {
                    //               idGrade = x.Id,
                    //               IdLevel = x.MsLevel.Id,
                    //               idAcademicYear = x.MsLevel.IdAcademicYear
                    //           })
                    //           .ToListAsync(CancellationToken);
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

            if (param.SelectedPosition == PositionConstant.ClassAdvisor && positions.ClassAdvisors.Count != 0)
            {
                var idHomerooms = positions.ClassAdvisors.Select(x => x.Id);
                predicateHomeroom = predicateHomeroom.And(x
                    => idHomerooms.Contains(x.Id)
                    && x.HomeroomTeachers.Any(ht => ht.IdBinusian == param.IdUser));
            }

            else if (param.SelectedPosition == PositionConstant.SubjectTeacher && positions.SubjectTeachers.Count != 0)
            {
                predicateLesson = predicateLesson.And(x => x.IdUser == param.IdUser);

                var idLessons = positions.SubjectTeachers.Select(x => x.Id).ToArray();
                if (idLessons.Length != 0)
                {
                    predicateLesson = predicateLesson.And(x => idLessons.Contains(x.IdLesson));
                    predicateHomeroom = predicateHomeroom.And(x => x.HomeroomPathways.Any(y => y.LessonPathways.Any(z => idLessons.Contains(z.IdLesson))));
                }
            }

            #endregion

            var query1 =
                from level in _dbContext.Entity<MsLevel>()
                    .Where(predicateLevel)
                    .Where(x => x.Id == param.IdLevel)
                    .Select(x => new GetSummaryDetailResult<SummaryBySubjectResult>
                    {
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.AcademicYear.Id,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        Level = new CodeWithIdVm
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description
                        }
                    })
                let idHomerooms =
                    _dbContext.Entity<MsHomeroom>()
                        .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear
                                    && x.Grade.IdLevel == param.IdLevel
                                    // && x.Semester == param.Semester
                                    && (string.IsNullOrEmpty(param.IdGrade) || x.IdGrade == param.IdGrade)
                                    && (string.IsNullOrEmpty(param.IdHomeroom) || x.Id == param.IdHomeroom))
                        .Where(predicateHomeroom)
                        .Select(x => x.Id)
                        .ToList()
                select new
                {
                    level,
                    idHomerooms
                };
            var query1Result = await query1.FirstOrDefaultAsync(CancellationToken);

            var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                            .Where(x => x.IsGenerated
                                                        && x.ScheduleDate.Date >= param.StartDate
                                                        && x.ScheduleDate.Date <= param.EndDate
                                                        && query1Result.idHomerooms.Contains(x.IdHomeroom))
                                            .Where(predicateLesson)
                                            .Where(predicateStudentGrade)
                                            .ToListAsync(CancellationToken);

            var result = query1Result.level;
            if (result is null)
            {
                var query2 =
                    from grade in _dbContext.Entity<MsGrade>()
                    let staff =
                        (from stf in _dbContext.Entity<MsStaff>()
                         where stf.IdBinusian == param.IdUser
                         select new { stf.FirstName, stf.LastName })
                        .FirstOrDefault()
                    where grade.Id == param.IdGrade
                    select new
                    {
                        grade.Description,
                        staff.FirstName,
                        staff.LastName
                    };
                var query2Result = await query2.FirstOrDefaultAsync(CancellationToken);

                throw new NotFoundException($"{query2Result.FirstName} {query2Result.LastName} has no assign to Grade {query2Result.Description}");
            }

            var query3 =
                from mappingAttendance in _dbContext.Entity<MsMappingAttendance>()
                let mappingEa =
                    (from mea in _dbContext.Entity<MsSchoolMappingEA>()
                     join sch in _dbContext.Entity<MsSchool>() on mea.IdSchool equals sch.Id
                     join ay in _dbContext.Entity<MsAcademicYear>() on sch.Id equals ay.IdSchool
                     join lvl in _dbContext.Entity<MsLevel>() on ay.Id equals lvl.IdAcademicYear
                     where lvl.Id == param.IdLevel
                     select new { mea.IsGrouped })
                    .FirstOrDefault()
                where mappingAttendance.IdLevel == query1Result.level.Level.Id
                select new
                {
                    mappingAttendance,
                    mappingEa
                };
            var query3Result = await query3.FirstOrDefaultAsync(CancellationToken);

            var mapping = query3Result.mappingAttendance;
            if (mapping is null)
                throw new NotFoundException("mapping is not found for this level");
            if (query3Result.mappingEa is null)
                throw new NotFoundException("EA mapping is not found for this school");

            result.IsEAGrouped = query3Result.mappingEa.IsGrouped;
            result.IsUseDueToLateness = mapping.IsUseDueToLateness;
            result.Term = mapping.AbsentTerms;

            var datas = schedules.GroupBy(x => new { x.IdHomeroom, x.HomeroomName, x.ClassID, x.IdSubject, x.SubjectName });

            var results = new List<SummaryBySubjectResult>();

            foreach (var item in datas)
            {
                var idUserGroup = item.Select(x => x.IdUser).Distinct().ToList();

                var itemData = new SummaryBySubjectResult()
                {
                    Homeroom = new ItemValueVm
                    {
                        Id = item.Key.IdHomeroom,
                        Description = item.Key.HomeroomName
                    },
                    ClassId = item.Key.ClassID,
                    Subject = new ItemValueVm
                    {
                        Id = item.Key.IdSubject,
                        Description = item.Key.SubjectName
                    },
                    Teacher = new ItemValueVm
                    {
                        Id = string.Join(",", item.Select(x => x.IdUser).Distinct()),
                        Description = string.Join(",", item.Select(x => x.TeacherName).Distinct())
                    },
                    HasSession = mapping.AbsentTerms == AbsentTerm.Session,
                    Pending = mapping.AbsentTerms == AbsentTerm.Day ?
                                                 item.Where(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Pending))
                                                  .GroupBy(y => new { y.ScheduleDate.Date, y.ClassID, y.IdSubject, y.SubjectName, y.IdHomeroom, y.HomeroomName })
                                                  .Select(y => new UnresolvedAttendanceGroupResult
                                                  {
                                                      Date = y.Key.Date,
                                                      ClassID = y.Key.ClassID,
                                                      Teacher = new ItemValueVm
                                                      {
                                                          Id = string.Join(",", y.Select(y => y.IdUser).Distinct()),
                                                          Description = string.Join(",", y.Select(y => y.TeacherName).Distinct())
                                                      },
                                                      Subject = new ItemValueVm
                                                      {
                                                          Id = y.Key.IdSubject,
                                                          Description = y.Key.SubjectName
                                                      },
                                                      Homeroom = new ItemValueVm
                                                      {
                                                          Id = y.Key.IdHomeroom,
                                                          Description = y.Key.HomeroomName
                                                      },
                                                      Level = result.Level,
                                                      TotalStudent = y.Where(z => z.IdUser == idUserGroup.FirstOrDefault()).Count()
                                                  })
                                                  .OrderByDescending(x => x.Date)
                                                  .ToList() :
                                                 item.Where(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Pending))
                                                  .GroupBy(y => new { y.ScheduleDate.Date, y.ClassID, y.IdSubject, y.SubjectName, y.IdHomeroom, y.HomeroomName, y.IdSession, y.SessionID })
                                                  .Select(y => new UnresolvedAttendanceGroupResult
                                                  {
                                                      Date = y.Key.Date,
                                                      ClassID = y.Key.ClassID,
                                                      Teacher = new ItemValueVm
                                                      {
                                                          Id = string.Join(",", y.Select(y => y.IdUser).Distinct()),
                                                          Description = string.Join(",", y.Select(y => y.TeacherName).Distinct())
                                                      },
                                                      Subject = new ItemValueVm
                                                      {
                                                          Id = y.Key.IdSubject,
                                                          Description = y.Key.SubjectName
                                                      },
                                                      Homeroom = new ItemValueVm
                                                      {
                                                          Id = y.Key.IdHomeroom,
                                                          Description = y.Key.HomeroomName
                                                      },
                                                      Session = new ItemValueVm
                                                      {
                                                          Id = y.Key.IdSession,
                                                          Description = y.Key.SessionID
                                                      },
                                                      Level = result.Level,
                                                      TotalStudent = y.Where(z => z.IdUser == idUserGroup.FirstOrDefault()).Count()
                                                  })
                                                  .OrderByDescending(x => x.Date)
                                                  .ToList(),
                    Unsubmitted = mapping.AbsentTerms == AbsentTerm.Day ?
                                                     item.Where(y => !y.AttendanceEntries.Any()
                                                                  && y.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                                      .GroupBy(y => new { y.ScheduleDate.Date, y.ClassID, y.IdSubject, y.SubjectName, y.IdHomeroom, y.HomeroomName })
                                                      .Select(y => new UnresolvedAttendanceGroupResult
                                                      {
                                                          Date = y.Key.Date,
                                                          ClassID = y.Key.ClassID,
                                                          Teacher = new ItemValueVm
                                                          {
                                                              Id = string.Join(",", y.Select(y => y.IdUser).Distinct()),
                                                              Description = string.Join(",", y.Select(y => y.TeacherName).Distinct())
                                                          },
                                                          Subject = new ItemValueVm
                                                          {
                                                              Id = y.Key.IdSubject,
                                                              Description = y.Key.SubjectName
                                                          },
                                                          Homeroom = new ItemValueVm
                                                          {
                                                              Id = y.Key.IdHomeroom,
                                                              Description = y.Key.HomeroomName
                                                          },
                                                          Level = result.Level,
                                                          TotalStudent = y.Where(z => z.IdUser == idUserGroup.FirstOrDefault()).Count()
                                                      })
                                                      .OrderByDescending(x => x.Date)
                                                      .ToList() :
                                                     item.Where(y => !y.AttendanceEntries.Any()
                                                                  && y.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                                      .GroupBy(y => new { y.ScheduleDate.Date, y.ClassID, y.IdSubject, y.SubjectName, y.IdHomeroom, y.HomeroomName, y.IdSession, y.SessionID })
                                                      .Select(y => new UnresolvedAttendanceGroupResult
                                                      {
                                                          Date = y.Key.Date,
                                                          ClassID = y.Key.ClassID,
                                                          Teacher = new ItemValueVm
                                                          {
                                                              Id = string.Join(",", y.Select(y => y.IdUser).Distinct()),
                                                              Description = string.Join(",", y.Select(y => y.TeacherName).Distinct())
                                                          },
                                                          Subject = new ItemValueVm
                                                          {
                                                              Id = y.Key.IdSubject,
                                                              Description = y.Key.SubjectName
                                                          },
                                                          Homeroom = new ItemValueVm
                                                          {
                                                              Id = y.Key.IdHomeroom,
                                                              Description = y.Key.HomeroomName
                                                          },
                                                          Session = new ItemValueVm
                                                          {
                                                              Id = y.Key.IdSession,
                                                              Description = y.Key.SessionID
                                                          },
                                                          Level = result.Level,
                                                          TotalStudent = y.Where(z => z.IdUser == idUserGroup.FirstOrDefault()).Count()
                                                      })
                                                      .OrderByDescending(x => x.Date)
                                                      .ToList()
                };
                results.Add(itemData);
            }

            result.Data = results;

            //result.Data = schedules.GroupBy(x => new { x.IdHomeroom, x.HomeroomName, x.ClassID, x.IdSubject, x.SubjectName })
            //                       .Select(x => new SummaryBySubjectResult
            //                       {
            //                           Homeroom = new ItemValueVm
            //                           {
            //                               Id = x.Key.IdHomeroom,
            //                               Description = x.Key.HomeroomName
            //                           },
            //                           ClassId = x.Key.ClassID,
            //                           Subject = new ItemValueVm
            //                           {
            //                               Id = x.Key.IdSubject,
            //                               Description = x.Key.SubjectName
            //                           },
            //                           Teacher = new ItemValueVm
            //                           {
            //                               Id = string.Join(",", x.Select(x => x.IdUser).Distinct()),
            //                               Description = string.Join(",", x.Select(x => x.TeacherName).Distinct())
            //                           },
            //                           HasSession = mapping.AbsentTerms == AbsentTerm.Session,
            //                           Pending = mapping.AbsentTerms == AbsentTerm.Day ?
            //                                     x.Where(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Pending))
            //                                      .GroupBy(y => new { y.ScheduleDate.Date, y.ClassID, y.IdUser, y.TeacherName, y.IdSubject, y.SubjectName, y.IdHomeroom, y.HomeroomName })
            //                                      .Select(y => new UnresolvedAttendanceGroupResult
            //                                      {
            //                                          Date = y.Key.Date,
            //                                          ClassID = y.Key.ClassID,
            //                                          Teacher = new ItemValueVm
            //                                          {
            //                                              Id = y.Key.IdUser,
            //                                              Description = y.Key.TeacherName
            //                                          },
            //                                          Subject = new ItemValueVm
            //                                          {
            //                                              Id = y.Key.IdSubject,
            //                                              Description = y.Key.SubjectName
            //                                          },
            //                                          Homeroom = new ItemValueVm
            //                                          {
            //                                              Id = y.Key.IdHomeroom,
            //                                              Description = y.Key.HomeroomName
            //                                          },
            //                                          Level = result.Level,
            //                                          TotalStudent = y.Count()
            //                                      })
            //                                      .OrderByDescending(x => x.Date)
            //                                      .ToList() :
            //                                     x.Where(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Pending))
            //                                      .GroupBy(y => new { y.ScheduleDate.Date, y.ClassID, y.IdUser, y.TeacherName, y.IdSubject, y.SubjectName, y.IdHomeroom, y.HomeroomName, y.IdSession, y.SessionID })
            //                                      .Select(y => new UnresolvedAttendanceGroupResult
            //                                      {
            //                                          Date = y.Key.Date,
            //                                          ClassID = y.Key.ClassID,
            //                                          Teacher = new ItemValueVm
            //                                          {
            //                                              Id = y.Key.IdUser,
            //                                              Description = y.Key.TeacherName
            //                                          },
            //                                          Subject = new ItemValueVm
            //                                          {
            //                                              Id = y.Key.IdSubject,
            //                                              Description = y.Key.SubjectName
            //                                          },
            //                                          Homeroom = new ItemValueVm
            //                                          {
            //                                              Id = y.Key.IdHomeroom,
            //                                              Description = y.Key.HomeroomName
            //                                          },
            //                                          Session = new ItemValueVm
            //                                          {
            //                                              Id = y.Key.IdSession,
            //                                              Description = y.Key.SessionID
            //                                          },
            //                                          Level = result.Level,
            //                                          TotalStudent = y.Count()
            //                                      })
            //                                      .OrderByDescending(x => x.Date)
            //                                      .ToList(),
            //                           Unsubmitted = mapping.AbsentTerms == AbsentTerm.Day ?
            //                                         x.Where(y => !y.AttendanceEntries.Any()
            //                                                      && y.ScheduleDate.Date <= _dateTime.ServerTime.Date)
            //                                          .GroupBy(y => new { y.ScheduleDate.Date, y.ClassID, y.IdUser, y.TeacherName, y.IdSubject, y.SubjectName, y.IdHomeroom, y.HomeroomName })
            //                                          .Select(y => new UnresolvedAttendanceGroupResult
            //                                          {
            //                                              Date = y.Key.Date,
            //                                              ClassID = y.Key.ClassID,
            //                                              Teacher = new ItemValueVm
            //                                              {
            //                                                  Id = y.Key.IdUser,
            //                                                  Description = y.Key.TeacherName
            //                                              },
            //                                              Subject = new ItemValueVm
            //                                              {
            //                                                  Id = y.Key.IdSubject,
            //                                                  Description = y.Key.SubjectName
            //                                              },
            //                                              Homeroom = new ItemValueVm
            //                                              {
            //                                                  Id = y.Key.IdHomeroom,
            //                                                  Description = y.Key.HomeroomName
            //                                              },
            //                                              Level = result.Level,
            //                                              TotalStudent = y.Count()
            //                                          })
            //                                          .OrderByDescending(x => x.Date)
            //                                          .ToList() :
            //                                         x.Where(y => !y.AttendanceEntries.Any()
            //                                                      && y.ScheduleDate.Date <= _dateTime.ServerTime.Date)
            //                                          .GroupBy(y => new { y.ScheduleDate.Date, y.ClassID, y.IdUser, y.TeacherName, y.IdSubject, y.SubjectName, y.IdHomeroom, y.HomeroomName, y.IdSession, y.SessionID })
            //                                          .Select(y => new UnresolvedAttendanceGroupResult
            //                                          {
            //                                              Date = y.Key.Date,
            //                                              ClassID = y.Key.ClassID,
            //                                              Teacher = new ItemValueVm
            //                                              {
            //                                                  Id = y.Key.IdUser,
            //                                                  Description = y.Key.TeacherName
            //                                              },
            //                                              Subject = new ItemValueVm
            //                                              {
            //                                                  Id = y.Key.IdSubject,
            //                                                  Description = y.Key.SubjectName
            //                                              },
            //                                              Homeroom = new ItemValueVm
            //                                              {
            //                                                  Id = y.Key.IdHomeroom,
            //                                                  Description = y.Key.HomeroomName
            //                                              },
            //                                              Session = new ItemValueVm
            //                                              {
            //                                                  Id = y.Key.IdSession,
            //                                                  Description = y.Key.SessionID
            //                                              },
            //                                              Level = result.Level,
            //                                              TotalStudent = y.Count()
            //                                          })
            //                                          .OrderByDescending(x => x.Date)
            //                                          .ToList()
            //                       })
            //                       .ToList();

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
