using System.Threading.Tasks;
using Azure.Core;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnSchedule.RolePosition.Validator;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using System.Linq;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Constants;
using System.Collections.Generic;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using System;
using BinusSchool.Common.Abstractions;
using NPOI.XSSF.UserModel;
using BinusSchool.Data.Api.User.FnUser;
using FluentEmail.Core;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;

namespace BinusSchool.Scheduling.FnSchedule.RolePosition
{
    public class GetUserRolePositionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRedisCache _redisCache;
        public GetUserRolePositionHandler(ISchedulingDbContext dbContext, IRedisCache redisCache)
        {
            _dbContext = dbContext;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetUserRolePositionRequest, GetUserByRolePositionValidator>();

            #region GetData
            var listRolePosition = await _dbContext.Entity<TrRolePosition>()
                                            .Where(x => x.TeacherPosition.IdSchool == body.IdSchool)
                                            .ToListAsync(CancellationToken);

            var listUserRole = await _dbContext.Entity<MsUserRole>()
                                .Include(e => e.Role).ThenInclude(e => e.RoleGroup)
                                .Where(x => x.Role.IdSchool == body.IdSchool && (x.Role.RoleGroup.Code == RoleConstant.Staff || x.Role.RoleGroup.Code == RoleConstant.Teacher))
                                .ToListAsync(CancellationToken);

            var listNonTeachingLoad = await _dbContext.Entity<TrNonTeachingLoad>()
                                       .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition)
                                       .Where(x => x.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear && x.Data != null)
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

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(e => e.Student)
                                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                        .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                        .Where(e => e.Homeroom.IdAcademicYear == body.IdAcademicYear
                                            && e.Student.IdStudentStatus == 1)
                                        .GroupBy(e => new
                                        {
                                            IdHomeroomStudent = e.Id,
                                            IdStudent = e.IdStudent,
                                            IdHomeroom = e.IdHomeroom,
                                            Homeroom = e.Homeroom.Grade.Code
                                                    + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                                            Semester = e.Homeroom.Semester,
                                            IdGrade = e.Homeroom.Grade.Id,
                                            Grade = e.Homeroom.Grade.Description,
                                            GradeCode = e.Homeroom.Grade.Code,
                                            GradeOrder = e.Homeroom.Grade.OrderNumber,
                                            IdLevel = e.Homeroom.Grade.Level.Id,
                                            Level = e.Homeroom.Grade.Level.Description,
                                            LevelCode = e.Homeroom.Grade.Level.Code,
                                            LevelOrder = e.Homeroom.Grade.Level.OrderNumber,
                                        })
                                        .Select(e => new GetStudentRolePositionResult
                                        {
                                            Homeroom = new GetSubjectHomeroom
                                            {
                                                Id = e.Key.IdHomeroom,
                                                Description = e.Key.Homeroom,
                                                Semester = e.Key.Semester,
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
                                            IdStudent = e.Key.IdStudent,
                                            IdHomeroomStudent = e.Key.IdHomeroomStudent
                                        })
                                        .ToListAsync(CancellationToken);

            var listHomeroomStudentAll = listHomeroomStudent;
            if (!string.IsNullOrEmpty(body.IdUser))
            {
                var idUser = body.IdUser;
                var role = "";
                if (body.IdUser.Substring(0, 1) == "P")
                {
                    idUser = body.IdUser.Substring(1);
                    role = UserRolePersonalOptionRole.PARENT.GetDescription();
                }
                else
                {
                    role = UserRolePersonalOptionRole.STUDENT.GetDescription();
                }

                listUserRole = listUserRole.Where(e => e.IdUser == idUser).ToList();
                listNonTeachingLoad = listNonTeachingLoad.Where(e => e.IdUser == idUser).ToList();
                listLessonTeacher = listLessonTeacher.Where(e => e.IdUser == idUser).ToList();
                listHomeroomTeacher = listHomeroomTeacher.Where(e => e.IdBinusian == idUser).ToList();
                listHomeroomStudent = listHomeroomStudent.Where(e => e.IdStudent == idUser).ToList();

                List<UserRolePersonalOptionRole> listRole = new List<UserRolePersonalOptionRole>();
                if (listUserRole.Any())
                    listRole.Add(UserRolePersonalOptionRole.STAFF);

                if (listNonTeachingLoad.Any())
                {
                    listRole.Add(UserRolePersonalOptionRole.STAFF);
                    listRole.Add(UserRolePersonalOptionRole.TEACHER);
                }

                if (listLessonTeacher.Any() || listHomeroomTeacher.Any())
                    listRole.Add(UserRolePersonalOptionRole.TEACHER);

                if (listHomeroomStudent.Any())
                {
                    if (role == UserRolePersonalOptionRole.PARENT.GetDescription())
                        listRole.Add(UserRolePersonalOptionRole.PARENT);
                    else
                        listRole.Add(UserRolePersonalOptionRole.STUDENT);
                }

                if (listRole.Any())
                    listRole.Add(UserRolePersonalOptionRole.ALL);

                body.UserRolePositions = body.UserRolePositions.Where(e => listRole.Contains(e.Role)).ToList();
            }
            #endregion

            List<GetUserRolePositionResult> listUser = new List<GetUserRolePositionResult>();

            foreach (var item in body.UserRolePositions)
            {
                if (item.Option == UserRolePersonalOptionType.None)
                    item.Option = UserRolePersonalOptionType.All;

                if (item.Role == UserRolePersonalOptionRole.STAFF || item.Role == UserRolePersonalOptionRole.ALL)
                {
                    var listUserRoleForStaff = listUserRole.Where(e => e.Role.RoleGroup.Code == RoleConstant.Staff).ToList();
                    if (item.Option == UserRolePersonalOptionType.All)
                    {
                        var param = new GetUserAllRequest
                        {
                            Role = UserRolePersonalOptionRole.STAFF,
                            ListUserRole = listUserRoleForStaff,
                            ListRolePosition = listRolePosition,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListLessonTeacher = listLessonTeacher,
                            ListHomeroomTeacher = listHomeroomTeacher,
                            ListLessonPathway = listLessonPathway,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListTeacherPosition = listTeacherPosition,
                            UserRolePosition = item,
                            ListHomeroomStudent = null,
                            IdUserRolePositions = item.IdUserRolePositions
                        };

                        var _listIdUser = await GetUserAllAsync(param);
                        listUser.AddRange(_listIdUser);
                    }

                    if (item.Option == UserRolePersonalOptionType.Position)
                    {
                        var paramNonTeachingLoad = new GetNonTeachingLoad
                        {
                            ListIdTeacherPosition = item.TeacherPositions,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListLessonPathway = listLessonPathway,
                            Department = new List<string>(),
                            Role = UserRolePersonalOptionRole.STAFF,
                            IdUserRolePositions = item.IdUserRolePositions
                        };

                        var listRoleStaffPosition = await _dbContext.Entity<TrRolePosition>()
                                .Include(e => e.Role).ThenInclude(e => e.RoleGroup)
                                .Where(x => x.Role.IdSchool == body.IdSchool && (x.Role.RoleGroup.Code == RoleConstant.Staff) && item.TeacherPositions.Contains(x.IdTeacherPosition))
                                .Select(x=> x.IdRole)
                                .ToListAsync(CancellationToken);

                        var listUserRoleForStaffPosition = listUserRole.Where(e => e.Role.RoleGroup.Code == RoleConstant.Staff && listRoleStaffPosition.Contains(e.IdRole)).ToList();

                        var param = new GetUserAllRequest
                        {
                            Role = UserRolePersonalOptionRole.STAFF,
                            ListUserRole = listUserRoleForStaffPosition,
                            ListRolePosition = listRolePosition,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListLessonTeacher = listLessonTeacher,
                            ListHomeroomTeacher = listHomeroomTeacher,
                            ListLessonPathway = listLessonPathway,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListTeacherPosition = listTeacherPosition,
                            UserRolePosition = item,
                            ListHomeroomStudent = null,
                            IdUserRolePositions = item.IdUserRolePositions
                        };

                        var _listIdUser = await GetUserAllAsync(param);
                        listUser.AddRange(_listIdUser);
                    }
                    if (item.Option == UserRolePersonalOptionType.Department)
                    {
                        var listIdRole = listUserRoleForStaff
                                            .Select(e => e.IdRole)
                                            .Distinct().ToList();

                        var listIdTeacherPosition = listRolePosition
                                                  .Where(e => listIdRole.Contains(e.IdRole))
                                                  .Select(e => e.IdTeacherPosition)
                                                  .ToList();

                        #region NonTeachingLoad
                        var paramNonTeachingLoad = new GetNonTeachingLoad
                        {
                            ListIdTeacherPosition = listIdTeacherPosition,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListLessonPathway = listLessonPathway,
                            Department = item.Departemens,
                            Role = UserRolePersonalOptionRole.TEACHER,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var listIdUserByNonTeaching = GetNonTeachingLoad(paramNonTeachingLoad);

                        listUser.AddRange(listIdUserByNonTeaching);
                        #endregion
                    }

                    if (item.Option == UserRolePersonalOptionType.Personal)
                    {
                        var newPersonal = item.Personal
                            .Select(e => new GetUserRolePositionResult
                            {
                                IdUserRolePositions = item.IdUserRolePositions,
                                IdUser = e,
                                Role = UserRolePersonalOptionRole.STAFF.GetDescription(),
                            })
                            .ToList();

                        listUser.AddRange(newPersonal);
                    }
                }

                if (item.Role == UserRolePersonalOptionRole.TEACHER || item.Role == UserRolePersonalOptionRole.ALL)
                {
                    var listUserRoleForTeacher = listUserRole.Where(e => e.Role.RoleGroup.Code == RoleConstant.Teacher).ToList();
                    if (item.Option == UserRolePersonalOptionType.All)
                    {
                        var param = new GetUserAllRequest
                        {
                            Role = UserRolePersonalOptionRole.TEACHER,
                            ListUserRole = listUserRoleForTeacher,
                            ListRolePosition = listRolePosition,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListLessonTeacher = listLessonTeacher,
                            ListHomeroomTeacher = listHomeroomTeacher,
                            ListLessonPathway = listLessonPathway,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListTeacherPosition = listTeacherPosition,
                            UserRolePosition = item,
                            ListHomeroomStudent = null,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var _listIdUser = await GetUserAllAsync(param);
                        listUser.AddRange(_listIdUser);
                    }

                    if (item.Option == UserRolePersonalOptionType.Position)
                    {
                        #region Subject Teacher
                        var paramSubjectTeacher = new GetSubjectTeacherRequest
                        {
                            IdTeacherPosition = item.TeacherPositions,
                            ListLessonTeacher = listLessonTeacher,
                            ListTeacherPosition = listTeacherPosition,
                            ListDepartmentLevel = listDepartmentLevel,
                            Department = new List<string>(),
                            Role = UserRolePersonalOptionRole.TEACHER,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var listIdUserBySubjectTeacher = GetSubjectTeacher(paramSubjectTeacher);
                        listUser.AddRange(listIdUserBySubjectTeacher);
                        #endregion

                        #region class Advisor
                        var paramHomeroomTeacher = new GetHomeroomTeacherRequest
                        {
                            IdTeacherPosition = item.TeacherPositions,
                            ListHomeroomTeacher = listHomeroomTeacher,
                            ListTeacherPosition = listTeacherPosition,
                            ListDepartmentLevel = listDepartmentLevel,
                            Department = new List<string>(),
                            Role = UserRolePersonalOptionRole.TEACHER,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var listIdUserByHomeroomTeacher = GetHomeroomTeacher(paramHomeroomTeacher);
                        listUser.AddRange(listIdUserByHomeroomTeacher);
                        #endregion

                        #region NonTeachingLoad
                        var paramNonTeachingLoad = new GetNonTeachingLoad
                        {
                            ListIdTeacherPosition = item.TeacherPositions,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListLessonPathway = listLessonPathway,
                            Department = new List<string>(),
                            Role = UserRolePersonalOptionRole.TEACHER,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var listIdUserByNonTeaching = GetNonTeachingLoad(paramNonTeachingLoad);

                        listUser.AddRange(listIdUserByNonTeaching);
                        #endregion
                    }

                    if (item.Option == UserRolePersonalOptionType.Department)
                    {
                        var listIdRole = listUserRoleForTeacher
                                            .Select(e => e.IdRole)
                                            .Distinct().ToList();

                        var listIdTeacherPosition = listRolePosition
                                                  .Where(e => listIdRole.Contains(e.IdRole))
                                                  .Select(e => e.IdTeacherPosition)
                                                  .ToList();

                        #region Subject Teacher
                        var paramSubjectTeacher = new GetSubjectTeacherRequest
                        {
                            IdTeacherPosition = listIdTeacherPosition,
                            ListLessonTeacher = listLessonTeacher,
                            ListTeacherPosition = listTeacherPosition,
                            ListDepartmentLevel = listDepartmentLevel,
                            Department = item.Departemens,
                            Role = UserRolePersonalOptionRole.TEACHER,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var listIdUserBySubjectTeacher = GetSubjectTeacher(paramSubjectTeacher);
                        listUser.AddRange(listIdUserBySubjectTeacher);
                        #endregion

                        #region class Advisor
                        var paramHomeroomTeacher = new GetHomeroomTeacherRequest
                        {
                            IdTeacherPosition = listIdTeacherPosition,
                            ListHomeroomTeacher = listHomeroomTeacher,
                            ListTeacherPosition = listTeacherPosition,
                            ListDepartmentLevel = listDepartmentLevel,
                            Department = item.Departemens,
                            Role = UserRolePersonalOptionRole.TEACHER,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var listIdUserByHomeroomTeacher = GetHomeroomTeacher(paramHomeroomTeacher);
                        listUser.AddRange(listIdUserByHomeroomTeacher);
                        #endregion

                        #region NonTeachingLoad
                        var paramNonTeachingLoad = new GetNonTeachingLoad
                        {
                            ListIdTeacherPosition = listIdTeacherPosition,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListLessonPathway = listLessonPathway,
                            Department = item.Departemens,
                            Role = UserRolePersonalOptionRole.TEACHER,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var listIdUserByNonTeaching = GetNonTeachingLoad(paramNonTeachingLoad);

                        listUser.AddRange(listIdUserByNonTeaching);
                        #endregion
                    }

                    if (item.Option == UserRolePersonalOptionType.Personal)
                    {
                        var newPersonal = item.Personal
                            .Select(e => new GetUserRolePositionResult
                            {
                                IdUserRolePositions = item.IdUserRolePositions,
                                IdUser = e,
                                Role = UserRolePersonalOptionRole.STAFF.GetDescription(),
                            })
                            .ToList();

                        listUser.AddRange(newPersonal);
                    }
                }

                if (item.Role == UserRolePersonalOptionRole.STUDENT || item.Role == UserRolePersonalOptionRole.ALL)
                {
                    if (item.Option == UserRolePersonalOptionType.All)
                    {
                        var param = new GetUserAllRequest
                        {
                            Role = UserRolePersonalOptionRole.STUDENT,
                            ListUserRole = listUserRole,
                            ListRolePosition = listRolePosition,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListLessonTeacher = listLessonTeacher,
                            ListHomeroomTeacher = listHomeroomTeacher,
                            ListHomeroomStudent = listHomeroomStudent,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListTeacherPosition = listTeacherPosition,
                            UserRolePosition = item,
                            ListLessonPathway = null,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var _listIdUser = await GetUserAllAsync(param);
                        listUser.AddRange(_listIdUser);
                    }

                    if (item.Option == UserRolePersonalOptionType.Level || item.Option == UserRolePersonalOptionType.Grade)
                    {
                        var paramUserStudentParant = new GetUserStudentParantRequest
                        {
                            Role = item.Role,
                            ListHomeroomStudent = listHomeroomStudent,
                            ListIdHomeroom = item.Homeroom,
                            ListIdLevel = item.Level,
                            IdUserRolePositions = item.IdUserRolePositions,
                        };

                        var _listIdUser = await GetUserStudentParant(paramUserStudentParant);
                        listUser.AddRange(_listIdUser);
                    }

                    if (item.Option == UserRolePersonalOptionType.Personal)
                    {
                        var newPersonal = item.Personal
                            .Select(e => new GetUserRolePositionResult
                            {
                                IdUserRolePositions = item.IdUserRolePositions,
                                IdUser = e,
                                Role = UserRolePersonalOptionRole.STAFF.GetDescription(),
                            })
                            .ToList();

                        listUser.AddRange(newPersonal);
                    }
                }

                if (item.Role == UserRolePersonalOptionRole.PARENT || item.Role == UserRolePersonalOptionRole.ALL)
                {
                    if (item.Option == UserRolePersonalOptionType.All)
                    {
                        var param = new GetUserAllRequest
                        {
                            Role = UserRolePersonalOptionRole.PARENT,
                            ListUserRole = listUserRole,
                            ListRolePosition = listRolePosition,
                            ListNonTeachingLoad = listNonTeachingLoad,
                            ListLessonTeacher = listLessonTeacher,
                            ListHomeroomTeacher = listHomeroomTeacher,
                            ListHomeroomStudent = listHomeroomStudent,
                            ListDepartmentLevel = listDepartmentLevel,
                            ListTeacherPosition = listTeacherPosition,
                            UserRolePosition = item,
                            ListLessonPathway = null,
                            IdUserRolePositions = item.IdUserRolePositions,
                            ListHomeroomStudentAll = listHomeroomStudentAll,
                        };

                        var _listIdUser = await GetUserAllAsync(param);
                        listUser.AddRange(_listIdUser);
                    }

                    if (item.Option == UserRolePersonalOptionType.Level || item.Option == UserRolePersonalOptionType.Grade)
                    {
                        var paramUserStudentParant = new GetUserStudentParantRequest
                        {
                            Role = item.Role,
                            ListHomeroomStudent = listHomeroomStudent,
                            ListIdHomeroom = item.Homeroom,
                            ListIdLevel = item.Level,
                            IdUserRolePositions = item.IdUserRolePositions,
                            ListHomeroomStudentAll = listHomeroomStudentAll,
                        };

                        var _listIdUser = await GetUserStudentParant(paramUserStudentParant);
                        listUser.AddRange(_listIdUser);
                    }

                    if (item.Option == UserRolePersonalOptionType.Personal)
                    {
                        var newPersonal = item.Personal
                           .Select(e => new GetUserRolePositionResult
                           {
                               IdUserRolePositions = item.IdUserRolePositions,
                               IdUser = e,
                               Role = UserRolePersonalOptionRole.STAFF.GetDescription(),
                           })
                           .ToList();

                        listUser.AddRange(newPersonal);
                    }
                }
            }
            listUser = listUser.Distinct().ToList();
            return Request.CreateApiResult2(listUser as object);
        }

        protected async Task<List<GetUserRolePositionResult>> GetUserAllAsync(GetUserAllRequest param)
        {
            List<GetUserRolePositionResult> listUser = new List<GetUserRolePositionResult>();

            if (UserRolePersonalOptionRole.STAFF == param.Role)
            {
                #region UserRole
                var listUserStaffByUserRole = param.ListUserRole
                                            .Where(e => e.Role.RoleGroup.Code == RoleConstant.Staff)
                                            .ToList();

                var listIdRole = listUserStaffByUserRole
                                    .Select(e => e.IdRole)
                                    .Distinct().ToList();

                var listIdUserByUserRole = listUserStaffByUserRole
                                    .Select(e => new GetUserRolePositionResult
                                    {
                                        IdUserRolePositions = param.IdUserRolePositions,
                                        IdUser = e.IdUser,
                                        Role = param.Role.GetDescription()
                                    })
                                    .Distinct().ToList();

                listUser.AddRange(listIdUserByUserRole);
                #endregion

                #region NonTeachingLoad
                var listIdTeacherPosition = param.ListRolePosition
                                          .Where(e => listIdRole.Contains(e.IdRole))
                                          .Select(e => e.IdTeacherPosition)
                                          .ToList();

                var paramNonTeachingLoad = new GetNonTeachingLoad
                {
                    ListIdTeacherPosition = listIdTeacherPosition,
                    ListNonTeachingLoad = param.ListNonTeachingLoad,
                    ListDepartmentLevel = param.ListDepartmentLevel,
                    ListLessonPathway = param.ListLessonPathway,
                    Department = new List<string>(),
                    Role = param.Role,
                    IdUserRolePositions = param.IdUserRolePositions,
                };

                var listIdUserByNonTeaching = GetNonTeachingLoad(paramNonTeachingLoad);

                listUser.AddRange(listIdUserByNonTeaching);
                #endregion
            }

            if (UserRolePersonalOptionRole.TEACHER == param.Role)
            {
                var listUserStaffByUserRole = param.ListUserRole
                                           .Where(e => e.Role.RoleGroup.Code == RoleConstant.Teacher)
                                           .ToList();

                var listIdRole = listUserStaffByUserRole
                                    .Select(e => e.IdRole)
                                    .Distinct().ToList();

                var listIdTeacherPosition = param.ListRolePosition
                                         .Where(e => listIdRole.Contains(e.IdRole))
                                         .Select(e => e.IdTeacherPosition)
                                         .ToList();

                #region Subject Teacher
                var paramSubjectTeacher = new GetSubjectTeacherRequest
                {
                    IdTeacherPosition = listIdTeacherPosition,
                    ListLessonTeacher = param.ListLessonTeacher,
                    ListTeacherPosition = param.ListTeacherPosition,
                    ListDepartmentLevel = new List<MsDepartmentLevel>(),
                    Department = new List<string>(),
                    Role = param.Role,
                    IdUserRolePositions = param.IdUserRolePositions,
                };

                var listIdUserBySubjectTeacher = GetSubjectTeacher(paramSubjectTeacher);
                listUser.AddRange(listIdUserBySubjectTeacher);
                #endregion

                #region class Advisor
                var paramHomeroomTeacher = new GetHomeroomTeacherRequest
                {
                    IdTeacherPosition = listIdTeacherPosition,
                    ListHomeroomTeacher = param.ListHomeroomTeacher,
                    ListTeacherPosition = param.ListTeacherPosition,
                    ListDepartmentLevel = new List<MsDepartmentLevel>(),
                    Department = new List<string>(),
                    Role = param.Role,
                    IdUserRolePositions = param.IdUserRolePositions,
                };

                var listIdUserByHomeroomTeacher = GetHomeroomTeacher(paramHomeroomTeacher);
                listUser.AddRange(listIdUserByHomeroomTeacher);
                #endregion 

                #region NonTeachingLoad
                var paramNonTeachingLoad = new GetNonTeachingLoad
                {
                    ListIdTeacherPosition = listIdTeacherPosition,
                    ListNonTeachingLoad = param.ListNonTeachingLoad,
                    ListDepartmentLevel = param.ListDepartmentLevel,
                    ListLessonPathway = param.ListLessonPathway,
                    Department = new List<string>(),
                    Role = param.Role,
                    IdUserRolePositions = param.IdUserRolePositions,
                };

                var listIdUserByNonTeaching = GetNonTeachingLoad(paramNonTeachingLoad);

                listUser.AddRange(listIdUserByNonTeaching);
                #endregion
            }

            if (UserRolePersonalOptionRole.STUDENT == param.Role || UserRolePersonalOptionRole.PARENT == param.Role)
            {
                var paramUserStudentParant = new GetUserStudentParantRequest
                {
                    Role = param.Role,
                    ListHomeroomStudent = param.ListHomeroomStudent,
                    ListIdHomeroom = param.UserRolePosition.Homeroom,
                    ListIdLevel = param.UserRolePosition.Level,
                    IdUserRolePositions = param.IdUserRolePositions,
                    ListHomeroomStudentAll = param.ListHomeroomStudentAll,
                };

                var _listIdUser = await GetUserStudentParant(paramUserStudentParant);
                listUser.AddRange(_listIdUser);
            }

            return listUser;
        }

        protected List<GetUserRolePositionResult> GetNonTeachingLoad(GetNonTeachingLoad param)
        {
            List<GetNonTeachingLoadResult> listUserNonteachingLoad = new List<GetNonTeachingLoadResult>();

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
                    _listLessonPathway = _listLessonPathway
                                            .Where(e => listIdLevelByDept.Contains(e.Level.Id) && e.Subject.IdDepartment == _DepartemenPosition.Id)
                                            .ToList();
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
                                        .Select(e => new GetNonTeachingLoadResult
                                        {
                                            IdUser = item.IdUser,
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
                            })
                            .Select(e => new GetUserRolePositionResult
                            {
                                IdUserRolePositions = param.IdUserRolePositions,
                                IdUser = e.Key.IdUser,
                                Role = param.Role.GetDescription()
                            })
                            .ToList();



            return listUser;
        }

        protected List<GetUserRolePositionResult> GetSubjectTeacher(GetSubjectTeacherRequest param)
        {
            var listUser = new List<GetUserRolePositionResult>();
            var exsisSubjectTeacher = param.ListTeacherPosition
                                        .Where(e => e.Position.Code == PositionConstant.SubjectTeacher && param.IdTeacherPosition.Contains(e.Id))
                                        .Any();

            if (exsisSubjectTeacher)
            {
                var listLessonTeacher = param.ListLessonTeacher;
                if (param.Department.Any())
                    listLessonTeacher = listLessonTeacher.Where(e => param.Department.Contains(e.Lesson.Subject.IdDepartment)).ToList();

                listUser = listLessonTeacher
                            .GroupBy(e => new
                            {
                                IdUser = e.IdUser,
                            })
                            .Select(e => new GetUserRolePositionResult
                            {
                                IdUserRolePositions = param.IdUserRolePositions,
                                IdUser = e.Key.IdUser,
                                Role = param.Role.GetDescription(),
                            })
                            .ToList();
            }

            return listUser;
        }

        protected List<GetUserRolePositionResult> GetHomeroomTeacher(GetHomeroomTeacherRequest param)
        {
            var listUser = new List<GetUserRolePositionResult>();
            var exsisSubjectTeacher = param.ListTeacherPosition
                                        .Where(e => e.Position.Code == PositionConstant.ClassAdvisor && param.IdTeacherPosition.Contains(e.Id))
                                        .Any();

            if (exsisSubjectTeacher)
            {
                var listHomeroomTeacher = param.ListHomeroomTeacher;
                if (param.Department.Any())
                {
                    var listLevel = param.ListDepartmentLevel.Where(e => param.Department.Contains(e.IdDepartment)).Select(e => e.IdLevel).Distinct().ToList();
                    listHomeroomTeacher = listHomeroomTeacher.Where(e => listLevel.Contains(e.Homeroom.Grade.IdLevel)).ToList();
                }

                listUser = listHomeroomTeacher
                           .GroupBy(e => new
                           {
                               IdUser = e.IdBinusian,
                           })
                           .Select(e => new GetUserRolePositionResult
                           {
                               IdUserRolePositions = param.IdUserRolePositions,
                               IdUser = e.Key.IdUser,
                               Role = param.Role.GetDescription(),
                           })
                           .ToList();

            }

            return listUser;
        }

        public async Task<List<GetUserRolePositionResult>> GetUserStudentParant(GetUserStudentParantRequest param)
        {
            var query = param.ListHomeroomStudent.Distinct();

            #region filter
            if (param.ListIdLevel != null)
            {
                if (param.ListIdLevel.Any())
                {
                    query = param.ListHomeroomStudent.Where(e => param.ListIdLevel.Contains(e.Level.Id));
                }
            }

            if (param.ListIdHomeroom != null)
            {
                if (param.ListIdHomeroom.Any())
                {
                    query = param.ListHomeroomStudent.Where(e => param.ListIdHomeroom.Contains(e.Homeroom.Id));
                }
            }
            #endregion

            var listUser = new List<GetUserRolePositionResult>();
            if (param.Role == UserRolePersonalOptionRole.STUDENT)
            {
                listUser = query
                            .GroupBy(e => new
                            {
                                IdUser = e.IdStudent,
                                IdLevel = e.Level.Id,
                                Level = e.Level.Description,
                                LevelCode = e.Level.Code,
                                LevelOrder = e.Level.OrderNumber,
                                IdGrade = e.Grade.Id,
                                Grade = e.Grade.Description,
                                GradeCode = e.Grade.Code,
                                GradeOrder = e.Grade.OrderNumber,
                                IdHomeroom = e.Homeroom.Id,
                                Homeroom = e.Homeroom.Description,
                                Semester = e.Homeroom.Semester,
                                IdHomeroomStudent = e.IdHomeroomStudent
                            })
                            .Select(e => new GetUserRolePositionResult
                            {
                                IdUserRolePositions = param.IdUserRolePositions,
                                IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                IdUser = e.Key.IdUser,
                                Level = new ItemValueVmWithOrderNumber
                                {
                                    Id = e.Key.IdLevel,
                                    Code = e.Key.LevelCode,
                                    Description = e.Key.Level,
                                    OrderNumber = e.Key.LevelOrder,
                                },
                                Grade = new ItemValueVmWithOrderNumber
                                {
                                    Id = e.Key.IdGrade,
                                    Code = e.Key.GradeCode,
                                    Description = e.Key.Grade,
                                    OrderNumber = e.Key.GradeOrder,
                                },
                                Homeroom = new UserRolePositionHomeroom
                                {
                                    Id = e.Key.IdHomeroom,
                                    Description = e.Key.Homeroom,
                                    Semester = e.Key.Semester
                                },
                                Role = param.Role.GetDescription()
                            })
                            .Distinct().ToList();
            }
            else if (param.Role == UserRolePersonalOptionRole.PARENT)
            {
                var _listUser = query.Select(e => e.IdStudent).Distinct().ToList();

                var listIdSiblingGroup = await _dbContext.Entity<MsSiblingGroup>()
                            .Where(e => _listUser.Contains(e.IdStudent))
                            .Select(e => e.Id)
                            .ToListAsync(CancellationToken);

                var listStudentSiblingById = await _dbContext.Entity<MsSiblingGroup>()
                                                .Where(e => listIdSiblingGroup.Contains(e.Id))
                                                .ToListAsync(CancellationToken);

                var listStudentSiblingGroup = listStudentSiblingById
                                               .GroupBy(e => new
                                               {
                                                   e.Id
                                               });

                foreach (var item in listStudentSiblingGroup)
                {
                    var listStudentSibling = item.Select(e => e.IdStudent).ToList();

                    foreach(var itemStudentParent in listStudentSibling)
                    {

                        foreach (var itemStudentStudent in listStudentSibling)
                        {
                            var listHomeroomStudentByIdStudent = param.ListHomeroomStudentAll.Where(e=>e.IdStudent==itemStudentStudent).ToList();

                            var listHomeroomStudentByFilter = query
                                                                .Where(e => listStudentSibling.Contains(e.IdStudent))
                                                                .GroupBy(e => new
                                                                {
                                                                    IdUser = e.IdStudent,
                                                                    IdLevel = e.Level.Id,
                                                                    Level = e.Level.Description,
                                                                    LevelCode = e.Level.Code,
                                                                    LevelOrder = e.Level.OrderNumber,
                                                                    IdGrade = e.Grade.Id,
                                                                    Grade = e.Grade.Description,
                                                                    GradeCode = e.Grade.Code,
                                                                    GradeOrder = e.Grade.OrderNumber,
                                                                    IdHomeroom = e.Homeroom.Id,
                                                                    Homeroom = e.Homeroom.Description,
                                                                    Semester = e.Homeroom.Semester,
                                                                    //IdHomeroomStudent = e.IdHomeroomStudent
                                                                })
                                                                .Select(e => new GetUserRolePositionResult
                                                                {
                                                                    IdUser = "P" + itemStudentParent,
                                                                    IdUserRolePositions = param.IdUserRolePositions,
                                                                    IdHomeroomStudent = listHomeroomStudentByIdStudent
                                                                                            .Where(f=>f.Homeroom.Semester==e.Key.Semester && f.Homeroom.Id==e.Key.IdHomeroom)
                                                                                            .Select(e=>e.IdHomeroomStudent).FirstOrDefault(),
                                                                    IdUserChild = itemStudentStudent,
                                                                    Level = new ItemValueVmWithOrderNumber
                                                                    {
                                                                        Id = e.Key.IdLevel,
                                                                        Code = e.Key.LevelCode,
                                                                        Description = e.Key.Level,
                                                                        OrderNumber = e.Key.LevelOrder,
                                                                    },
                                                                    Grade = new ItemValueVmWithOrderNumber
                                                                    {
                                                                        Id = e.Key.IdGrade,
                                                                        Code = e.Key.GradeCode,
                                                                        Description = e.Key.Grade,
                                                                        OrderNumber = e.Key.GradeOrder,
                                                                    },
                                                                    Homeroom = new UserRolePositionHomeroom
                                                                    {
                                                                        Id = e.Key.IdHomeroom,
                                                                        Description = e.Key.Homeroom,
                                                                        Semester = e.Key.Semester
                                                                    },
                                                                    Role = param.Role.GetDescription()
                                                                })
                                                                .ToList();

                            listUser.AddRange(listHomeroomStudentByFilter);
                        }

                    }
                }
            }

            listUser = listUser.Where(e => e.IdHomeroomStudent != null).ToList();

            return listUser;
        }
    }

    public class GetUserAllRequest
    {
        public string IdUserRolePositions { get; set; }
        public UserRolePersonalOptionRole Role { get; set; }
        public List<MsUserRole> ListUserRole { get; set; }
        public List<TrRolePosition> ListRolePosition { get; set; }
        public List<TrNonTeachingLoad> ListNonTeachingLoad { get; set; }
        public List<MsLessonTeacher> ListLessonTeacher { get; set; }
        public List<MsHomeroomTeacher> ListHomeroomTeacher { get; set; }
        public List<MsDepartmentLevel> ListDepartmentLevel { get; set; }
        public List<GetSubjectByUserResult> ListLessonPathway { get; set; }
        public List<GetStudentRolePositionResult> ListHomeroomStudentAll { get; set; }
        public List<GetStudentRolePositionResult> ListHomeroomStudent { get; set; }
        public List<MsTeacherPosition> ListTeacherPosition { get; set; }
        public GetUserRolePosition UserRolePosition { get; set; }
    }

    public class GetNonTeachingLoad
    {
        public string IdUserRolePositions { get; set; }
        public UserRolePersonalOptionRole Role { get; set; }
        public List<string> ListIdTeacherPosition { get; set; }
        public List<TrNonTeachingLoad> ListNonTeachingLoad { get; set; }
        public List<GetSubjectByUserResult> ListLessonPathway { get; set; }
        public List<MsDepartmentLevel> ListDepartmentLevel { get; set; }
        public List<string> Department { get; set; }
    }

    public class GetSubjectTeacherRequest
    {
        public string IdUserRolePositions { get; set; }
        public UserRolePersonalOptionRole Role { get; set; }
        public List<string> IdTeacherPosition { get; set; }
        public List<MsTeacherPosition> ListTeacherPosition { get; set; }
        public List<MsLessonTeacher> ListLessonTeacher { get; set; }
        public List<MsDepartmentLevel> ListDepartmentLevel { get; set; }
        public List<string> Department { get; set; }
    }

    public class GetHomeroomTeacherRequest
    {
        public string IdUserRolePositions { get; set; }
        public UserRolePersonalOptionRole Role { get; set; }
        public List<string> IdTeacherPosition { get; set; }
        public List<MsTeacherPosition> ListTeacherPosition { get; set; }
        public List<MsHomeroomTeacher> ListHomeroomTeacher { get; set; }
        public List<MsDepartmentLevel> ListDepartmentLevel { get; set; }
        public List<string> Department { get; set; }
    }

    public class GetUserStudentParantRequest
    {
        public string IdUserRolePositions { get; set; }
        public UserRolePersonalOptionRole Role { get; set; }
        public List<GetStudentRolePositionResult> ListHomeroomStudent { get; set; }
        public List<GetStudentRolePositionResult> ListHomeroomStudentAll { get; set; }
        public List<string> ListIdLevel { get; set; }
        public List<string> ListIdHomeroom { get; set; }
    }
}
