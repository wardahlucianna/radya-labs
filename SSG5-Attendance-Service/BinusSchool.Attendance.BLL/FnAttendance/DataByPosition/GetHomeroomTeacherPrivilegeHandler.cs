using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Attendance.FnAttendance.DataByPosition.Validator;
using BinusSchool.Attendance.FnAttendance.Utils;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition
{
    public class GetHomeroomTeacherPrivilegeHandler : FunctionsHttpSingleHandler
    {

        private readonly IAttendanceDbContext _dbContext;
        private readonly GetAvailabilityPositionByIdUserHandler _availabilityPositionByIdUser;

        public GetHomeroomTeacherPrivilegeHandler(
            IAttendanceDbContext dbContext,
            GetAvailabilityPositionByIdUserHandler availabilityPositionByIdUser
            )
        {
            _dbContext = dbContext;
            _availabilityPositionByIdUser = availabilityPositionByIdUser;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetHomeroomTeacherPrivilegeRequest, GetHomeroomTeacherPrivilegeValidator>();
            var result = await GetHomeroomTeacherPrivileges(new GetHomeroomTeacherPrivilegeRequest
            {
                IdSchool = param.IdSchool,
                IdAcademicYear = param.IdAcademicYear,
                IdUser = param.IdUser,
                Semester = param.Semester
            });
            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetHomeroomTeacherPrivilegeResult>> GetHomeroomTeacherPrivileges(GetHomeroomTeacherPrivilegeRequest param)
        {
            List<GetHomeroomTeacherPrivilegeResult> ReturnResult = new List<GetHomeroomTeacherPrivilegeResult>();

            List<string> _idLevels = new List<string>();
            List<string> _idGrades = new List<string>();
            List<string> _idLessons = new List<string>();

            var positionUserdetail = await _availabilityPositionByIdUser.GetAvailablePositionDetail(param.IdUser, param.IdAcademicYear, null);
            var positionUser = await _availabilityPositionByIdUser.GetAvailablePosition(param.IdUser, param.IdAcademicYear, null);

            if (positionUser.Count() > 0)
            {
                foreach (var item in positionUser)
                {
                    if (item.Id == PositionConstant.Principal || item.Id.ToLower() == "acop" || item.Id.ToLower() == "gc")
                    {
                        var res = CheckPositionByIdUserUtil.Principal(positionUserdetail.OtherPositions);
                        if (res.Count > 0)
                        {
                            _idLevels = (List<string>)res["idLevel"];

                            foreach (var _idLevel in _idLevels)
                            {
                                var data = _dbContext.Entity<MsHomeroom>()
                                    .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                x.Semester == (param.Semester != null ? param.Semester : x.Semester) &&
                                                x.Grade.IdLevel == _idLevel)
                                    //.Where(x => _idLevels.Distinct().Any(y => y == x.Grade.IdLevel))
                                    .Select(x => new GetHomeroomTeacherPrivilegeResult
                                    {

                                        AcademicYear = x.Grade.Level.IdAcademicYear,
                                        Semester = x.Semester,
                                        IdGrade = x.IdGrade,
                                        GradeCode = x.Grade.Code,
                                        GradeName = x.Grade.Description,
                                        GradeOrder = x.Grade.OrderNumber,
                                        IdLevel = x.Grade.IdLevel,
                                        LevelName = x.Grade.Level.Description,
                                        LevelOrder = x.Grade.Level.OrderNumber,
                                        IdHomeroom = x.Id,
                                        HomeroomCode = x.GradePathwayClassroom.Classroom.Code,
                                        HomeroomName = x.Grade.Code + x.GradePathwayClassroom.Classroom.Description
                                    }).Distinct().ToList();

                                if (data.Count() > 0)
                                    ReturnResult.AddRange(data);
                            }
                        }
                    }
                    if (item.Id == PositionConstant.VicePrincipal)
                    {
                        var res = CheckPositionByIdUserUtil.VicePrincipal(positionUserdetail.OtherPositions);
                        if (res.Count > 0)
                        {
                            _idLevels = (List<string>)res["idLevel"];

                            foreach (var _idLevel in _idLevels)
                            {
                                var data = _dbContext.Entity<MsHomeroom>()
                                   .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                               x.Semester == (param.Semester != null ? param.Semester : x.Semester) &&
                                               x.Grade.IdLevel == _idLevel)
                                   //.Where(x => _idLevels.Distinct().Any(y => y == x.Grade.IdLevel))
                                   .Select(x => new GetHomeroomTeacherPrivilegeResult
                                   {

                                       AcademicYear = x.Grade.Level.IdAcademicYear,
                                       Semester = x.Semester,
                                       IdGrade = x.IdGrade,
                                       GradeCode = x.Grade.Code,
                                       GradeName = x.Grade.Description,
                                       GradeOrder = x.Grade.OrderNumber,
                                       IdLevel = x.Grade.IdLevel,
                                       LevelName = x.Grade.Level.Description,
                                       LevelOrder = x.Grade.Level.OrderNumber,
                                       IdHomeroom = x.Id,
                                       HomeroomCode = x.GradePathwayClassroom.Classroom.Code,
                                       HomeroomName = x.Grade.Code + x.GradePathwayClassroom.Classroom.Description
                                   }).Distinct().ToList();

                                if (data.Count() > 0)
                                    ReturnResult.AddRange(data);
                            }
                        }
                    }
                    if (item.Id == PositionConstant.AffectiveCoordinator)
                    {
                        var res = CheckPositionByIdUserUtil.AffectiveCoordinator(positionUserdetail.OtherPositions);
                        if (res.Count > 0)
                        {
                            _idLevels = (List<string>)res["idLevel"];

                            foreach (var _idLevel in _idLevels)
                            {
                                var data = _dbContext.Entity<MsHomeroom>()
                                    .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                x.Semester == (param.Semester != null ? param.Semester : x.Semester) &&
                                                x.Grade.IdLevel == _idLevel)
                                    //.Where(x => _idLevels.Distinct().Any(y => y == x.Grade.IdLevel))
                                    .Select(x => new GetHomeroomTeacherPrivilegeResult
                                    {
                                        AcademicYear = x.Grade.Level.IdAcademicYear,
                                        Semester = x.Semester,
                                        IdGrade = x.IdGrade,
                                        GradeCode = x.Grade.Code,
                                        GradeName = x.Grade.Description,
                                        GradeOrder = x.Grade.OrderNumber,
                                        IdLevel = x.Grade.IdLevel,
                                        LevelName = x.Grade.Level.Description,
                                        LevelOrder = x.Grade.Level.OrderNumber,
                                        IdHomeroom = x.Id,
                                        HomeroomCode = x.GradePathwayClassroom.Classroom.Code,
                                        HomeroomName = x.Grade.Code + x.GradePathwayClassroom.Classroom.Description
                                    }).Distinct().ToList();

                                if (data.Count() > 0)
                                    ReturnResult.AddRange(data);
                            }
                        }
                    }
                    if (item.Id == PositionConstant.LevelHead)
                    {
                        var res = CheckPositionByIdUserUtil.LevelHead(positionUserdetail.OtherPositions);
                        if (res.Count > 0)
                        {
                            _idGrades = (List<string>)res["idGrade"];

                            foreach (var _idGrade in _idGrades)
                            {
                                var data = _dbContext.Entity<MsHomeroom>()
                                    .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                x.Semester == (param.Semester != null ? param.Semester : x.Semester) &&
                                                x.IdGrade == _idGrade)
                                     //.Where(x => _idGrades.Distinct().Any(y => y == x.HomeroomStudent.Homeroom.IdGrade))
                                     .Select(x => new GetHomeroomTeacherPrivilegeResult
                                     {
                                         AcademicYear = x.Grade.Level.IdAcademicYear,
                                         Semester = x.Semester,
                                         IdGrade = x.IdGrade,
                                         GradeCode = x.Grade.Code,
                                         GradeName = x.Grade.Description,
                                         GradeOrder = x.Grade.OrderNumber,
                                         IdLevel = x.Grade.IdLevel,
                                         LevelName = x.Grade.Level.Description,
                                         LevelOrder = x.Grade.Level.OrderNumber,
                                         IdHomeroom = x.Id,
                                         HomeroomCode = x.GradePathwayClassroom.Classroom.Code,
                                         HomeroomName = x.Grade.Code + x.GradePathwayClassroom.Classroom.Description
                                     }).Distinct().ToList();

                                if (data.Count() > 0)
                                    ReturnResult.AddRange(data);
                            }
                        }
                    }
                    if (item.Id == PositionConstant.SubjectHead)
                    {
                        var res = CheckPositionByIdUserUtil.SubjectHead(positionUserdetail.OtherPositions);
                        if (res.Count > 0)
                        {
                            var _idSubjects = (List<string>)res["idSubject"];

                            var data = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Where(x => x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                                            x.HomeroomStudent.Homeroom.Semester == (param.Semester != null ? param.Semester : x.HomeroomStudent.Homeroom.Semester))
                                .Where(x => _idSubjects.Distinct().Any(y => y == x.IdSubject))
                                .Where(x => !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear)
                                            || (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear) && (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && (a.ActiveStatus == true || a.IdTrStudentStatus == null || a.IdStudentStatus == 7))))
                                                                                                                                             && !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && a.CurrentStatus == "A" && a.ActiveStatus == false))
                                .Select(x => new GetHomeroomTeacherPrivilegeResult
                                {
                                    AcademicYear = x.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear,
                                    Semester = x.HomeroomStudent.Homeroom.Semester,
                                    IdGrade = x.HomeroomStudent.Homeroom.IdGrade,
                                    GradeCode = x.HomeroomStudent.Homeroom.Grade.Code,
                                    GradeName = x.HomeroomStudent.Homeroom.Grade.Description,
                                    GradeOrder = x.HomeroomStudent.Homeroom.Grade.OrderNumber,
                                    IdLevel = x.HomeroomStudent.Homeroom.Grade.IdLevel,
                                    LevelName = x.HomeroomStudent.Homeroom.Grade.Level.Description,
                                    LevelOrder = x.HomeroomStudent.Homeroom.Grade.Level.OrderNumber,
                                    IdHomeroom = x.HomeroomStudent.IdHomeroom,
                                    HomeroomCode = x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                    HomeroomName = x.HomeroomStudent.Homeroom.Grade.Code + x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description
                                }).Distinct().ToList();

                            if (data.Count() > 0)
                                ReturnResult.AddRange(data);
                        }
                    }
                    if (item.Id == PositionConstant.SubjectHeadAssitant)
                    {
                        var res = CheckPositionByIdUserUtil.SubjectHeadAsisstant(positionUserdetail.OtherPositions);
                        if (res.Count > 0)
                        {
                            var _idSubjects = (List<string>)res["idSubject"];

                            var data = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                  .Where(x => x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                                              x.HomeroomStudent.Homeroom.Semester == (param.Semester != null ? param.Semester : x.HomeroomStudent.Homeroom.Semester))
                                  .Where(x => _idSubjects.Distinct().Any(y => y == x.IdSubject))
                                  .Where(x => !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear)
                                            || (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear) && (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && (a.ActiveStatus == true || a.IdTrStudentStatus == null || a.IdStudentStatus == 7))))
                                                                                                                                             && !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && a.CurrentStatus == "A" && a.ActiveStatus == false))
                                  .Select(x => new GetHomeroomTeacherPrivilegeResult
                                  {
                                      AcademicYear = x.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear,
                                      Semester = x.HomeroomStudent.Homeroom.Semester,
                                      IdGrade = x.HomeroomStudent.Homeroom.IdGrade,
                                      GradeCode = x.HomeroomStudent.Homeroom.Grade.Code,
                                      GradeName = x.HomeroomStudent.Homeroom.Grade.Description,
                                      GradeOrder = x.HomeroomStudent.Homeroom.Grade.OrderNumber,
                                      IdLevel = x.HomeroomStudent.Homeroom.Grade.IdLevel,
                                      LevelName = x.HomeroomStudent.Homeroom.Grade.Level.Description,
                                      LevelOrder = x.HomeroomStudent.Homeroom.Grade.Level.OrderNumber,
                                      IdHomeroom = x.HomeroomStudent.IdHomeroom,
                                      HomeroomCode = x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                      HomeroomName = x.HomeroomStudent.Homeroom.Grade.Code + x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description
                                  }).Distinct().ToList();

                            if (data.Count() > 0)
                                ReturnResult.AddRange(data);
                        }
                    }
                    if (item.Id == PositionConstant.HeadOfDepartment)
                    {
                        var res = CheckPositionByIdUserUtil.HeadOfDepartment(positionUserdetail.OtherPositions);
                        if (res.Count > 0)
                        {
                            List<string> IdGrade = new List<string>();
                            List<string> IdSubject = new List<string>();
                            var _idDepartments = (List<string>)res["idDepartment"];

                            var data = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Where(x => x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                                            x.HomeroomStudent.Homeroom.Semester == (param.Semester != null ? param.Semester : x.HomeroomStudent.Homeroom.Semester))
                                .Where(x => _idDepartments.Distinct().Any(y => y == x.Subject.IdDepartment))
                                .Where(x => !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear)
                                            || (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear) && (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && (a.ActiveStatus == true || a.IdTrStudentStatus == null || a.IdStudentStatus == 7))))
                                                                                                                                             && !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && a.CurrentStatus == "A" && a.ActiveStatus == false))
                                 .Select(x => new GetHomeroomTeacherPrivilegeResult
                                 {
                                     AcademicYear = x.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear,
                                     Semester = x.HomeroomStudent.Homeroom.Semester,
                                     IdGrade = x.HomeroomStudent.Homeroom.IdGrade,
                                     GradeCode = x.HomeroomStudent.Homeroom.Grade.Code,
                                     GradeName = x.HomeroomStudent.Homeroom.Grade.Description,
                                     GradeOrder = x.HomeroomStudent.Homeroom.Grade.OrderNumber,
                                     IdLevel = x.HomeroomStudent.Homeroom.Grade.IdLevel,
                                     LevelName = x.HomeroomStudent.Homeroom.Grade.Level.Description,
                                     LevelOrder = x.HomeroomStudent.Homeroom.Grade.Level.OrderNumber,
                                     IdHomeroom = x.HomeroomStudent.IdHomeroom,
                                     HomeroomCode = x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                     HomeroomName = x.HomeroomStudent.Homeroom.Grade.Code + x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description
                                 }).Distinct().ToList();

                            if (data.Count() > 0)
                                ReturnResult.AddRange(data);
                        }
                    }
                    if (item.Id == PositionConstant.ClassAdvisor && param.IncludeClassAdvisor == true)
                    {
                        var CAHomeroom = positionUserdetail.ClassAdvisors.Select(x => x.Id).ToList();

                        var data = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                            .Where(x => x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                                        x.HomeroomStudent.Homeroom.Semester == (param.Semester != null ? param.Semester : x.HomeroomStudent.Homeroom.Semester))
                            .Where(x => CAHomeroom.Any(y => y == x.HomeroomStudent.IdHomeroom))
                            .Where(x => !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear)
                                            || (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear) && (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && (a.ActiveStatus == true || a.IdTrStudentStatus == null || a.IdStudentStatus == 7))))
                                                                                                                                             && !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && a.CurrentStatus == "A" && a.ActiveStatus == false))
                            .Select(x => new GetHomeroomTeacherPrivilegeResult
                            {
                                AcademicYear = x.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear,
                                Semester = x.HomeroomStudent.Homeroom.Semester,
                                IdGrade = x.HomeroomStudent.Homeroom.IdGrade,
                                GradeCode = x.HomeroomStudent.Homeroom.Grade.Code,
                                GradeName = x.HomeroomStudent.Homeroom.Grade.Description,
                                GradeOrder = x.HomeroomStudent.Homeroom.Grade.OrderNumber,
                                IdLevel = x.HomeroomStudent.Homeroom.Grade.IdLevel,
                                LevelName = x.HomeroomStudent.Homeroom.Grade.Level.Description,
                                LevelOrder = x.HomeroomStudent.Homeroom.Grade.Level.OrderNumber,
                                IdHomeroom = x.HomeroomStudent.IdHomeroom,
                                HomeroomCode = x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                HomeroomName = x.HomeroomStudent.Homeroom.Grade.Code + x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description
                            }).Distinct().ToList();

                        if (data.Count() > 0)
                            ReturnResult.AddRange(data);
                    }
                    if (item.Id == PositionConstant.SubjectTeacher && param.IncludeSubjectTeacher == true)
                    {
                        var checkST = await _dbContext.Entity<MsLessonTeacher>()
                                .Where(x => x.Lesson.AcademicYear.School.Id == param.IdSchool &&
                                            x.Lesson.IdAcademicYear == param.IdAcademicYear &&
                                            x.IdUser == param.IdUser)
                                .Select(x => x.IdLesson).Distinct()
                                .ToListAsync(CancellationToken);

                        var data = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Where(x => x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                                            x.HomeroomStudent.Homeroom.Semester == (param.Semester != null ? param.Semester : x.HomeroomStudent.Homeroom.Semester))
                                .Where(x => checkST.Any(y => y == x.IdLesson))
                                .Where(x => !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear)
                                            || (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear) && (x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && (a.ActiveStatus == true || a.IdTrStudentStatus == null || a.IdStudentStatus == 7))))
                                                                                                                                             && !x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.IdAcademicYear == param.IdAcademicYear && a.CurrentStatus == "A" && a.ActiveStatus == false))
                                 .Select(x => new GetHomeroomTeacherPrivilegeResult
                                 {
                                     AcademicYear = x.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear,
                                     Semester = x.HomeroomStudent.Homeroom.Semester,
                                     IdGrade = x.HomeroomStudent.Homeroom.IdGrade,
                                     GradeCode = x.HomeroomStudent.Homeroom.Grade.Code,
                                     GradeName = x.HomeroomStudent.Homeroom.Grade.Description,
                                     GradeOrder = x.HomeroomStudent.Homeroom.Grade.OrderNumber,
                                     IdLevel = x.HomeroomStudent.Homeroom.Grade.IdLevel,
                                     LevelName = x.HomeroomStudent.Homeroom.Grade.Level.Description,
                                     LevelOrder = x.HomeroomStudent.Homeroom.Grade.Level.OrderNumber,
                                     IdHomeroom = x.HomeroomStudent.IdHomeroom,
                                     HomeroomCode = x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                     HomeroomName = x.HomeroomStudent.Homeroom.Grade.Code + x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description
                                 }).Distinct().ToList();

                        if (data.Count() > 0)
                            ReturnResult.AddRange(data);
                    }
                }
            }

            return ReturnResult;
        }
    }

}
