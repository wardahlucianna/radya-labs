using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnSchedule.RolePosition.Validator;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Newtonsoft.Json;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Common.Model.Enums;
//using BinusSchool.Scheduling.FnSchedule.Timer;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnSchedule.RolePosition
{
    public class GetUserSubjectByEmailRecepientHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly ILogger<GetUserSubjectByEmailRecepientHandler> _logger;
        public GetUserSubjectByEmailRecepientHandler(ISchedulingDbContext DbContext, ILogger<GetUserSubjectByEmailRecepientHandler> logger)
        {
            _dbContext = DbContext;
            _logger = logger;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetUserSubjectByEmailRecepientRequest, GetUserSubjectByEmailRecepientValidator>();

            #region GetData
            var listRole = await _dbContext.Entity<LtRole>()
                                            .Include(e=>e.RoleGroup)
                                            .Where(x => x.IdSchool == body.IdSchool
                                                    && (x.RoleGroup.Code == RoleConstant.Staff || x.RoleGroup.Code == RoleConstant.Teacher))
                                            .ToListAsync(CancellationToken);

            var listRolePosition = await _dbContext.Entity<TrRolePosition>()
                                            .Include(e=>e.TeacherPosition).ThenInclude(e=>e.Position)
                                            .Where(x => x.TeacherPosition.IdSchool == body.IdSchool
                                                    && (x.Role.RoleGroup.Code == RoleConstant.Staff || x.Role.RoleGroup.Code == RoleConstant.Teacher))
                                            .ToListAsync(CancellationToken);

            var listUserRole = await _dbContext.Entity<MsUserRole>()
                                .Include(e => e.Role).ThenInclude(e => e.RoleGroup)
                                .Where(x => x.Role.IdSchool == body.IdSchool && x.Role.RoleGroup.Code == RoleConstant.Staff)
                                .ToListAsync(CancellationToken);

            var listNonTeachingLoad = await _dbContext.Entity<TrNonTeachingLoad>()
                                       .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition)
                                       .Where(x => x.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear && x.Data!=null)
                                       .ToListAsync(CancellationToken);

            var listLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                        .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                                        .Where(x => x.Lesson.IdAcademicYear == body.IdAcademicYear)
                                        .ToListAsync(CancellationToken);

            var listHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                                        .Where(x => x.Homeroom.IdAcademicYear == body.IdAcademicYear)
                                        .ToListAsync(CancellationToken);

            var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                .Include(e => e.Level).ThenInclude(e => e.MsGrades)
                                 .Where(x => x.Level.IdAcademicYear == body.IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            var listTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                .Include(e => e.Position)
                                 .Where(x => x.IdSchool == body.IdSchool)
                                 .ToListAsync(CancellationToken);

            var listLessonPathway = await _dbContext.Entity<MsLessonPathway>()
                                        .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                                        .Include(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                        .Include(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                        .Where(e => e.HomeroomPathway.Homeroom.IdAcademicYear == body.IdAcademicYear)
                                        .GroupBy(e => new
                                        {
                                            IdLesson = e.IdLesson,
                                            IdSubject = e.Lesson.IdSubject,
                                            Subject = e.Lesson.Subject.Description,
                                            IdDepartmentSubject = e.Lesson.Subject.IdDepartment,
                                            SubjectCode = e.Lesson.Subject.Code,
                                            IdHomeroom = e.HomeroomPathway.IdHomeroom,
                                            Homeroom = e.HomeroomPathway.Homeroom.Grade.Code
                                                    + e.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                                            Semester = e.HomeroomPathway.Homeroom.Semester,
                                            IdGrade = e.HomeroomPathway.Homeroom.Grade.Id,
                                            Grade = e.HomeroomPathway.Homeroom.Grade.Description,
                                            GradeCode = e.HomeroomPathway.Homeroom.Grade.Code,
                                            GradeOrder = e.HomeroomPathway.Homeroom.Grade.OrderNumber,
                                            IdLevel = e.HomeroomPathway.Homeroom.Grade.Level.Id,
                                            Level = e.HomeroomPathway.Homeroom.Grade.Level.Description,
                                            LevelCode = e.HomeroomPathway.Homeroom.Grade.Level.Code,
                                            LevelOrder = e.HomeroomPathway.Homeroom.Grade.Level.OrderNumber,
                                            IdGradePathwayClassRoom = e.HomeroomPathway.Homeroom.IdGradePathwayClassRoom,
                                            IdGradePathway = e.HomeroomPathway.Homeroom.IdGradePathway,
                                            ClassId = e.Lesson.ClassIdGenerated,
                                        })
                                        .Select(e => new GetSubjectByUserResult
                                        {
                                            Lesson = new GetSubjectLesson
                                            {
                                                Id = e.Key.IdLesson,
                                                ClassId = e.Key.ClassId
                                            },
                                            Subject = new GetSubjectUser
                                            {
                                                Id = e.Key.IdSubject,
                                                Description = e.Key.Subject,
                                                Code = e.Key.SubjectCode,
                                                IdDepartment = e.Key.IdDepartmentSubject
                                            },
                                            Homeroom = new GetSubjectHomeroom
                                            {
                                                Id = e.Key.IdHomeroom,
                                                Description = e.Key.Homeroom,
                                                Semester = e.Key.Semester,
                                                IdGradePathway = e.Key.IdGradePathway,
                                                IdGradePathwayClassRoom = e.Key.IdGradePathwayClassRoom
                                            },
                                            Grade = new ItemValueVmWithOrderNumber
                                            {
                                                Id = e.Key.IdGrade,
                                                Description = e.Key.Grade,
                                                Code = e.Key.GradeCode,
                                                OrderNumber = e.Key.GradeOrder
                                            },
                                            Level = new ItemValueVmWithOrderNumber
                                            {
                                                Id = e.Key.IdLevel,
                                                Description = e.Key.Level,
                                                Code = e.Key.LevelCode,
                                                OrderNumber = e.Key.LevelOrder
                                            },
                                        })
                                        .ToListAsync(CancellationToken);

            //var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
            //                            .Include(e => e.Lesson).ThenInclude(e => e.Subject)
            //                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
            //                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
            //                            .Where(e => e.HomeroomStudent.Homeroom.IdAcademicYear == body.IdAcademicYear)
            //                            .GroupBy(e => new
            //                            {
            //                                IdStudent = e.HomeroomStudent.IdStudent,
            //                                IdLesson = e.IdLesson,
            //                                IdSubject = e.Lesson.IdSubject,
            //                                Subject = e.Lesson.Subject.Description,
            //                                IdDepartmentSubject = e.Lesson.Subject.IdDepartment,
            //                                SubjectCode = e.Lesson.Subject.Code,
            //                                IdHomeroom = e.HomeroomStudent.IdHomeroom,
            //                                Homeroom = e.HomeroomStudent.Homeroom.Grade.Code
            //                                        + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
            //                                Semester = e.HomeroomStudent.Homeroom.Semester,
            //                                IdGrade = e.HomeroomStudent.Homeroom.Grade.Id,
            //                                Grade = e.HomeroomStudent.Homeroom.Grade.Description,
            //                                GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
            //                                GradeOrder = e.HomeroomStudent.Homeroom.Grade.OrderNumber,
            //                                IdLevel = e.HomeroomStudent.Homeroom.Grade.Level.Id,
            //                                Level = e.HomeroomStudent.Homeroom.Grade.Level.Description,
            //                                LevelCode = e.HomeroomStudent.Homeroom.Grade.Level.Code,
            //                                LevelOrder = e.HomeroomStudent.Homeroom.Grade.Level.OrderNumber,
            //                                IdGradePathwayClassRoom = e.HomeroomStudent.Homeroom.IdGradePathwayClassRoom,
            //                                IdGradePathway = e.HomeroomStudent.Homeroom.IdGradePathway,
            //                                ClassId = e.Lesson.ClassIdGenerated,
            //                            })
            //                            .Select(e => new GetUserSubjectByEmailRecepientResult
            //                            {
            //                                IdUser = e.Key.IdStudent,
            //                                Lesson = new GetSubjectLesson
            //                                {
            //                                    Id = e.Key.IdLesson,
            //                                    ClassId = e.Key.ClassId
            //                                },
            //                                Subject = new GetSubjectUser
            //                                {
            //                                    Id = e.Key.IdSubject,
            //                                    Description = e.Key.Subject,
            //                                    Code = e.Key.SubjectCode,
            //                                    IdDepartment = e.Key.IdDepartmentSubject
            //                                },
            //                                Homeroom = new GetSubjectHomeroom
            //                                {
            //                                    Id = e.Key.IdHomeroom,
            //                                    Description = e.Key.Homeroom,
            //                                    Semester = e.Key.Semester,
            //                                    IdGradePathway = e.Key.IdGradePathway,
            //                                    IdGradePathwayClassRoom = e.Key.IdGradePathwayClassRoom
            //                                },
            //                                Grade = new ItemValueVmWithOrderNumber
            //                                {
            //                                    Id = e.Key.IdGrade,
            //                                    Description = e.Key.Grade,
            //                                    Code = e.Key.GradeCode,
            //                                    OrderNumber = e.Key.GradeOrder
            //                                },
            //                                Level = new ItemValueVmWithOrderNumber
            //                                {
            //                                    Id = e.Key.IdLevel,
            //                                    Description = e.Key.Level,
            //                                    Code = e.Key.LevelCode,
            //                                    OrderNumber = e.Key.LevelOrder
            //                                },
            //                            })
            //                            .ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(body.IdUser))
            {
                var idUser = body.IdUser;
                listUserRole = listUserRole.Where(e => e.IdUser == idUser).ToList();
            }
            #endregion

            List<GetUserSubjectByEmailRecepientResult> listUserLessonSubject = new List<GetUserSubjectByEmailRecepientResult>();
            foreach (var item in body.EmailRecepients)
            {
                if (!string.IsNullOrEmpty(item.IdRole))
                {
                    var listRoleByRole = listRole.Where(e => e.Id == item.IdRole).ToList();
                    var listCodeRoleGroup = listRoleByRole.Select(e => e.RoleGroup.Code).Distinct().ToList();

                    if (listCodeRoleGroup.Contains(RoleConstant.Staff))
                    {
                        if (string.IsNullOrEmpty(item.IdTeacherPosition))
                        {
                            var queryUserRoleByStaff = listUserRole.Where(e=>e.IdRole==item.IdRole).Distinct();

                            if (!string.IsNullOrEmpty(item.IdUser))
                                queryUserRoleByStaff = queryUserRoleByStaff.Where(e => e.IdUser == item.IdUser);

                            var listUserRoleByStaff = queryUserRoleByStaff.Select(e=>e.IdUser).Distinct().ToList();

                            foreach(var itemUser in listUserRoleByStaff)
                            {
                                var listUserByStaff = listLessonPathway
                                        .Select(e => new GetUserSubjectByEmailRecepientResult
                                        {
                                            IdUser = itemUser,
                                            IdTeacherPosition = null,
                                            Level = e.Level,
                                            Grade = e.Grade,
                                            Homeroom = e.Homeroom,
                                            Lesson = e.Lesson,
                                            Subject = e.Subject
                                        })
                                        .ToList();

                                listUserLessonSubject.AddRange(listUserByStaff);
                            }

                            
                        }
                        else
                        {
                            var listIdTeacherPosition = listRolePosition.Where(e=>e.IdTeacherPosition==item.IdTeacherPosition).Select(e => e.IdTeacherPosition).ToList();

                            var paramNonTeachingLoad = new GetNonTeachingLoad
                            {
                                ListIdTeacherPosition = listIdTeacherPosition,
                                ListNonTeachingLoad = listNonTeachingLoad,
                                ListDepartmentLevel = listDepartmentLevel,
                                ListLessonPathway = listLessonPathway,
                                Department = new List<string>(),
                            };

                            var listIdUserByNonTeaching = GetNonTeachingLoad(paramNonTeachingLoad);

                            if (!string.IsNullOrEmpty(item.IdUser))
                                listIdUserByNonTeaching = listIdUserByNonTeaching.Where(e => e.IdUser == item.IdUser).ToList();

                            listUserLessonSubject.AddRange(listIdUserByNonTeaching);
                        }
                    }
                    else if (listCodeRoleGroup.Contains(RoleConstant.Teacher))
                    {
                        var listTeacherPositionById = listRolePosition.ToList();

                        if (!string.IsNullOrEmpty(item.IdTeacherPosition))
                            listTeacherPositionById = listTeacherPositionById.Where(e => e.IdTeacherPosition == item.IdTeacherPosition).ToList();

                        #region subject Teacher
                        var listIdTeacherPositionBySt = listTeacherPositionById.Where(e=>e.TeacherPosition.Position.Code==PositionConstant.SubjectTeacher).Select(e=>e.Id).ToList();

                        foreach(var itemIdTeacherPosition in listIdTeacherPositionBySt)
                        {
                            foreach (var itemLessonTeacher in listLessonTeacher)
                            {

                                var listUserByLessonTeacher = listLessonPathway
                                                                .Where(e => e.Lesson.Id == itemLessonTeacher.IdLesson)
                                                                .Select(e => new GetUserSubjectByEmailRecepientResult
                                                                {
                                                                    IdUser = itemLessonTeacher.IdUser,
                                                                    IdTeacherPosition = itemIdTeacherPosition,
                                                                    Grade = e.Grade,
                                                                    Lesson = e.Lesson,
                                                                    Homeroom = e.Homeroom,
                                                                    Level = e.Level,
                                                                    Subject = e.Subject,
                                                                }).ToList();

                                listUserLessonSubject.AddRange(listUserByLessonTeacher);

                            }
                        }
                        #endregion

                        #region Homeroom Teacher
                        var listIdTeacherPositionByCA = listTeacherPositionById.Where(e => e.TeacherPosition.Position.Code == PositionConstant.ClassAdvisor).Select(e => e.Id).ToList();

                        foreach (var itemIdTeacherPosition in listIdTeacherPositionByCA)
                        {
                            foreach (var itemHomeroomTeacher in listHomeroomTeacher)
                            {
                                var listUserByLessonTeacher = listLessonPathway
                                                                .Where(e => e.Homeroom.Id == itemHomeroomTeacher.IdHomeroom)
                                                                .Select(e => new GetUserSubjectByEmailRecepientResult
                                                                {
                                                                    IdUser = itemHomeroomTeacher.IdBinusian,
                                                                    IdTeacherPosition = itemIdTeacherPosition,
                                                                    Grade = e.Grade,
                                                                    Lesson = e.Lesson,
                                                                    Homeroom = e.Homeroom,
                                                                    Level = e.Level,
                                                                    Subject = e.Subject,
                                                                }).ToList();

                                listUserLessonSubject.AddRange(listUserByLessonTeacher);
                            }
                        }
                        #endregion

                        #region non teaching load
                        var listIdTeacherPosition = listTeacherPositionById.Select(e => e.IdTeacherPosition).Distinct().ToList();
                        var paramNonTeachingLoad = new GetNonTeachingLoad
                        {
                            ListIdTeacherPosition = listIdTeacherPosition,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListLessonPathway = listLessonPathway,
                            Department = new List<string>(),
                        };

                        var listUserByNonTeaching = GetNonTeachingLoad(paramNonTeachingLoad);
                        listUserLessonSubject.AddRange(listUserByNonTeaching);
                        #endregion

                        if (!string.IsNullOrEmpty(item.IdUser))
                            listUserLessonSubject = listUserLessonSubject.Where(e => e.IdUser == item.IdUser).ToList();
                    }
                    //else if (listCodeRoleGroup.Contains(RoleConstant.Student))
                    //{
                    //    var listUserByStudent = listHomeroomStudent
                    //                                    .Select(e => new GetUserSubjectByEmailRecepientResult
                    //                                    {
                    //                                        IdUser = e.IdUser,
                    //                                        Grade = e.Grade,
                    //                                        Lesson = e.Lesson,
                    //                                        Homeroom = e.Homeroom,
                    //                                        Level = e.Level,
                    //                                        Subject = e.Subject,
                    //                                    }).ToList();

                    //    listUserLessonSubject.AddRange(listUserByStudent);

                    //    if (!string.IsNullOrEmpty(item.IdUser))
                    //        listUserLessonSubject = listUserLessonSubject.Where(e => e.IdUser == item.IdUser).ToList();
                    //}
                }
            }

            if (!string.IsNullOrEmpty(body.IdUser))
                listUserLessonSubject = listUserLessonSubject.Where(e => e.IdUser == body.IdUser).ToList();

            if (body.IsShowIdUser)
                listUserLessonSubject = listUserLessonSubject
                            .GroupBy(e => new
                            {
                                IdUser = e.IdUser
                            })
                            .Select(e => new GetUserSubjectByEmailRecepientResult
                            {
                                IdUser = e.Key.IdUser,
                            })
                            .ToList();
            else
                listUserLessonSubject = listUserLessonSubject
                            .GroupBy(e => new
                            {
                                IdLesson = e.Lesson.Id,
                                IdSubject = e.Subject.Id,
                                Subject = e.Subject.Description,
                                IdDepartmentSubject = e.Subject.IdDepartment,
                                SubjectCode = e.Subject.Code,
                                IdHomeroom = e.Homeroom.Id,
                                Homeroom = e.Homeroom.Description,
                                Semester = e.Homeroom.Semester,
                                IdGrade = e.Grade.Id,
                                Grade = e.Grade.Description,
                                GradeCode = e.Grade.Code,
                                GradeOrder = e.Grade.OrderNumber,
                                IdLevel = e.Level.Id,
                                Level = e.Level.Description,
                                LevelCode = e.Level.Code,
                                LevelOrder = e.Level.OrderNumber,
                                IdGradePathwayClassRoom = e.Homeroom.IdGradePathwayClassRoom,
                                IdGradePathway = e.Homeroom.IdGradePathway,
                                ClassId = e.Lesson.ClassId,
                            })
                            .Select(e => new GetUserSubjectByEmailRecepientResult
                            {
                                Lesson = new GetSubjectLesson
                                {
                                    Id = e.Key.IdLesson,
                                    ClassId = e.Key.ClassId
                                },
                                Subject = new GetSubjectUser
                                {
                                    Id = e.Key.IdSubject,
                                    Description = e.Key.Subject,
                                    Code = e.Key.SubjectCode,
                                    IdDepartment = e.Key.IdDepartmentSubject
                                },
                                Homeroom = new GetSubjectHomeroom
                                {
                                    Id = e.Key.IdHomeroom,
                                    Description = e.Key.Homeroom,
                                    Semester = e.Key.Semester,
                                    IdGradePathway = e.Key.IdGradePathway,
                                    IdGradePathwayClassRoom = e.Key.IdGradePathwayClassRoom
                                },
                                Grade = new ItemValueVmWithOrderNumber
                                {
                                    Id = e.Key.IdGrade,
                                    Description = e.Key.Grade,
                                    Code = e.Key.GradeCode,
                                    OrderNumber = e.Key.GradeOrder
                                },
                                Level = new ItemValueVmWithOrderNumber
                                {
                                    Id = e.Key.IdLevel,
                                    Description = e.Key.Level,
                                    Code = e.Key.LevelCode,
                                    OrderNumber = e.Key.LevelOrder
                                },
                            })
                            .ToList();

            return Request.CreateApiResult2(listUserLessonSubject as object);
        }

        protected List<GetUserSubjectByEmailRecepientResult> GetNonTeachingLoad(GetNonTeachingLoad param)
        {
            List<GetUserSubjectByEmailRecepientResult> listUserNonteachingLoad = new List<GetUserSubjectByEmailRecepientResult>();

            var listNonTeachingLoad = param.ListNonTeachingLoad
                                        .Where(e => param.ListIdTeacherPosition.Contains(e.MsNonTeachingLoad.IdTeacherPosition))
                                        .Distinct().ToList();

            foreach (var item in listNonTeachingLoad)
            {
                var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
                _dataNewPosition.TryGetValue("Streaming", out var _StreamingPosition);
                _dataNewPosition.TryGetValue("Classroom", out var _ClassroomPosition);

                var _listLessonPathway = param.ListLessonPathway.ToList();
                if (_DepartemenPosition != null)
                {
                    var listIdLevelByDept = param.ListDepartmentLevel.Select(e => e.IdLevel).Distinct().ToList();

                    if (listIdLevelByDept.Any())
                    {
                        _listLessonPathway = _listLessonPathway
                                            .Where(e => listIdLevelByDept.Contains(e.Level.Id) && e.Subject.IdDepartment == _DepartemenPosition.Id)
                                            .ToList();
                    }
                    
                }

                if (_GradePosition != null)
                    _listLessonPathway = _listLessonPathway
                                            .Where(e => e.Grade.Id == _GradePosition.Id)
                                            .ToList();

                if (_LevelPosition != null)
                    _listLessonPathway = _listLessonPathway
                                            .Where(e => e.Level.Id == _LevelPosition.Id)
                                            .ToList();

                if (_SubjectPosition != null)
                    _listLessonPathway = _listLessonPathway
                                            .Where(e => e.Subject.Id == _SubjectPosition.Id)
                                            .ToList();

                if (_StreamingPosition != null)
                    _listLessonPathway = _listLessonPathway
                                            .Where(e => e.Homeroom.IdGradePathway == _StreamingPosition.Id)
                                            .ToList();

                if (_ClassroomPosition != null)
                    _listLessonPathway = _listLessonPathway
                                            .Where(e => e.Homeroom.IdGradePathwayClassRoom == _ClassroomPosition.Id)
                                            .ToList();

                var _listNonteachingLoad = _listLessonPathway
                                        .Select(e => new GetUserSubjectByEmailRecepientResult
                                        {
                                            IdUser = item.IdUser,
                                            IdTeacherPosition = item.MsNonTeachingLoad.IdTeacherPosition,
                                            Level = e.Level,
                                            Grade = e.Grade,
                                            Homeroom = e.Homeroom,
                                            Lesson = e.Lesson,
                                            Subject = e.Subject
                                        })
                                        .ToList();


                listUserNonteachingLoad.AddRange(_listNonteachingLoad);
            }

            if (param.Department.Any())
            {
                var listLevel = param.ListDepartmentLevel.Where(e => param.Department.Contains(e.IdDepartment)).Select(e => e.IdLevel).Distinct().ToList();
                listUserNonteachingLoad = listUserNonteachingLoad.Where(e => param.Department.Contains(e.Subject.IdDepartment)).ToList();
                listUserNonteachingLoad = listUserNonteachingLoad.Where(e => listLevel.Contains(e.Level.Id)).ToList();
            }

            var listUser = listUserNonteachingLoad
                            .GroupBy(e => new
                            {
                                IdUser = e.IdUser,
                                IdTeacherPosition = e.IdTeacherPosition,
                                IdLesson = e.Lesson.Id,
                                IdSubject = e.Subject.Id,
                                Subject = e.Subject.Description,
                                IdDepartmentSubject = e.Subject.IdDepartment,
                                SubjectCode = e.Subject.Code,
                                IdHomeroom = e.Homeroom.Id,
                                Homeroom = e.Homeroom.Description,
                                Semester = e.Homeroom.Semester,
                                IdGrade = e.Grade.Id,
                                Grade = e.Grade.Description,
                                GradeCode = e.Grade.Code,
                                GradeOrder = e.Grade.OrderNumber,
                                IdLevel = e.Level.Id,
                                Level = e.Level.Description,
                                LevelCode = e.Level.Code,
                                LevelOrder = e.Level.OrderNumber,
                                IdGradePathwayClassRoom = e.Homeroom.IdGradePathwayClassRoom,
                                IdGradePathway = e.Homeroom.IdGradePathway,
                                ClassId = e.Lesson.ClassId,
                            })
                            .Select(e => new GetUserSubjectByEmailRecepientResult
                            {
                                IdUser = e.Key.IdUser,
                                IdTeacherPosition = e.Key.IdTeacherPosition,
                                Lesson = new GetSubjectLesson
                                {
                                    Id = e.Key.IdLesson,
                                    ClassId = e.Key.ClassId
                                },
                                Subject = new GetSubjectUser
                                {
                                    Id = e.Key.IdSubject,
                                    Description = e.Key.Subject,
                                    Code = e.Key.SubjectCode,
                                    IdDepartment = e.Key.IdDepartmentSubject
                                },
                                Homeroom = new GetSubjectHomeroom
                                {
                                    Id = e.Key.IdHomeroom,
                                    Description = e.Key.Homeroom,
                                    Semester = e.Key.Semester,
                                    IdGradePathway = e.Key.IdGradePathway,
                                    IdGradePathwayClassRoom = e.Key.IdGradePathwayClassRoom
                                },
                                Grade = new ItemValueVmWithOrderNumber
                                {
                                    Id = e.Key.IdGrade,
                                    Description = e.Key.Grade,
                                    Code = e.Key.GradeCode,
                                    OrderNumber = e.Key.GradeOrder
                                },
                                Level = new ItemValueVmWithOrderNumber
                                {
                                    Id = e.Key.IdLevel,
                                    Description = e.Key.Level,
                                    Code = e.Key.LevelCode,
                                    OrderNumber = e.Key.LevelOrder
                                },
                            })
                            .ToList();



            return listUser;
        }
    }
}
