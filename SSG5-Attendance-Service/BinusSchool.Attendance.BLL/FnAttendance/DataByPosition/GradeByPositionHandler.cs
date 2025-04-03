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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.GradeByPosition;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition
{
  public class GradeByPositionHandler : FunctionsHttpSingleHandler
  {
    private readonly IAttendanceDbContext _dbContext;

    public GradeByPositionHandler(IAttendanceDbContext dbContext)
    {
      _dbContext = dbContext;
    }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            List<GradeByPositionResult> listUserPosition = new List<GradeByPositionResult>();

            var param = Request.ValidateParams<GradeByPositionRequest>(nameof(GradeByPositionRequest.IdAcademicYear),
                                                                         nameof(GradeByPositionRequest.IdUser),
                                                                         nameof(GradeByPositionRequest.SelectedPosition));

            var gatAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => e.Id == param.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);

            var listGrade = await _dbContext.Entity<MsGrade>()
                                .Include(e => e.Level)
                                .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear && x.IdLevel==param.IdLevel)
                                .Select(e => new
                                {
                                    IdLevel = e.IdLevel,
                                    IdGrade = e.Id,
                                    Grade = e.Description
                                })
                                .ToListAsync(CancellationToken);

            if (param.SelectedPosition == "All")
            {
                #region Staff
                var listLevelByStaff = listGrade
                    .GroupBy(e => new
                    {
                        e.IdLevel,
                        e.IdGrade,
                        e.Grade,
                    })
                    .Select(e => new GradeByPositionResult
                    {
                        IdUser = param.IdUser,
                        Posistion = param.SelectedPosition,
                        Grade = new GradeByPosition
                        {
                            IdLevel = e.Key.IdLevel,
                            IdGrade = e.Key.IdGrade,
                            Grade = e.Key.Grade
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
                                    .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear && x.Grade.IdLevel==param.IdLevel)
                                    .Select(e => new
                                    {
                                        IdLesson = e.Id,
                                        IdSubject = e.IdSubject,
                                        IdLevel = e.Grade.IdLevel,
                                        IdGrade = e.Grade.Id,
                                        Grade = e.Grade.Description,
                                    })
                                    .ToListAsync(CancellationToken);



                var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                    .Include(e => e.Level)
                                     .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear && x.IdLevel==param.IdLevel)
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
                            var listGradeById = listGrade.Where(e => e.IdLevel == itemDepartement.Level.Id).ToList();

                            foreach (var itemGrade in listGradeById)
                            {
                                GradeByPositionResult newRespondent = new GradeByPositionResult
                                {
                                    IdUser = item.IdUser,
                                    Posistion = param.SelectedPosition,
                                    Grade = new GradeByPosition
                                    {
                                        IdLevel = itemGrade.IdLevel,
                                        IdGrade = itemGrade.IdGrade,
                                        Grade = itemGrade.Grade
                                    }
                                };
                                listUserPosition.Add(newRespondent);
                            }
                        }
                    }
                    else if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null && _DepartemenPosition != null)
                    {
                        var listLessonByIdSubject = listLesson.Where(e => e.IdSubject == _SubjectPosition.Id).ToList();

                        foreach (var itemSubject in listLessonByIdSubject)
                        {
                            GradeByPositionResult newRespondent = new GradeByPositionResult
                            {
                                IdUser = item.IdUser,
                                Posistion = param.SelectedPosition,
                                Grade = new GradeByPosition
                                {
                                    IdLevel = itemSubject.IdLevel,
                                    IdGrade = itemSubject.IdGrade,
                                    Grade = itemSubject.Grade
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
                            GradeByPositionResult newRespondent = new GradeByPositionResult
                            {
                                IdUser = item.IdUser,
                                Posistion = param.SelectedPosition,
                                Grade = new GradeByPosition
                                {
                                    IdLevel = itemGrade.IdLevel,
                                    IdGrade = itemGrade.IdGrade,
                                    Grade = itemGrade.Grade
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
                            GradeByPositionResult newRespondent = new GradeByPositionResult
                            {
                                IdUser = item.IdUser,
                                Posistion = param.SelectedPosition,
                                Grade = new GradeByPosition
                                {
                                    IdLevel = itemGrade.IdLevel,
                                    IdGrade = itemGrade.IdGrade,
                                    Grade = itemGrade.Grade
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

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Grade.IdLevel == param.IdLevel);

                if (!string.IsNullOrEmpty(param.IdUser))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.IdBinusian == param.IdUser);

                if (!string.IsNullOrEmpty(param.SelectedPosition))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.TeacherPosition.LtPosition.Code == PositionConstant.ClassAdvisor ||
                    e.TeacherPosition.LtPosition.Code == PositionConstant.CoTeacher);

                var listHomeroomTeacher = await queryHomeroomTeacher.ToListAsync(CancellationToken);

                foreach (var itemHomeroomTeacher in listHomeroomTeacher)
                {
                    GradeByPositionResult newRespondent = new GradeByPositionResult
                    {
                        IdUser = itemHomeroomTeacher.IdBinusian,
                        Posistion = param.SelectedPosition,
                        Grade = new GradeByPosition
                        {
                            IdLevel = itemHomeroomTeacher.Homeroom.Grade.Level.Id,
                            IdGrade = itemHomeroomTeacher.Homeroom.Grade.Id,
                            Grade = itemHomeroomTeacher.Homeroom.Grade.Description,
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

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryLessonBySt = queryLessonBySt.Where(e => e.Lesson.Grade.IdLevel == param.IdLevel);

                if (!string.IsNullOrEmpty(param.IdUser))
                    queryLessonBySt = queryLessonBySt.Where(e => e.IdUser == param.IdUser);


                if (listTeacherPosition.Any())
                {
                    var listLessonBySt = await queryLessonBySt
                                       .Select(e => new GradeByPositionResult
                                       {
                                           IdUser = e.IdUser,
                                           Posistion = param.SelectedPosition,
                                           Grade = new GradeByPosition
                                           {
                                               IdLevel = e.Lesson.Grade.Level.Id,
                                               IdGrade = e.Lesson.Grade.Id,
                                               Grade = e.Lesson.Grade.Description
                                           }
                                       })
                                       .Distinct()
                                       .ToListAsync(CancellationToken);

                    listUserPosition.AddRange(listLessonBySt);
                }
                #endregion
            }

            var listLevels = listUserPosition
                                .Where(e => e.Posistion == param.SelectedPosition && e.Grade.IdLevel==param.IdLevel)
                                .GroupBy(e => new
                                {
                                    IdGrade = e.Grade.IdGrade,
                                    Grade = e.Grade.Grade
                                })
                                .Select(e => new ItemValueVm
                                {
                                    Id = e.Key.IdGrade,
                                    Description = e.Key.Grade,
                                })
                                .ToList();

            return Request.CreateApiResult2(listLevels as object);
        }

        //protected override async Task<ApiErrorResult<object>> Handler1()
        //{
        //    #region Get Position User
        //    var param = Request.ValidateParams<GradeByPositionRequest>(nameof(GradeByPositionRequest.IdAcademicYear),
        //                                                                nameof(GradeByPositionRequest.IdLevel),
        //                                                                 nameof(GradeByPositionRequest.IdUser),
        //                                                                 nameof(GradeByPositionRequest.SelectedPosition));
        //    List<string> avaiablePosition = new List<string>();
        //    var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
        //    .Include(x => x.Homeroom)
        //        .ThenInclude(x => x.Grade)
        //            .ThenInclude(x => x.Level)
        //    .Where(x => x.IdBinusian == param.IdUser)
        //    .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear)
        //    .Where(x => x.Homeroom.Grade.IdLevel == param.IdLevel)
        //    .Distinct()
        //    .Select(x => x).ToListAsync(CancellationToken);

        //    var dataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
        //    .Include(x => x.Lesson)
        //        .ThenInclude(x => x.Grade)
        //            .ThenInclude(x => x.Level)
        //    .Where(x => x.IdUser == param.IdUser)
        //    .Where(x => x.Lesson.Grade.IdLevel == param.IdLevel)
        //    .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear)
        //    .Distinct()
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
        //    List<ItemValueVm> idGrades = new List<ItemValueVm>();
        //    if (positionUser.Count > 0 || avaiablePosition.Count>0)
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
        //                        if (_levelLH.Id == param.IdLevel)
        //                        {
        //                            idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
        //                                .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                                    .ThenBy(x => x.Level.OrderNumber)
        //                                        .ThenBy(x => x.OrderNumber)
        //                            .Select(x => new ItemValueVm
        //                            {
        //                                Id = x.Id,
        //                                Description = x.Description
        //                            }).ToListAsync(CancellationToken);
        //                        }
        //                    }
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
        //                        if (_levelLH.Id == param.IdLevel)
        //                        {
        //                            idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
        //                            .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                                .ThenBy(x => x.Level.OrderNumber)
        //                                    .ThenBy(x => x.OrderNumber)
        //                            .Select(x => new ItemValueVm
        //                            {
        //                                Id = x.Id,
        //                                Description = x.Description
        //                            }).ToListAsync(CancellationToken);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (param.SelectedPosition == PositionConstant.LevelHead)
        //        {
        //            if (positionUser.Where(y => y.Code == PositionConstant.LevelHead).ToList() != null)
        //            {
        //                var LevelHead = positionUser.Where(x => x.Code == PositionConstant.LevelHead).ToList();
        //                List<string> IdGrade = new List<string>();
        //                foreach (var item in LevelHead)
        //                {
        //                    var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                    _dataNewLH.TryGetValue("Level", out var _levelLH);
        //                    if (_levelLH.Id == param.IdLevel)
        //                    {
        //                        _dataNewLH.TryGetValue("Grade", out var _gradeLH);
        //                        IdGrade.Add(_gradeLH.Id);
        //                    }
        //                    idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
        //                            .Where(x => IdGrade.Contains(x.Id))
        //                            .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                                .ThenBy(x => x.Level.OrderNumber)
        //                                    .ThenBy(x => x.OrderNumber)
        //                            .Select(x => new ItemValueVm
        //                            {
        //                                Id = x.Id,
        //                                Description = x.Description
        //                            }).ToListAsync(CancellationToken);
        //                }
        //            }
        //        }
        //        if (param.SelectedPosition == PositionConstant.SubjectHead)
        //        {
        //            if (positionUser.Where(y => y.Code == PositionConstant.SubjectHead).ToList() != null)
        //            {
        //                var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHead).ToList();
        //                List<string> IdGrade = new List<string>();
        //                List<string> IdSubject = new List<string>();
        //                foreach (var item in LevelHead)
        //                {
        //                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                    _dataNewSH.TryGetValue("Level", out var _leveltSH);
        //                    if (_leveltSH.Id == param.IdLevel)
        //                    {
        //                        _dataNewSH.TryGetValue("Grade", out var _gradeSH);
        //                        IdGrade.Add(_gradeSH.Id);
        //                    }
        //                    idGrades = await _dbContext.Entity<MsGrade>()
        //                    .Include(x => x.Level)
        //                        .ThenInclude(x => x.MappingAttendances)
        //                    .Where(x => x.IdLevel == param.IdLevel)
        //                    .Where(x => IdGrade.Contains(x.Id))
        //                    .Where(x => x.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
        //                    .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                        .ThenBy(x => x.Level.OrderNumber)
        //                            .ThenBy(x => x.OrderNumber)
        //                    .Select(x => new ItemValueVm
        //                    {
        //                        Id = x.Id,
        //                        Description = x.Description
        //                    }).ToListAsync(CancellationToken);
        //                }
        //            }
        //        }
        //        if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
        //        {
        //            if (positionUser.Where(y => y.Code == PositionConstant.SubjectHeadAssitant).ToList() != null)
        //            {
        //                var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHeadAssitant).ToList();
        //                List<string> IdGrade = new List<string>();
        //                List<string> IdSubject = new List<string>();
        //                foreach (var item in LevelHead)
        //                {
        //                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                    _dataNewSH.TryGetValue("Level", out var _leveltSH);
        //                    if (_leveltSH.Id == param.IdLevel)
        //                    {
        //                        _dataNewSH.TryGetValue("Grade", out var _gradeSH);
        //                        IdGrade.Add(_gradeSH.Id);
        //                    }
        //                    idGrades = await _dbContext.Entity<MsGrade>()
        //                    .Include(x => x.Level)
        //                        .ThenInclude(x => x.MappingAttendances)
        //                    .Where(x => x.IdLevel == param.IdLevel)
        //                    .Where(x => IdGrade.Contains(x.Id))
        //                    .Where(x => x.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
        //                    .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                        .ThenBy(x => x.Level.OrderNumber)
        //                            .ThenBy(x => x.OrderNumber)
        //                    .Select(x => new ItemValueVm
        //                    {
        //                        Id = x.Id,
        //                        Description = x.Description
        //                    }).ToListAsync(CancellationToken);
        //                }
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
        //                            if (departmentLevel.IdLevel == param.IdLevel)
        //                            {

        //                                idGrades = await _dbContext.Entity<MsGrade>()
        //                                    .Include(x => x.Level)
        //                                        .ThenInclude(x => x.AcademicYear)
        //                                    .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                                        .ThenBy(x => x.Level.OrderNumber)
        //                                            .ThenBy(x => x.OrderNumber)
        //                                    .Where(x => x.IdLevel == param.IdLevel)
        //                                .Select(x => new ItemValueVm
        //                                {
        //                                    Id = x.Id,
        //                                    Description = x.Description
        //                                }).ToListAsync(CancellationToken);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        idGrades = await _dbContext.Entity<MsGrade>()
        //                            .Include(x => x.Level)
        //                                .ThenInclude(x => x.AcademicYear)
        //                            .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                                .ThenBy(x => x.Level.OrderNumber)
        //                                    .ThenBy(x => x.OrderNumber)
        //                            .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
        //                            .Where(x => x.IdLevel == param.IdLevel)
        //                            .Select(x => new ItemValueVm
        //                            {
        //                                Id = x.Id,
        //                                Description = x.Description
        //                            }).ToListAsync(CancellationToken);
        //                    }
        //                }
        //            }
        //        }
        //        //if (param.SelectedPosition == PositionConstant.ClassAdvisor)
        //        //{
        //        //    List<string> IdGrade = new List<string>();
        //        //    if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
        //        //    {
        //        //        IdGrade = dataHomeroomTeacher.GroupBy(x => new
        //        //        {
        //        //            x.Homeroom.IdGrade,
        //        //            x.Homeroom.Grade.Description
        //        //        }).Select(x => x.Key.IdGrade).ToList();

        //        //        idGrades = await _dbContext.Entity<MsGrade>()
        //        //        .Include(x => x.Level)
        //        //            .ThenInclude(x => x.AcademicYear)
        //        //        .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //        //            .ThenBy(x => x.Level.OrderNumber)
        //        //                .ThenBy(x => x.OrderNumber)
        //        //        .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
        //        //        .Where(x => x.IdLevel == param.IdLevel)
        //        //        .Where(x => IdGrade.Contains(x.Id))
        //        //        .Select(x => new ItemValueVm
        //        //        {
        //        //            Id = x.Id,
        //        //            Description = x.Description
        //        //        }).ToListAsync(CancellationToken);
        //        //    }
        //        //}
        //        //if (param.SelectedPosition == PositionConstant.SubjectTeacher)
        //        //{
        //        //    List<string> IdGrade = new List<string>();
        //        //    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
        //        //    {
        //        //        var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
        //        //        idGrades = await _dbContext.Entity<MsLessonPathway>()
        //        //            .Include(x => x.HomeroomPathway)
        //        //                .ThenInclude(x => x.Homeroom)
        //        //                    .ThenInclude(x => x.Grade)
        //        //            .Include(x => x.HomeroomPathway)
        //        //                .ThenInclude(x => x.Homeroom)
        //        //                    .ThenInclude(x => x.GradePathwayClassroom)
        //        //                        .ThenInclude(x => x.Classroom)
        //        //            .Where(x => x.Lesson.Grade.IdLevel == param.IdLevel)
        //        //            .Where(x => idLessons.Contains(x.IdLesson))
        //        //            .GroupBy(x => new
        //        //            {
        //        //               orderNumber = x.HomeroomPathway.Homeroom.Grade.OrderNumber,
        //        //               x.HomeroomPathway.Homeroom.IdGrade,
        //        //               grade = x.HomeroomPathway.Homeroom.Grade.Code
        //        //            })
        //        //            .OrderBy(x => x.Key.orderNumber)
        //        //            .Select(x => new ItemValueVm
        //        //            {
        //        //                Id = x.Key.IdGrade,
        //        //                Description = x.Key.grade
        //        //            }).ToListAsync(CancellationToken);
        //        //    }
        //        //}
        //        if (param.SelectedPosition == PositionConstant.AffectiveCoordinator)
        //        {
        //            if (positionUser.Where(y => y.Code == PositionConstant.AffectiveCoordinator).ToList() != null)
        //            {
        //                var AffectiveCoordinator = positionUser.Where(x => x.Code == PositionConstant.AffectiveCoordinator).ToList();
        //                List<string> IdGrade = new List<string>();
        //                foreach (var item in AffectiveCoordinator)
        //                {
        //                    var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                    _dataNewLH.TryGetValue("Level", out var _levelLH);
        //                    if (_levelLH.Id == param.IdLevel)
        //                    {
        //                        _dataNewLH.TryGetValue("Grade", out var _gradeLH);
        //                        if (_gradeLH == null)
        //                        {
        //                            IdGrade = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
        //                            .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                                .ThenBy(x => x.Level.OrderNumber)
        //                                    .ThenBy(x => x.OrderNumber)
        //                            .Select(x => x.Id).ToListAsync(CancellationToken);
        //                        }
        //                        else
        //                        {
        //                            IdGrade.Add(_gradeLH.Id);
        //                        }

        //                        idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
        //                            .Where(x => IdGrade.Contains(x.Id))
        //                            .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                                .ThenBy(x => x.Level.OrderNumber)
        //                                    .ThenBy(x => x.OrderNumber)
        //                            .Select(x => new ItemValueVm
        //                            {
        //                                Id = x.Id,
        //                                Description = x.Description
        //                            }).ToListAsync(CancellationToken);
        //                    }
        //                }
        //            }
        //        }
        //        if (param.SelectedPosition == PositionConstant.ClassAdvisor)
        //        {
        //            List<string> IdGrade = new List<string>();
        //            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
        //            {
        //                IdGrade = dataHomeroomTeacher.GroupBy(x => new
        //                {
        //                    x.Homeroom.IdGrade,
        //                    x.Homeroom.Grade.Description
        //                }).Select(x => x.Key.IdGrade).ToList();

        //                idGrades = await _dbContext.Entity<MsGrade>()
        //                .Include(x => x.Level)
        //                    .ThenInclude(x => x.AcademicYear)
        //                .OrderBy(x => x.Level.AcademicYear.OrderNumber)
        //                    .ThenBy(x => x.Level.OrderNumber)
        //                        .ThenBy(x => x.OrderNumber)
        //                .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
        //                .Where(x => x.IdLevel == param.IdLevel)
        //                .Where(x => IdGrade.Contains(x.Id))
        //                .Select(x => new ItemValueVm
        //                {
        //                    Id = x.Id,
        //                    Description = x.Description
        //                }).ToListAsync(CancellationToken);
        //            }
        //        }
        //        if (param.SelectedPosition == PositionConstant.SubjectTeacher)
        //        {
        //            List<string> IdGrade = new List<string>();
        //            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
        //            {
        //                var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
        //                idGrades = await _dbContext.Entity<MsLessonPathway>()
        //                    .Include(x => x.HomeroomPathway)
        //                        .ThenInclude(x => x.Homeroom)
        //                            .ThenInclude(x => x.Grade)
        //                    .Include(x => x.HomeroomPathway)
        //                        .ThenInclude(x => x.Homeroom)
        //                            .ThenInclude(x => x.GradePathwayClassroom)
        //                                .ThenInclude(x => x.Classroom)
        //                    .Where(x => x.Lesson.Grade.IdLevel == param.IdLevel)
        //                    .Where(x => idLessons.Contains(x.IdLesson))
        //                    .GroupBy(x => new
        //                    {
        //                        orderNumber = x.HomeroomPathway.Homeroom.Grade.OrderNumber,
        //                        x.HomeroomPathway.Homeroom.IdGrade,
        //                        grade = x.HomeroomPathway.Homeroom.Grade.Code
        //                    })
        //                    .OrderBy(x => x.Key.orderNumber)
        //                    .Select(x => new ItemValueVm
        //                    {
        //                        Id = x.Key.IdGrade,
        //                        Description = x.Key.grade
        //                    }).ToListAsync(CancellationToken);
        //            }
        //        }
        //    }
        //    #endregion
        //    return Request.CreateApiResult2(idGrades as object);
        //}
    }
}
