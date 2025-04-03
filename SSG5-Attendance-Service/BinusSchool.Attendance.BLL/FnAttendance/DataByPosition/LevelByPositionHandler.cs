using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.LevelByPosition;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.LevelByPosition
{
  public class LevelByPositionHandler : FunctionsHttpSingleHandler
  {
    private readonly IAttendanceDbContext _dbContext;

    public LevelByPositionHandler(IAttendanceDbContext dbContext)
    {
      _dbContext = dbContext;
    }
    protected override async Task<ApiErrorResult<object>> Handler()
    {
      List<LevelByPositionResult> listUserPosition = new List<LevelByPositionResult>();

      var param = Request.ValidateParams<LevelByPositionRequest>(nameof(LevelByPositionRequest.IdAcademicYear),
                                                                  nameof(LevelByPositionRequest.IdUser),
                                                                  nameof(LevelByPositionRequest.SelectedPosition));

            var gatAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => e.Id == param.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);

      var listGrade = await _dbContext.Entity<MsGrade>()
                          .Include(e => e.Level)
                          .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
                          .Select(e => new
                          {
                            IdLevel = e.IdLevel,
                            IdGrade = e.Id,
                            Level = e.Level.Description
                          })
                          .ToListAsync(CancellationToken);

            if (param.SelectedPosition == "All")
            {
                #region Staff
                var listLevelByStaff = listGrade
                    .GroupBy(e => new
                    {
                        e.IdLevel,
                        e.Level
                    })
                    .Select(e => new LevelByPositionResult
                    {
                        IdUser = param.IdUser,
                        Posistion = param.SelectedPosition,
                        Level = new ItemValueVm
                        {
                            Id = e.Key.IdLevel,
                            Description = e.Key.Level
                        }
                    })
                    .ToList();
                #endregion
            }
            else
            {
                #region NonTeachLoad
                var queryTeacherNonTeaching = _dbContext.Entity<TrNonTeachingLoad>()
                                    .Include(e => e.NonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                                     .Where(x => x.NonTeachingLoad.IdAcademicYear == param.IdAcademicYear);

        if (!string.IsNullOrEmpty(param.IdUser))
          queryTeacherNonTeaching = queryTeacherNonTeaching.Where(e => e.IdUser == param.IdUser);

        if (!string.IsNullOrEmpty(param.SelectedPosition))
          queryTeacherNonTeaching = queryTeacherNonTeaching.Where(e => e.NonTeachingLoad.TeacherPosition.LtPosition.Code == param.SelectedPosition);

        var listTeacherNonTeaching = await queryTeacherNonTeaching.ToListAsync(CancellationToken);

        var listLesson = await _dbContext.Entity<MsLesson>()
                            .Include(e => e.Grade).ThenInclude(e => e.Level)
                            .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                            .Select(e => new
                            {
                              IdLesson = e.Id,
                              IdSubject = e.IdSubject,
                              IdLevel = e.Grade.IdLevel,
                              Level = e.Grade.Level.Description
                            })
                            .ToListAsync(CancellationToken);



        var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                            .Include(e => e.Level).ThenInclude(e => e.Grades)
                             .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
                             .ToListAsync(CancellationToken);

        foreach (var item in listTeacherNonTeaching)
        {
          var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
          _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
          _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
          _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
          _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
          if (_SubjectPosition == null && _GradePosition == null && _LevelPosition == null && _DepartemenPosition != null)
          {
            var getDepartmentLevelbyIdLevel = listDepartmentLevel.Where(e => e.IdDepartment == _DepartemenPosition.Id).ToList();

            foreach (var itemDepartement in getDepartmentLevelbyIdLevel)
            {
              var listLevel = itemDepartement.Level;

              LevelByPositionResult newRespondent = new LevelByPositionResult
              {
                IdUser = item.IdUser,
                Posistion = param.SelectedPosition,
                Level = new ItemValueVm
                {
                  Id = listLevel.Id,
                  Description = listLevel.Description,
                },
              };
              listUserPosition.Add(newRespondent);
            }
          }
          else if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null && _DepartemenPosition != null)
          {
            var listLessonByIdSubject = listLesson.Where(e => e.IdSubject == _SubjectPosition.Id).ToList();

            foreach (var itemSubject in listLessonByIdSubject)
            {
              LevelByPositionResult newRespondent = new LevelByPositionResult
              {
                IdUser = item.IdUser,
                Posistion = param.SelectedPosition,
                Level = new ItemValueVm
                {
                  Id = itemSubject.IdLevel,
                  Description = itemSubject.Level,
                }
              };
              listUserPosition.Add(newRespondent);
            }
          }
          else if (_SubjectPosition == null && _GradePosition != null && _LevelPosition != null)
          {
            var listGradeById = listGrade.Where(e => e.IdGrade == _GradePosition.Id).ToList();

            foreach (var itemGrade in listGradeById)
            {
              LevelByPositionResult newRespondent = new LevelByPositionResult
              {
                IdUser = item.IdUser,
                Posistion = param.SelectedPosition,
                Level = new ItemValueVm
                {
                  Id = itemGrade.IdLevel,
                  Description = itemGrade.Level
                }
              };
              listUserPosition.Add(newRespondent);
            }
          }
          else if (_SubjectPosition == null && _GradePosition == null && _LevelPosition != null)
          {
            var listGradeByIdLevel = listGrade.Where(e => e.IdLevel == _LevelPosition.Id).ToList();

            foreach (var itemGrade in listGradeByIdLevel)
            {
              LevelByPositionResult newRespondent = new LevelByPositionResult
              {
                IdUser = item.IdUser,
                Posistion = param.SelectedPosition,
                Level = new ItemValueVm
                {
                  Id = itemGrade.IdLevel,
                  Description = itemGrade.Level
                }
              };
              listUserPosition.Add(newRespondent);
            }
          }
        }
        #endregion

                #region HomeroomTeacher/Co Teacher
                var queryHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                            .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);

        if (!string.IsNullOrEmpty(param.IdUser))
          queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.IdBinusian == param.IdUser);

        if (!string.IsNullOrEmpty(param.SelectedPosition))
          queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.TeacherPosition.LtPosition.Code == PositionConstant.ClassAdvisor || 
          e.TeacherPosition.LtPosition.Code == PositionConstant.CoTeacher);

        var listHomeroomTeacher = await queryHomeroomTeacher.ToListAsync(CancellationToken);

                foreach (var itemHomeroomTeacher in listHomeroomTeacher)
                {
                    LevelByPositionResult newRespondent = new LevelByPositionResult
                    {
                        IdUser = itemHomeroomTeacher.IdBinusian,
                        Posistion = param.SelectedPosition,
                        Level = new ItemValueVm
                        {
                            Id = itemHomeroomTeacher.Homeroom.Grade.Level.Id,
                            Description = itemHomeroomTeacher.Homeroom.Grade.Level.Description,
                        }
                    };
                    listUserPosition.Add(newRespondent);
                }

                #endregion

        #region LessonTeacher
        var queryTeacherPosition = _dbContext.Entity<MsTeacherPosition>()
                                   .Include(e => e.LtPosition)
                                   .Where(x => x.IdSchool == gatAcademicYear.IdSchool && x.LtPosition.Code == PositionConstant.SubjectTeacher);

        if (!string.IsNullOrEmpty(param.SelectedPosition))
          queryTeacherPosition = queryTeacherPosition.Where(e => e.LtPosition.Code == param.SelectedPosition);

        var listTeacherPosition = await queryTeacherPosition
                                  .Select(e => new
                                  {
                                    Id = e.Id,
                                    PositionCode = e.LtPosition.Code,
                                  })
                                  .ToListAsync(CancellationToken);

        var queryLessonBySt = _dbContext.Entity<MsLessonTeacher>()
                                .Include(e => e.Lesson).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                               .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);

        if (!string.IsNullOrEmpty(param.IdUser))
          queryLessonBySt = queryLessonBySt.Where(e => e.IdUser == param.IdUser);


        if (listTeacherPosition.Any())
        {
          var listLessonBySt = await queryLessonBySt
                             .Select(e => new LevelByPositionResult
                             {
                               IdUser = e.IdUser,
                               Posistion = param.SelectedPosition,
                               Level = new ItemValueVm
                               {
                                 Id = e.Lesson.Grade.Level.Id,
                                 Description = e.Lesson.Grade.Level.Description
                               }
                             })
                             .Distinct()
                             .ToListAsync(CancellationToken);

          listUserPosition.AddRange(listLessonBySt);
        }
        #endregion
      }

            var listLevels = listUserPosition
                                .Where(e => e.Posistion == param.SelectedPosition)
                                .GroupBy(e => new
                                {
                                    idLevel = e.Level.Id,
                                    Level = e.Level.Description
                                })
                                .Select(e => new ItemValueVm
                                {
                                    Id = e.Key.idLevel,
                                    Description = e.Key.Level,
                                })
                                .ToList();

      return Request.CreateApiResult2(listLevels as object);
    }

    //protected override async Task<ApiErrorResult<object>> Handler()
    //{
    //    #region Get Position User
    //    var param = Request.ValidateParams<LevelByPositionRequest>(nameof(LevelByPositionRequest.IdAcademicYear),
    //                                                                 nameof(LevelByPositionRequest.IdUser),
    //                                                                 nameof(LevelByPositionRequest.SelectedPosition));
    //    List<string> avaiablePosition = new List<string>();
    //    var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
    //    .Include(x => x.Homeroom)
    //        .ThenInclude(x => x.Grade)
    //            .ThenInclude(x => x.Level)
    //    .Where(x => x.IdBinusian == param.IdUser)
    //    .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear)
    //    .Distinct()
    //    .OrderBy(x => x.Homeroom.Grade.OrderNumber)
    //    .Select(x => x).ToListAsync(CancellationToken);

    //    var dataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
    //    .Include(x => x.Lesson)
    //        .ThenInclude(x => x.Grade)
    //            .ThenInclude(x => x.Level)
    //    .Where(x => x.IdUser == param.IdUser)
    //    .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear)
    //    .Distinct()
    //    .OrderBy(x => x.Lesson.Grade.Level.OrderNumber)
    //    .Select(x => x).ToListAsync(CancellationToken);
    //    var positionUser = await _dbContext.Entity<TrNonTeachingLoad>().Include(x => x.NonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.LtPosition)
    //        .Where(x => x.NonTeachingLoad.IdAcademicYear == param.IdAcademicYear)
    //        .Where(x => x.IdUser == param.IdUser)
    //        .Select(x => new
    //        {
    //            x.Data,
    //            x.NonTeachingLoad.TeacherPosition.LtPosition.Code
    //        }).ToListAsync(CancellationToken);
    //    if (positionUser.Count == 0 && dataHomeroomTeacher.Count == 0 && dataLessonTeacher.Count == 0)
    //        throw new BadRequestException($"You dont have any position.");
    //    if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
    //        avaiablePosition.Add(PositionConstant.ClassAdvisor);
    //    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
    //        avaiablePosition.Add(PositionConstant.SubjectTeacher);
    //    foreach (var pu in positionUser)
    //    {
    //        avaiablePosition.Add(pu.Code);
    //    }
    //    if (avaiablePosition.Where(x => x == param.SelectedPosition).Count() == 0)
    //        throw new BadRequestException($"You dont assign as {param.SelectedPosition}.");
    //    List<string> idLevelPrincipalAndVicePrincipal = new List<string>();
    //    List<ItemValueVm> idLevels = new List<ItemValueVm>();

    //    if (positionUser.Count > 0)
    //    {
    //        if (param.SelectedPosition == PositionConstant.Principal)
    //        {
    //            if (positionUser.Any(y => y.Code == PositionConstant.Principal)) //check P Or VP
    //            {
    //                if (positionUser.Where(y => y.Code == PositionConstant.Principal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.Principal).Count() > 0)
    //                {
    //                    var Principal = positionUser.Where(x => x.Code == PositionConstant.Principal).ToList();

    //                    foreach (var item in Principal)
    //                    {
    //                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
    //                        _dataNewLH.TryGetValue("Level", out var _levelLH);
    //                        idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
    //                    }
    //                    idLevels = await _dbContext.Entity<MsLevel>()
    //                        .Where(x => idLevelPrincipalAndVicePrincipal.Contains(x.Id))
    //                        .OrderBy(x => x.AcademicYear.OrderNumber)
    //                        .ThenBy(x => x.OrderNumber)
    //                        .Select(x => new ItemValueVm
    //                        {
    //                            Id = x.Id,
    //                            Description = x.Description
    //                        }).ToListAsync(CancellationToken);
    //                }
    //            }
    //        }
    //        if (param.SelectedPosition == PositionConstant.VicePrincipal)
    //        {
    //            if (positionUser.Any(y => y.Code == PositionConstant.VicePrincipal))
    //            {
    //                if (positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).Count() > 0)
    //                {
    //                    var Principal = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();
    //                    List<string> IdLevels = new List<string>();
    //                    foreach (var item in Principal)
    //                    {
    //                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
    //                        _dataNewLH.TryGetValue("Level", out var _levelLH);
    //                        idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
    //                    }
    //                    idLevels = await _dbContext.Entity<MsLevel>()
    //                    .Where(x => idLevelPrincipalAndVicePrincipal.Contains(x.Id))
    //                    .OrderBy(x => x.AcademicYear.OrderNumber)
    //                    .ThenBy(x => x.OrderNumber)
    //                    .Select(x => new ItemValueVm
    //                    {
    //                        Id = x.Id,
    //                        Description = x.Description
    //                    }).ToListAsync(CancellationToken);
    //                }
    //            }
    //        }
    //        if (param.SelectedPosition == PositionConstant.LevelHead)
    //        {
    //            if (positionUser.Where(y => y.Code == PositionConstant.LevelHead).ToList() != null)
    //            {
    //                var LevelHead = positionUser.Where(x => x.Code == PositionConstant.LevelHead).ToList();
    //                List<string> idLevel = new List<string>();
    //                foreach (var item in LevelHead)
    //                {
    //                    var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
    //                    _dataNewLH.TryGetValue("Level", out var _levelLH);
    //                    _dataNewLH.TryGetValue("Grade", out var _gradeLH);
    //                    idLevel.Add(_levelLH.Id);
    //                }
    //                idLevels = await _dbContext.Entity<MsLevel>()
    //                    .Where(x => idLevel.Contains(x.Id))
    //                    .OrderBy(x => x.AcademicYear.OrderNumber)
    //                    .ThenBy(x => x.OrderNumber)
    //                    .Select(x => new ItemValueVm
    //                    {
    //                        Id = x.Id,
    //                        Description = x.Description
    //                    }).ToListAsync(CancellationToken);
    //            }
    //        }
    //        if (param.SelectedPosition == PositionConstant.SubjectHead)
    //        {
    //            if (positionUser.Where(y => y.Code == PositionConstant.SubjectHead).ToList() != null)
    //            {
    //                var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHead).ToList();
    //                List<string> IdGrade = new List<string>();
    //                List<string> IdSubject = new List<string>();
    //                List<string> IdLevel = new List<string>();
    //                foreach (var item in LevelHead)
    //                {
    //                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
    //                    _dataNewSH.TryGetValue("Level", out var _leveltSH);
    //                    _dataNewSH.TryGetValue("Grade", out var _gradeSH);
    //                    _dataNewSH.TryGetValue("Department", out var _departmentSH);
    //                    _dataNewSH.TryGetValue("Subject", out var _subjectSH);
    //                    IdLevel.Add(_leveltSH.Id);
    //                }
    //                idLevels = await _dbContext.Entity<MsLevel>()
    //                .Include(x => x.MappingAttendances)
    //                .Where(x => IdLevel.Contains(x.Id))
    //                .Where(x => x.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
    //                .OrderBy(x => x.AcademicYear.OrderNumber)
    //                .ThenBy(x => x.OrderNumber)
    //                .Select(x => new ItemValueVm
    //                {
    //                    Id = x.Id,
    //                    Description = x.Description
    //                }).ToListAsync(CancellationToken);
    //            }
    //        }
    //        if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
    //        {
    //            if (positionUser.Where(y => y.Code == PositionConstant.SubjectHeadAssitant).ToList() != null)
    //            {
    //                var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHeadAssitant).ToList();
    //                List<string> IdGrade = new List<string>();
    //                List<string> IdSubject = new List<string>();
    //                List<string> IdLevel = new List<string>();
    //                foreach (var item in LevelHead)
    //                {
    //                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
    //                    _dataNewSH.TryGetValue("Level", out var _leveltSH);
    //                    _dataNewSH.TryGetValue("Grade", out var _gradeSH);
    //                    _dataNewSH.TryGetValue("Department", out var _departmentSH);
    //                    _dataNewSH.TryGetValue("Subject", out var _subjectSH);
    //                    IdLevel.Add(_leveltSH.Id);
    //                }
    //                idLevels = await _dbContext.Entity<MsLevel>()
    //                .Include(x => x.MappingAttendances)
    //                .Where(x => IdLevel.Contains(x.Id))
    //                .Where(x => x.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
    //                .OrderBy(x => x.AcademicYear.OrderNumber)
    //                .ThenBy(x => x.OrderNumber)
    //                .Select(x => new ItemValueVm
    //                {
    //                    Id = x.Id,
    //                    Description = x.Description
    //                }).ToListAsync(CancellationToken);
    //            }
    //        }
    //        if (param.SelectedPosition == PositionConstant.HeadOfDepartment)
    //        {
    //            if (positionUser.Where(y => y.Code == PositionConstant.HeadOfDepartment).ToList() != null)
    //            {
    //                var HOD = positionUser.Where(x => x.Code == PositionConstant.HeadOfDepartment).ToList();
    //                List<string> idDepartment = new List<string>();
    //                List<string> IdGrade = new List<string>();
    //                List<string> IdSubject = new List<string>();
    //                List<string> IdLevel = new List<string>();
    //                foreach (var item in HOD)
    //                {
    //                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
    //                    _dataNewSH.TryGetValue("Department", out var _departmentSH);
    //                    idDepartment.Add(_departmentSH.Id);
    //                }
    //                var departments = await _dbContext.Entity<MsDepartment>()
    //                    .Include(x => x.DepartmentLevels)
    //                        .ThenInclude(x => x.Level)
    //                            .ThenInclude(x => x.Grades)
    //                    .Where(x => idDepartment.Contains(x.Id))
    //                    .Select(x => x)
    //                    .ToListAsync(CancellationToken);
    //                var idDepartments = departments.Select(x => x.Id);
    //                var subjectByDepartments = await _dbContext.Entity<MsSubject>()
    //                    .Include(x => x.Department)
    //                    .Where(x => idDepartments.Contains(x.IdDepartment))
    //                    .Select(x => new
    //                    {
    //                        x.Id,
    //                        x.IdGrade,
    //                        x.Grade.IdLevel
    //                    }
    //                    )
    //                    .ToListAsync(CancellationToken);
    //                foreach (var department in departments)
    //                {
    //                    if (department.Type == DepartmentType.Level)
    //                    {

    //                        foreach (var departmentLevel in department.DepartmentLevels)
    //                        {
    //                            IdLevel.Add(departmentLevel.IdLevel);
    //                        }
    //                        idLevels = await _dbContext.Entity<MsLevel>()
    //                        .Where(x => IdLevel.Contains(x.Id))
    //                        .OrderBy(x => x.AcademicYear.OrderNumber)
    //                        .ThenBy(x => x.OrderNumber)
    //                        .Select(x => new ItemValueVm
    //                        {
    //                            Id = x.Id,
    //                            Description = x.Description
    //                        }).ToListAsync(CancellationToken);
    //                    }
    //                    else
    //                    {
    //                        idLevels = await _dbContext.Entity<MsLevel>()
    //                            .Include(x => x.AcademicYear)
    //                            .Where(x => x.IdAcademicYear == param.IdAcademicYear)
    //                            .OrderBy(x => x.AcademicYear.OrderNumber)
    //                                .ThenBy(x => x.OrderNumber)
    //                        .Select(x => new ItemValueVm
    //                        {
    //                            Id = x.Id,
    //                            Description = x.Description
    //                        }).ToListAsync(CancellationToken);
    //                    }
    //                }
    //            }
    //        }
    //        if (param.SelectedPosition == PositionConstant.ClassAdvisor)
    //        {
    //            List<string> IdLevel = new List<string>();
    //            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
    //            {
    //                IdLevel = dataHomeroomTeacher
    //                .GroupBy(x => new
    //                {
    //                    x.Homeroom.Grade.IdLevel,
    //                    x.Homeroom.Grade.Level.Description
    //                })
    //                .Select(x => x.Key.IdLevel).ToList();

    //                idLevels = await _dbContext.Entity<MsLevel>()
    //                           .Include(x => x.AcademicYear)
    //                           .Where(x => x.IdAcademicYear == param.IdAcademicYear)
    //                           .Where(x => IdLevel.Contains(x.Id))
    //                           .OrderBy(x => x.AcademicYear.OrderNumber)
    //                               .ThenBy(x => x.OrderNumber)
    //                       .Select(x => new ItemValueVm
    //                       {
    //                           Id = x.Id,
    //                           Description = x.Description
    //                       }).ToListAsync(CancellationToken);
    //            }
    //        }

    //        if (param.SelectedPosition == PositionConstant.SubjectTeacher)
    //        {
    //            List<string> IdLevel = new List<string>();
    //            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
    //            {
    //                IdLevel = dataLessonTeacher.GroupBy(x => new
    //                {
    //                    x.Lesson.Grade.IdLevel,
    //                    x.Lesson.Grade.Level.Description
    //                }).Select(x => x.Key.IdLevel).ToList();
    //                idLevels = await _dbContext.Entity<MsLevel>()
    //                          .Include(x => x.AcademicYear)
    //                          .Include(x => x.MappingAttendances)
    //                          .Where(x => x.IdAcademicYear == param.IdAcademicYear)
    //                          .Where(x => IdLevel.Contains(x.Id))
    //                          //.Where(x => x.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
    //                          .OrderBy(x => x.AcademicYear.OrderNumber)
    //                            .ThenBy(x => x.OrderNumber)
    //                              .ThenBy(x => x.OrderNumber)
    //                      .Select(x => new ItemValueVm
    //                      {
    //                          Id = x.Id,
    //                          Description = x.Description
    //                      }).ToListAsync(CancellationToken);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if (param.SelectedPosition == PositionConstant.ClassAdvisor)
    //        {
    //            List<string> IdLevel = new List<string>();
    //            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
    //            {
    //                IdLevel = dataHomeroomTeacher
    //                .GroupBy(x => new
    //                {
    //                    x.Homeroom.Grade.IdLevel,
    //                    x.Homeroom.Grade.Level.Description
    //                })
    //                .Select(x => x.Key.IdLevel).ToList();

    //                idLevels = await _dbContext.Entity<MsLevel>()
    //                           .Include(x => x.AcademicYear)
    //                           .Where(x => x.IdAcademicYear == param.IdAcademicYear)
    //                           .Where(x => IdLevel.Contains(x.Id))
    //                           .OrderBy(x => x.AcademicYear.OrderNumber)
    //                               .ThenBy(x => x.OrderNumber)
    //                       .Select(x => new ItemValueVm
    //                       {
    //                           Id = x.Id,
    //                           Description = x.Description
    //                       }).ToListAsync(CancellationToken);
    //            }
    //        }

    //        if (param.SelectedPosition == PositionConstant.SubjectTeacher)
    //        {
    //            List<string> IdLevel = new List<string>();
    //            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
    //            {
    //                IdLevel = dataLessonTeacher.GroupBy(x => new
    //                {
    //                    x.Lesson.Grade.IdLevel,
    //                    x.Lesson.Grade.Level.Description
    //                }).Select(x => x.Key.IdLevel).ToList();
    //                idLevels = await _dbContext.Entity<MsLevel>()
    //                             .Include(x => x.AcademicYear)
    //                           .Where(x => x.IdAcademicYear == param.IdAcademicYear)
    //                           .Where(x => IdLevel.Contains(x.Id))
    //                           .OrderBy(x => x.AcademicYear.OrderNumber)
    //                               .ThenBy(x => x.OrderNumber)
    //                         .Select(x => new ItemValueVm
    //                         {
    //                             Id = x.Id,
    //                             Description = x.Description
    //                         }).ToListAsync(CancellationToken);
    //            }
    //        }
    //    }
    //    #endregion
    //    return Request.CreateApiResult2(idLevels as object);
    //}
  }
}
