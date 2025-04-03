using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.HomeroomByPosition;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition
{
    public class HomeroomByPositionHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public HomeroomByPositionHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            List<HomeroomByPositionResult> listUserPosition = new List<HomeroomByPositionResult>();

            var param = Request.ValidateParams<HomeroomByPositionRequest>(nameof(HomeroomByPositionRequest.IdAcademicYear),
                                                                            nameof(HomeroomByPositionRequest.IdLevel),
                                                                            nameof(HomeroomByPositionRequest.IdGrade),
                                                                             nameof(HomeroomByPositionRequest.IdUser),
                                                                             nameof(HomeroomByPositionRequest.SelectedPosition));

            var gatAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => e.Id == param.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);

            var listHomeroom = await _dbContext.Entity<MsHomeroom>()
                                .Include(e => e.Grade).ThenInclude(e=>e.Level)
                                .Include(e => e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear 
                                        && x.Grade.IdLevel == param.IdLevel
                                        && x.IdGrade==param.IdGrade
                                        && x.Semester==param.Semester)
                                .Select(e => new
                                {
                                    IdLevel = e.Grade.IdLevel,
                                    IdGrade = e.Grade.Id,
                                    IdHomeroom = e.Id,
                                    Homeroom = e.Grade.Code + e.GradePathwayClassroom.Classroom.Code,
                                    Semester = e.Semester
                                })
                                .ToListAsync(CancellationToken);

            if (param.SelectedPosition == "All")
            {
                #region Staff
                var listLevelByStaff = listHomeroom
                    .GroupBy(e => new
                    {
                        e.IdLevel,
                        e.IdGrade,
                        e.IdHomeroom,
                        e.Homeroom,
                        e.Semester
                    })
                    .Select(e => new HomeroomByPositionResult
                    {
                        IdUser = param.IdUser,
                        Posistion = param.SelectedPosition,
                        Homeroom = new HomeroomByPosition
                        {
                            IdLevel = e.Key.IdLevel,
                            IdGrade = e.Key.IdGrade,
                            IdHomeroom = e.Key.IdHomeroom,
                            Homeroom = e.Key.Homeroom,
                            Semester = e.Key.Semester
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

                var listStudentEnroll = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                    .Include(e => e.Lesson)
                                    .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                    .Where(x => x.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear
                                        && x.HomeroomStudent.Homeroom.Grade.IdLevel == param.IdLevel
                                        && x.HomeroomStudent.Homeroom.Grade.Id == param.IdGrade
                                        && x.HomeroomStudent.Homeroom.Semester == param.Semester
                                        )
                                    .Select(e => new
                                    {
                                        IdLesson = e.Id,
                                        IdSubject = e.IdSubject,
                                        IdLevel = e.HomeroomStudent.Homeroom.Grade.IdLevel,
                                        IdGrade = e.HomeroomStudent.Homeroom.Grade.Id,
                                        IdHomeroom = e.HomeroomStudent.Homeroom.Id,
                                        Homeroom = e.HomeroomStudent.Homeroom.Grade.Code 
                                                    + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                        Semester = e.HomeroomStudent.Homeroom.Semester
                                    })
                                    .ToListAsync(CancellationToken);



                var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                    .Include(e => e.Level)
                                     .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear && x.IdLevel == param.IdLevel)
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
                            var listGradeById = listHomeroom
                                                .Where(e => e.IdLevel == itemDepartement.Level.Id
                                                        && e.IdGrade==param.IdGrade
                                                        && e.Semester==param.Semester
                                                        )
                                                .ToList();

                            foreach (var itemGrade in listGradeById)
                            {
                                HomeroomByPositionResult newRespondent = new HomeroomByPositionResult
                                {
                                    IdUser = item.IdUser,
                                    Posistion = param.SelectedPosition,
                                    Homeroom = new HomeroomByPosition
                                    {
                                        IdLevel = itemGrade.IdLevel,
                                        IdGrade = itemGrade.IdGrade,
                                        IdHomeroom = itemGrade.IdHomeroom,
                                        Homeroom = itemGrade.Homeroom,
                                        Semester = itemGrade.Semester
                                    }
                                };
                                listUserPosition.Add(newRespondent);
                            }
                        }
                    }
                    else if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null && _DepartemenPosition != null)
                    {
                        var listStudentEnrollByIdSubject = listStudentEnroll
                                                    .Where(e => e.IdSubject == _SubjectPosition.Id
                                                                && e.IdLevel== param.IdLevel
                                                                && e.IdGrade == param.IdGrade
                                                                && e.Semester == param.Semester)
                                                    .ToList();

                        foreach (var itemStudentEnroll in listStudentEnrollByIdSubject)
                        {
                            HomeroomByPositionResult newRespondent = new HomeroomByPositionResult
                            {
                                IdUser = item.IdUser,
                                Posistion = param.SelectedPosition,
                                Homeroom = new HomeroomByPosition
                                {
                                    IdLevel = itemStudentEnroll.IdLevel,
                                    IdGrade = itemStudentEnroll.IdGrade,
                                    IdHomeroom = itemStudentEnroll.IdHomeroom,
                                    Homeroom = itemStudentEnroll.Homeroom,
                                    Semester = itemStudentEnroll.Semester
                                }
                            };
                            listUserPosition.Add(newRespondent);
                        }
                    }
                    else if (_SubjectPosition == null && _GradePosition != null && _LevelPosition != null)
                    {
                        var listGradeById = listHomeroom
                                            .Where(e => e.IdGrade == _GradePosition.Id
                                                        && e.Semester==param.Semester)
                                            .ToList();

                        foreach (var itemGrade in listGradeById)
                        {
                            HomeroomByPositionResult newRespondent = new HomeroomByPositionResult
                            {
                                IdUser = item.IdUser,
                                Posistion = param.SelectedPosition,
                                Homeroom = new HomeroomByPosition
                                {
                                    IdLevel = itemGrade.IdLevel,
                                    IdGrade = itemGrade.IdGrade,
                                    IdHomeroom = itemGrade.IdHomeroom,
                                    Homeroom = itemGrade.Homeroom,
                                    Semester = itemGrade.Semester
                                }
                            };
                            listUserPosition.Add(newRespondent);
                        }
                    }
                    else if (_SubjectPosition == null && _GradePosition == null && _LevelPosition != null)
                    {
                        var listGradeByIdLevel = listHomeroom
                                                .Where(e => e.IdLevel == _LevelPosition.Id
                                                        && e.IdGrade == param.IdGrade
                                                        && e.Semester == param.Semester
                                                        )
                                                .ToList();

                        foreach (var itemGrade in listGradeByIdLevel)
                        {
                            HomeroomByPositionResult newRespondent = new HomeroomByPositionResult
                            {
                                IdUser = item.IdUser,
                                Posistion = param.SelectedPosition,
                                Homeroom = new HomeroomByPosition
                                {
                                    IdLevel = itemGrade.IdLevel,
                                    IdGrade = itemGrade.IdGrade,
                                    IdHomeroom = itemGrade.IdHomeroom,
                                    Homeroom = itemGrade.Homeroom,
                                    Semester = itemGrade.Semester
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
                                            .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                            .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Grade.IdLevel == param.IdLevel);

                if (!string.IsNullOrEmpty(param.IdGrade))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Grade.Id == param.IdGrade);

                if (!string.IsNullOrEmpty(param.Semester.ToString()))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Semester == param.Semester);

                if (!string.IsNullOrEmpty(param.IdUser))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.IdBinusian == param.IdUser);

                if (!string.IsNullOrEmpty(param.SelectedPosition))
                    queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.TeacherPosition.LtPosition.Code == param.SelectedPosition);

                var listHomeroomTeacher = await queryHomeroomTeacher.ToListAsync(CancellationToken);

                foreach (var itemHomeroomTeacher in listHomeroomTeacher)
                {
                    HomeroomByPositionResult newRespondent = new HomeroomByPositionResult
                    {
                        IdUser = itemHomeroomTeacher.IdBinusian,
                        Posistion = param.SelectedPosition,
                        Homeroom = new HomeroomByPosition
                        {
                            IdLevel = itemHomeroomTeacher.Homeroom.Grade.Level.Id,
                            IdGrade = itemHomeroomTeacher.Homeroom.Grade.Id,
                            IdHomeroom = itemHomeroomTeacher.Homeroom.Id,
                            Homeroom = itemHomeroomTeacher.Homeroom.Grade.Code
                                        + itemHomeroomTeacher.Homeroom.GradePathwayClassroom.Classroom.Code,
                            Semester = itemHomeroomTeacher.Homeroom.Semester,
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
                                       .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear && x.Lesson.Grade.IdLevel == param.IdLevel);

                if (!string.IsNullOrEmpty(param.IdUser))
                    queryLessonBySt = queryLessonBySt.Where(e => e.IdUser == param.IdUser);


                if (listTeacherPosition.Any())
                {
                    var listLessonBySt = await queryLessonBySt
                                       .Select(e =>e.IdLesson)
                                       .Distinct()
                                       .ToListAsync(CancellationToken);

                    var listStudentEnrollByIdSubject = listStudentEnroll
                                                    .Where(e => listLessonBySt.Contains(e.IdLesson)
                                                                && e.IdLevel == param.IdLevel
                                                                && e.IdGrade == param.IdGrade
                                                                && e.Semester == param.Semester)
                                                    .ToList();

                    foreach (var itemStudentEnroll in listStudentEnrollByIdSubject)
                    {
                        HomeroomByPositionResult newRespondent = new HomeroomByPositionResult
                        {
                            IdUser = param.IdUser,
                            Posistion = param.SelectedPosition,
                            Homeroom = new HomeroomByPosition
                            {
                                IdLevel = itemStudentEnroll.IdLevel,
                                IdGrade = itemStudentEnroll.IdGrade,
                                IdHomeroom = itemStudentEnroll.IdHomeroom,
                                Homeroom = itemStudentEnroll.Homeroom,
                                Semester = itemStudentEnroll.Semester
                            }
                        };
                        listUserPosition.Add(newRespondent);
                    }
                }
                #endregion
            }

            var listLevels = listUserPosition
                                .Where(e => e.Posistion == param.SelectedPosition 
                                        && e.Homeroom.IdLevel == param.IdLevel
                                        && e.Homeroom.IdGrade == param.IdGrade
                                        && e.Homeroom.Semester == param.Semester
                                        )
                                .GroupBy(e => new
                                {
                                    IdHomeroom = e.Homeroom.IdHomeroom,
                                    Homeroom = e.Homeroom.Homeroom
                                })
                                .Select(e => new ItemValueVm
                                {
                                    Id = e.Key.IdHomeroom,
                                    Description = e.Key.Homeroom,
                                })
                                .OrderBy(e=>e.Description).ToList();

            return Request.CreateApiResult2(listLevels as object);
        }

        //protected override async Task<ApiErrorResult<object>> Handler()
        //{
        //    #region Get Position User
        //    var param = Request.ValidateParams<HomeroomByPositionRequest>(nameof(HomeroomByPositionRequest.IdAcademicYear),
        //                                                                nameof(HomeroomByPositionRequest.IdLevel),
        //                                                                nameof(HomeroomByPositionRequest.IdGrade),
        //                                                                 nameof(HomeroomByPositionRequest.IdUser),
        //                                                                 nameof(HomeroomByPositionRequest.SelectedPosition));
        //    var avaiablePosition = new List<string>();
        //    var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
        //    .Include(x => x.Homeroom)
        //        .ThenInclude(x => x.Grade)
        //            .ThenInclude(x => x.Level)
        //    .Include(x => x.Homeroom)
        //        .ThenInclude(x => x.GradePathwayClassroom)
        //            .ThenInclude(x => x.Classroom)
        //    .Where(x => x.IdBinusian == param.IdUser)
        //    .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear)
        //    .Where(x => x.Homeroom.Grade.IdLevel == param.IdLevel)
        //    .Where(x => x.Homeroom.IdGrade == param.IdGrade)
        //    .OrderBy(x => x.Homeroom.Grade.Level.AcademicYear.OrderNumber)
        //        .ThenBy(x => x.Homeroom.Grade.Level.OrderNumber)
        //            .ThenBy(x => x.Homeroom.Grade.OrderNumber)
        //                .ThenBy(x => x.Homeroom.GradePathwayClassroom.Classroom.Code)
        //    .Distinct()
        //    .Select(x => x).ToListAsync(CancellationToken);

        //    var dataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
        //    .Include(x => x.Lesson)
        //        .ThenInclude(x => x.Grade)
        //            .ThenInclude(x => x.Level)
        //    .Include(x => x.Lesson)
        //        .ThenInclude(x => x.LessonPathways)
        //            .ThenInclude(x => x.HomeroomPathway)
        //                .ThenInclude(x => x.Homeroom)
        //    .Where(x => x.IdUser == param.IdUser)
        //    .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear)
        //    .Where(x => x.Lesson.Grade.IdLevel == param.IdLevel)
        //    .Where(x => x.Lesson.IdGrade == param.IdGrade)
        //    .Where(x => x.Lesson.Semester == param.Semester)
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
        //        avaiablePosition.Add(pu.Code);
        //    if (avaiablePosition.Where(x => x == param.SelectedPosition).Count() == 0)
        //        throw new BadRequestException($"You dont assign as {param.SelectedPosition}.");
        //    var idLevelPrincipalAndVicePrincipal = new List<string>();
        //    var IdHomerooms = new List<ItemValueVm>();
        //    if (positionUser.Count > 0)
        //    {
        //        if (param.SelectedPosition == PositionConstant.Principal)
        //        {
        //            if (positionUser.Any(y => y.Code == PositionConstant.Principal)) //check P Or VP
        //                if (positionUser.Where(y => y.Code == PositionConstant.Principal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.Principal).Count() > 0)
        //                {
        //                    var Principal = positionUser.Where(x => x.Code == PositionConstant.Principal).ToList();

        //                    foreach (var item in Principal)
        //                    {
        //                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                        _dataNewLH.TryGetValue("Level", out var _levelLH);
        //                        if (_levelLH.Id == param.IdLevel)
        //                        {
        //                            IdHomerooms = await _dbContext.Entity<MsHomeroom>()
        //                                .Include(x => x.Grade)
        //                                    .ThenInclude(x => x.Level)
        //                                        .ThenInclude(x => x.AcademicYear)
        //                                .Include(x => x.GradePathwayClassroom)
        //                                    .ThenInclude(x => x.Classroom)
        //                                .Where(x => x.Grade.IdLevel == param.IdLevel)
        //                                .Where(x => x.IdGrade == param.IdGrade)
        //                                .Where(x => x.Semester == param.Semester)
        //                                .OrderBy(x => x.Grade.Level.AcademicYear.OrderNumber)
        //                                    .ThenBy(x => x.Grade.Level.OrderNumber)
        //                                        .ThenBy(x => x.Grade.OrderNumber)
        //                                            .ThenBy(x => x.GradePathwayClassroom.Classroom.Code)
        //                            .Select(x => new ItemValueVm
        //                            {
        //                                Id = x.Id,
        //                                Description = x.Grade.Code + x.GradePathwayClassroom.Classroom.Code
        //                            }).ToListAsync(CancellationToken);
        //                        }

        //                    }
        //                }
        //        }

        //        if (param.SelectedPosition == PositionConstant.VicePrincipal)
        //        {
        //            if (positionUser.Any(y => y.Code == PositionConstant.VicePrincipal))
        //            {
        //                if (positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).Count() > 0)
        //                {
        //                    var Principal = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();
        //                    var IdLevels = new List<string>();
        //                    foreach (var item in Principal)
        //                    {
        //                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                        _dataNewLH.TryGetValue("Level", out var _levelLH);
        //                        if (_levelLH.Id == param.IdLevel)
        //                        {
        //                            IdHomerooms = await _dbContext.Entity<MsHomeroom>()
        //                                .Include(x => x.Grade)
        //                                    .ThenInclude(x => x.Level)
        //                                .Include(x => x.GradePathwayClassroom)
        //                                    .ThenInclude(x => x.Classroom)
        //                                .Where(x => x.Grade.IdLevel == param.IdLevel)
        //                                .Where(x => x.IdGrade == param.IdGrade)
        //                                .Where(x => x.Semester == param.Semester)
        //                                .OrderBy(x => x.Grade.Level.AcademicYear.OrderNumber)
        //                                    .ThenBy(x => x.Grade.Level.OrderNumber)
        //                                        .ThenBy(x => x.Grade.OrderNumber)
        //                                            .ThenBy(x => x.GradePathwayClassroom.Classroom.Code)
        //                            .Select(x => new ItemValueVm
        //                            {
        //                                Id = x.Id,
        //                                Description = x.Grade.Code + x.GradePathwayClassroom.Classroom.Code
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
        //                var IdGrade = new List<string>();
        //                foreach (var item in LevelHead)
        //                {
        //                    var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                    _dataNewLH.TryGetValue("Level", out var _levelLH);
        //                    if (_levelLH.Id == param.IdLevel)
        //                    {
        //                        _dataNewLH.TryGetValue("Grade", out var _gradeLH);
        //                        if (_gradeLH.Id == param.IdGrade)
        //                        {
        //                            IdHomerooms = await _dbContext.Entity<MsHomeroom>()
        //                                .Include(x => x.Grade)
        //                                    .ThenInclude(x => x.Level)
        //                                .Include(x => x.GradePathwayClassroom)
        //                                    .ThenInclude(x => x.Classroom)
        //                                .Where(x => x.Grade.IdLevel == param.IdLevel)
        //                                .Where(x => x.IdGrade == param.IdGrade)
        //                                .Where(x => x.Semester == param.Semester)
        //                                .OrderBy(x => x.Grade.Level.AcademicYear.OrderNumber)
        //                                    .ThenBy(x => x.Grade.Level.OrderNumber)
        //                                        .ThenBy(x => x.Grade.OrderNumber)
        //                                            .ThenBy(x => x.GradePathwayClassroom.Classroom.Code)
        //                            .Select(x => new ItemValueVm
        //                            {
        //                                Id = x.Id,
        //                                Description = x.Grade.Code + x.GradePathwayClassroom.Classroom.Code
        //                            }).ToListAsync(CancellationToken);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (param.SelectedPosition == PositionConstant.SubjectHead)
        //        {
        //            if (positionUser.Where(y => y.Code == PositionConstant.SubjectHead).ToList() != null)
        //            {
        //                var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHead).ToList();
        //                var IdGrade = new List<string>();
        //                var IdSubject = new List<string>();
        //                foreach (var item in LevelHead)
        //                {
        //                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                    _dataNewSH.TryGetValue("Level", out var _leveltSH);
        //                    if (_leveltSH.Id == param.IdLevel)
        //                    {
        //                        _dataNewSH.TryGetValue("Grade", out var _gradeSH);
        //                        if (_gradeSH.Id == param.IdGrade)
        //                        {
        //                            IdHomerooms = await _dbContext.Entity<MsHomeroom>()
        //                                   .Include(x => x.Grade)
        //                                       .ThenInclude(x => x.Level)
        //                                            .ThenInclude(x => x.MappingAttendances)
        //                                   .Include(x => x.GradePathwayClassroom)
        //                                       .ThenInclude(x => x.Classroom)
        //                                   .Where(x => x.Grade.IdLevel == param.IdLevel)
        //                                   .Where(x => x.IdGrade == param.IdGrade)
        //                                   .Where(x => x.Semester == param.Semester)
        //                                   .Where(x=>x.Grade.Level.MappingAttendances.Any(y=>y.AbsentTerms == Common.Model.Enums.AbsentTerm.Session))
        //                                   .OrderBy(x => x.Grade.Level.AcademicYear.OrderNumber)
        //                                    .ThenBy(x => x.Grade.Level.OrderNumber)
        //                                        .ThenBy(x => x.Grade.OrderNumber)
        //                                            .ThenBy(x => x.GradePathwayClassroom.Classroom.Code)
        //                               .Select(x => new ItemValueVm
        //                               {
        //                                   Id = x.Id,
        //                                   Description = x.Grade.Code + x.GradePathwayClassroom.Classroom.Code
        //                               }).ToListAsync(CancellationToken);
        //                        }

        //                    }
        //                }
        //            }
        //        }

        //        if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
        //        {
        //            if (positionUser.Where(y => y.Code == PositionConstant.SubjectHeadAssitant).ToList() != null)
        //            {
        //                var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHeadAssitant).ToList();
        //                var IdGrade = new List<string>();
        //                var IdSubject = new List<string>();
        //                foreach (var item in LevelHead)
        //                {
        //                    var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                    _dataNewSH.TryGetValue("Level", out var _leveltSH);
        //                    if (_leveltSH.Id == param.IdLevel)
        //                    {
        //                        _dataNewSH.TryGetValue("Grade", out var _gradeSH);
        //                        if (_gradeSH.Id == param.IdGrade)
        //                        {
        //                            IdHomerooms = await _dbContext.Entity<MsHomeroom>()
        //                                  .Include(x => x.Grade)
        //                                      .ThenInclude(x => x.Level)
        //                                           .ThenInclude(x => x.MappingAttendances)
        //                                  .Include(x => x.GradePathwayClassroom)
        //                                      .ThenInclude(x => x.Classroom)
        //                                  .Where(x => x.Grade.IdLevel == param.IdLevel)
        //                                  .Where(x => x.IdGrade == param.IdGrade)
        //                                  .Where(x => x.Semester == param.Semester)
        //                                  .Where(x => x.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == Common.Model.Enums.AbsentTerm.Session))
        //                                  .OrderBy(x => x.Grade.Level.AcademicYear.OrderNumber)
        //                                   .ThenBy(x => x.Grade.Level.OrderNumber)
        //                                       .ThenBy(x => x.Grade.OrderNumber)
        //                                           .ThenBy(x => x.GradePathwayClassroom.Classroom.Code)
        //                              .Select(x => new ItemValueVm
        //                              {
        //                                  Id = x.Id,
        //                                  Description = x.Grade.Code + x.GradePathwayClassroom.Classroom.Code
        //                              }).ToListAsync(CancellationToken);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (param.SelectedPosition == PositionConstant.HeadOfDepartment)
        //        {
        //            if (positionUser.Where(y => y.Code == PositionConstant.HeadOfDepartment).ToList() != null)
        //            {
        //                var HOD = positionUser.Where(x => x.Code == PositionConstant.HeadOfDepartment).ToList();
        //                var idDepartment = new List<string>();
        //                var IdGrade = new List<string>();
        //                var IdSubject = new List<string>();
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

        //                IdHomerooms = await _dbContext.Entity<MsHomeroom>()
        //                    .Include(x => x.Grade)
        //                        .ThenInclude(x => x.Level)
        //                    .Include(x => x.GradePathwayClassroom)
        //                        .ThenInclude(x => x.Classroom)
        //                    .Where(x => x.Grade.IdLevel == param.IdLevel)
        //                    .Where(x => x.IdGrade == param.IdGrade)
        //                    .Where(x => x.Semester == param.Semester)
        //                    .OrderBy(x => x.Grade.Level.AcademicYear.OrderNumber)
        //                        .ThenBy(x => x.Grade.Level.OrderNumber)
        //                            .ThenBy(x => x.Grade.OrderNumber)
        //                                .ThenBy(x => x.GradePathwayClassroom.Classroom.Code)
        //                .Select(x => new ItemValueVm
        //                {
        //                    Id = x.Id,
        //                    Description = x.Grade.Code + x.GradePathwayClassroom.Classroom.Code
        //                }).ToListAsync(CancellationToken);
        //                //foreach (var department in departments)
        //                //    if (department.Type == DepartmentType.Level)

        //                //        foreach (var departmentLevel in department.DepartmentLevels)
        //                //            if (departmentLevel.IdLevel == param.IdLevel)
        //                //                IdHomerooms = await _dbContext.Entity<MsHomeroom>()
        //                //                  .Include(x => x.Grade)
        //                //                      .ThenInclude(x => x.Level)
        //                //                  .Include(x => x.GradePathwayClassroom)
        //                //                      .ThenInclude(x => x.Classroom)
        //                //                  .Where(x => x.Grade.IdLevel == param.IdLevel)
        //                //                  .Where(x => x.IdGrade == param.IdGrade)
        //                //                  .Where(x => x.Semester == param.Semester)
        //                //                  .OrderBy(x => x.Grade.Level.AcademicYear.OrderNumber)
        //                //                    .ThenBy(x => x.Grade.Level.OrderNumber)
        //                //                        .ThenBy(x => x.Grade.OrderNumber)
        //                //                            .ThenBy(x => x.GradePathwayClassroom.Classroom.Code)
        //                //              .Select(x => new ItemValueVm
        //                //              {
        //                //                  Id = x.Id,
        //                //                  Description = x.Grade.Code + x.GradePathwayClassroom.Classroom.Code
        //                //              }).ToListAsync(CancellationToken);
        //                //            else
        //                //                IdHomerooms = await _dbContext.Entity<MsHomeroom>()
        //                //                  .Include(x => x.Grade)
        //                //                      .ThenInclude(x => x.Level)
        //                //                  .Include(x => x.GradePathwayClassroom)
        //                //                      .ThenInclude(x => x.Classroom)
        //                //                  .Where(x => x.Grade.IdLevel == param.IdLevel)
        //                //                  .Where(x => x.Semester == param.Semester)
        //                //                  .OrderBy(x => x.Grade.Level.AcademicYear.OrderNumber)
        //                //                    .ThenBy(x => x.Grade.Level.OrderNumber)
        //                //                        .ThenBy(x => x.Grade.OrderNumber)
        //                //                            .ThenBy(x => x.GradePathwayClassroom.Classroom.Code)
        //                //              .Select(x => new ItemValueVm
        //                //              {
        //                //                  Id = x.Id,
        //                //                  Description = x.Grade.Code + x.GradePathwayClassroom.Classroom.Code
        //                //              }).ToListAsync(CancellationToken);
        //            }
        //        }
        //        if (param.SelectedPosition == PositionConstant.ClassAdvisor)
        //            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
        //                IdHomerooms = dataHomeroomTeacher
        //                .Where(x => x.Homeroom.IdGrade == param.IdGrade)
        //                .Where(x => x.Homeroom.Grade.IdLevel == param.IdLevel)
        //                .Where(x => x.Homeroom.Semester == param.Semester)
        //                .GroupBy(x => new
        //                {
        //                    x.Homeroom.Id,
        //                    grade = x.Homeroom.Grade.Code,
        //                    classroom = x.Homeroom.GradePathwayClassroom.Classroom.Code
        //                })
        //                .Select(x => new ItemValueVm
        //                {
        //                    Id = x.Key.Id,
        //                    Description = x.Key.grade + x.Key.classroom
        //                }).ToList();

        //        if (param.SelectedPosition == PositionConstant.SubjectTeacher)
        //            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
        //            {
        //                var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
        //                IdHomerooms = await _dbContext.Entity<MsLessonPathway>()
        //                    .Include(x => x.HomeroomPathway)
        //                        .ThenInclude(x => x.Homeroom)
        //                            .ThenInclude(x => x.Grade)
        //                    .Include(x => x.HomeroomPathway)
        //                        .ThenInclude(x => x.Homeroom)
        //                            .ThenInclude(x => x.GradePathwayClassroom)
        //                                .ThenInclude(x => x.Classroom)
        //                    .Where(x => idLessons.Contains(x.IdLesson))
        //                    .OrderBy(x => x.HomeroomPathway.Homeroom.Grade.Level.AcademicYear.OrderNumber)
        //                        .ThenBy(x => x.HomeroomPathway.Homeroom.Grade.Level.OrderNumber)
        //                            .ThenBy(x => x.HomeroomPathway.Homeroom.Grade.OrderNumber)
        //                                .ThenBy(x => x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
        //                    .GroupBy(x => new
        //                    {
        //                        homeroom = x.HomeroomPathway.Homeroom.Id,
        //                        grade = x.HomeroomPathway.Homeroom.Grade.Code,
        //                        classroom = x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code
        //                    })
        //                    .Select(x => new ItemValueVm
        //                    {
        //                        Id = x.Key.homeroom,
        //                        Description = x.Key.grade + x.Key.classroom
        //                    }).ToListAsync(CancellationToken);
        //            }
        //    }
        //    else
        //    {
        //        if (param.SelectedPosition == PositionConstant.ClassAdvisor)
        //            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
        //                IdHomerooms = dataHomeroomTeacher
        //                .Where(x => x.Homeroom.IdGrade == param.IdGrade)
        //                .Where(x => x.Homeroom.Grade.IdLevel == param.IdLevel)
        //                .Where(x => x.Homeroom.Semester == param.Semester)
        //                .GroupBy(x => new
        //                {
        //                    x.Homeroom.Id,
        //                    grade = x.Homeroom.Grade.Code,
        //                    classroom = x.Homeroom.GradePathwayClassroom.Classroom.Code
        //                })
        //                .Select(x => new ItemValueVm
        //                {
        //                    Id = x.Key.Id,
        //                    Description = x.Key.grade + x.Key.classroom
        //                }).ToList();

        //        if (param.SelectedPosition == PositionConstant.SubjectTeacher)
        //            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
        //            {
        //                var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
        //                IdHomerooms = await _dbContext.Entity<MsLessonPathway>()
        //                    .Include(x => x.HomeroomPathway)
        //                        .ThenInclude(x => x.Homeroom)
        //                            .ThenInclude(x => x.Grade)
        //                    .Include(x => x.HomeroomPathway)
        //                        .ThenInclude(x => x.Homeroom)
        //                            .ThenInclude(x => x.GradePathwayClassroom)
        //                                .ThenInclude(x => x.Classroom)
        //                    .Where(x => idLessons.Contains(x.IdLesson))
        //                    .OrderBy(x => x.HomeroomPathway.Homeroom.Grade.Level.AcademicYear.OrderNumber)
        //                        .ThenBy(x => x.HomeroomPathway.Homeroom.Grade.Level.OrderNumber)
        //                            .ThenBy(x => x.HomeroomPathway.Homeroom.Grade.OrderNumber)
        //                                .ThenBy(x => x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
        //                    .GroupBy(x => new
        //                    {
        //                        homeroom = x.HomeroomPathway.Homeroom.Id,
        //                        grade = x.HomeroomPathway.Homeroom.Grade.Code,
        //                        classroom = x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code
        //                    })
        //                    .Select(x => new ItemValueVm
        //                    {
        //                        Id = x.Key.homeroom,
        //                        Description = x.Key.grade + x.Key.classroom
        //                    }).ToListAsync(CancellationToken);
        //            }
        //    }
        //    #endregion
        //    return Request.CreateApiResult2(IdHomerooms as object);
        //}
    }
}

