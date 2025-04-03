using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnSchedule.RolePosition.Validator;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Common.Constants;
using Microsoft.Azure.Documents.SystemFunctions;
using Newtonsoft.Json;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using NPOI.XWPF.UserModel;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;

namespace BinusSchool.Scheduling.FnSchedule.RolePosition
{
    public class GetSubjectByUserHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetSubjectByUserHandler(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetSubjectByUserRequest, GetSubjectByUserValidator>();
            List<GetSubjectByUserResult> items = new List<GetSubjectByUserResult>();

            #region GetData
            #region LessonPathway
            var listStudentEnrollment = await _dbContext.Entity<MsLessonPathway>()
                                        .Include(e=>e.Lesson).ThenInclude(e => e.Subject)
                                        .Include(e=>e.HomeroomPathway).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                                        .Include(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                        .Where(e => e.HomeroomPathway.Homeroom.IdAcademicYear == body.IdAcademicYear)
                                        .GroupBy(e => new
                                        {
                                            IdLesson = e.IdLesson,
                                            IdSubject = e.Lesson.IdSubject,
                                            Subject = e.Lesson.Subject.Description,
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
            #endregion

            #region TeacherPosition
            var listPosition = await _dbContext.Entity<MsTeacherPosition>()
                                                 .Include(e => e.Position)
                                                 .Where(e => body.ListIdTeacherPositions.Contains(e.Id))
                                                 .Select(e => e.Position.Code)
                                                 .ToListAsync(CancellationToken);
            #endregion

            #region LessonTeacher
            var querySubjectTeacher = _dbContext.Entity<MsLessonTeacher>()
                                                .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                                                .Where(e => e.Lesson.IdAcademicYear == body.IdAcademicYear);

            if (!string.IsNullOrEmpty(body.IdUser))
                querySubjectTeacher = querySubjectTeacher.Where(e => e.IdUser == body.IdUser);

            if (!string.IsNullOrEmpty(body.IsPrimary.ToString()))
                querySubjectTeacher = querySubjectTeacher.Where(e => e.IsPrimary== body.IsPrimary);

            if (!string.IsNullOrEmpty(body.IsAttendance.ToString()))
                querySubjectTeacher = querySubjectTeacher.Where(e => e.IsAttendance == body.IsAttendance);

            //if (!string.IsNullOrEmpty(body.IsLessonPlan.ToString()))
            //    querySubjectTeacher = querySubjectTeacher.Where(e => e.IsLessonPlan== body.IsLessonPlan);

            //if (!string.IsNullOrEmpty(body.IsClassDiary.ToString()))
            //    querySubjectTeacher = querySubjectTeacher.Where(e => e.IsClassDiary == body.IsClassDiary);

            var listSubjectTeacher = await querySubjectTeacher
                                       .GroupBy(e => new
                                       {
                                           IdLesson = e.IdLesson,
                                           IdSubject = e.Lesson.IdSubject,
                                           Subject = e.Lesson.Subject.Description,
                                           SubjectCode = e.Lesson.Subject.Code,
                                           IdGrade = e.Lesson.Grade.Id,
                                           Grade = e.Lesson.Grade.Description,
                                           GradeCode = e.Lesson.Grade.Code,
                                           GradeOrder = e.Lesson.Grade.OrderNumber,
                                           IdLevel = e.Lesson.Grade.Level.Id,
                                           Level = e.Lesson.Grade.Level.Description,
                                           LevelCode = e.Lesson.Grade.Level.Code,
                                           LevelOrder = e.Lesson.Grade.Level.OrderNumber,
                                           IdUser = e.IdUser,
                                           ClassId = e.Lesson.ClassIdGenerated
                                       })
                                       .Select(e => new GetSubjectByUserResult
                                       {
                                           Lesson = new GetSubjectLesson
                                           {
                                               Id = e.Key.IdLesson,
                                               ClassId = e.Key.ClassId,
                                           },
                                           Subject = new GetSubjectUser
                                           {
                                               Id = e.Key.IdSubject,
                                               Description = e.Key.Subject,
                                               Code = e.Key.SubjectCode,
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
                                           }
                                       })
                                       .ToListAsync(CancellationToken);

            var listIdLesson = listSubjectTeacher.Select(e => e.Lesson.Id).Distinct().ToList();
            var listLessonHomeroom = await _dbContext.Entity<MsLessonPathway>()
                                       .Include(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom)
                                            .ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                       .Include(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom)
                                            .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                       .Where(e => e.Lesson.IdAcademicYear == body.IdAcademicYear && listIdLesson.Contains(e.IdLesson))
                                       .GroupBy(e => new
                                       {
                                           e.IdLesson,
                                           IdHomeroom = e.HomeroomPathway.Homeroom.Id,
                                           Semester = e.HomeroomPathway.Homeroom.Semester,
                                           Homeroom = e.HomeroomPathway.Homeroom.Grade.Code
                                            + e.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code
                                       })
                                       .Select(e => e.Key)
                                       .ToListAsync(CancellationToken);

            listSubjectTeacher.ForEach(e =>
            {
                e.Homeroom = new GetSubjectHomeroom
                {
                    Id = listLessonHomeroom.Where(x => x.IdLesson == e.Lesson.Id).Select(e => e.IdHomeroom).FirstOrDefault(),
                    Description = listLessonHomeroom.Where(x => x.IdLesson == e.Lesson.Id).Select(e => e.Homeroom).FirstOrDefault(),
                    Semester = listLessonHomeroom.Where(x => x.IdLesson == e.Lesson.Id).Select(e => e.Semester).FirstOrDefault(),

                };
            });
            #endregion

            #region HomeroomTeacher
            var queryHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                                           .Where(e => e.Homeroom.IdAcademicYear == body.IdAcademicYear
                                                   && e.IdTeacherPosition== body.IdAcademicYear);

            if (!string.IsNullOrEmpty(body.IdUser))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.IdBinusian == body.IdUser);

            var listHomeroomTeacher = await queryHomeroomTeacher
                                        .ToListAsync(CancellationToken);
            #endregion

            #region NonTachingLoad
            var queryNonTeachingLoad = _dbContext.Entity<TrNonTeachingLoad>()
                                           .Include(e=>e.MsNonTeachingLoad)
                                           .Where(e => e.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear
                                                   && body.ListIdTeacherPositions.Contains(e.MsNonTeachingLoad.IdTeacherPosition)
                                                   && e.Data != null);

            if (!string.IsNullOrEmpty(body.IdUser))
                queryNonTeachingLoad = queryNonTeachingLoad.Where(e => e.IdUser == body.IdUser);

            var listNonTeachingLoad = await queryNonTeachingLoad
                                        .ToListAsync(CancellationToken);
            #endregion

            #region DepartmentLevel
            var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                .Include(e => e.Level).ThenInclude(e => e.MsGrades)
                                 .Where(x => x.Level.IdAcademicYear == body.IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            #endregion
            #endregion

            var paramGetPositionUser = new GetPositionUserRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                IdTeacherPosition = body.ListIdTeacherPositions,
                IdUser = body.IdUser,
                ListDepartmentLevel = listDepartmentLevel,
                ListHomeroomTeacher = listHomeroomTeacher,
                ListNonTeachingLoad = listNonTeachingLoad,
                ListPosition = listPosition,
                ListStudentEnrollment = listStudentEnrollment,
                ListSubjectTeacher = listSubjectTeacher
            };

            items = GetPositionUser(paramGetPositionUser);
            return Request.CreateApiResult2(items as object);
        }

        private List<GetSubjectByUserResult> GetPositionUser(GetPositionUserRequest param)
        {
            List<GetSubjectByUserResult> listStudentEnrollment = new List<GetSubjectByUserResult>();

            foreach(var IdTeacherPosition in param.IdTeacherPosition)
            {
                if (IdTeacherPosition == "All")
                {
                    var listIdLesson = listStudentEnrollment.Select(e => e.Lesson.Id).Distinct().ToList();
                    var newListStudentEnrollment = param.ListStudentEnrollment.Where(e => !listIdLesson.Contains(e.Lesson.Id)).ToList();
                    listStudentEnrollment.AddRange(newListStudentEnrollment);
                }
                else
                {
                    #region LessonTeacher
                    if (param.ListPosition.Where(e => e == PositionConstant.SubjectTeacher).Any())
                    {
                        var listIdLesson = listStudentEnrollment.Select(e => e.Lesson.Id).Distinct().ToList();
                        var newListStudentEnrollment = param.ListSubjectTeacher.Where(e => !listIdLesson.Contains(e.Lesson.Id)).ToList();
                        listStudentEnrollment.AddRange(param.ListSubjectTeacher);
                    }
                    #endregion

                    #region HomeroomTeacher
                    var listIdHomeroom = param.ListHomeroomTeacher
                                        .Select(e => e.IdHomeroom)
                                        .Distinct()
                                        .ToList();

                    var listHomeroomTeacher = param.ListStudentEnrollment
                                        .Where(e => listIdHomeroom.Contains(e.Homeroom.Id))
                                        .GroupBy(e => new
                                        {
                                            IdLesson = e.Lesson.Id,
                                            IdSubject = e.Subject.Id,
                                            Subject = e.Subject.Description,
                                            SubjectCode = e.Subject.Code,
                                            IdGrade = e.Grade.Id,
                                            Grade = e.Grade.Description,
                                            GradeCode = e.Grade.Code,
                                            GradeOrder = e.Grade.OrderNumber,
                                            IdLevel = e.Level.Id,
                                            Level = e.Level.Description,
                                            LevelCode = e.Level.Code,
                                            LevelOrder = e.Level.OrderNumber,
                                            ClassId = e.Lesson.ClassId
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
                                            }
                                        })
                                        .ToList();

                    listStudentEnrollment.AddRange(listHomeroomTeacher);
                    #endregion

                    #region NonTeachingLoad
                    var listTeacherNonTeaching = param.ListNonTeachingLoad
                                .Where(e => e.MsNonTeachingLoad.IdTeacherPosition == IdTeacherPosition)
                                .ToList();

                    foreach (var item in listTeacherNonTeaching)
                    {
                        var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                        _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                        _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                        _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
                        _dataNewPosition.TryGetValue("Streaming", out var _StreamingPosition);
                        _dataNewPosition.TryGetValue("Classroom", out var _ClassroomPosition);

                        var _listStudentEnrollment = param.ListStudentEnrollment.ToList();
                        if (_DepartemenPosition != null)
                        {
                            var listIdLevelByDept = param.ListDepartmentLevel.Select(e => e.IdLevel).Distinct().ToList();
                            _listStudentEnrollment = _listStudentEnrollment
                                                    .Where(e => listIdLevelByDept.Contains(e.Level.Id))
                                                    .ToList();
                        }

                        if (_GradePosition != null)
                            _listStudentEnrollment = _listStudentEnrollment
                                                    .Where(e => e.Grade.Id == _GradePosition.Id)
                                                    .ToList();

                        if (_LevelPosition != null)
                            _listStudentEnrollment = _listStudentEnrollment
                                                    .Where(e => e.Level.Id == _LevelPosition.Id)
                                                    .ToList();

                        if (_SubjectPosition != null)
                            _listStudentEnrollment = _listStudentEnrollment
                                                    .Where(e => e.Subject.Id == _SubjectPosition.Id)
                                                    .ToList();

                        if (_StreamingPosition != null)
                            _listStudentEnrollment = _listStudentEnrollment
                                                    .Where(e => e.Homeroom.IdGradePathway == _StreamingPosition.Id)
                                                    .ToList();

                        if (_ClassroomPosition != null)
                            _listStudentEnrollment = _listStudentEnrollment
                                                    .Where(e => e.Homeroom.IdGradePathwayClassRoom == _ClassroomPosition.Id)
                                                    .ToList();

                        var listIdLesson = listStudentEnrollment.Select(e => e.Lesson.Id).Distinct().ToList();
                        var newListStudentEnrollment = _listStudentEnrollment.Where(e => !listIdLesson.Contains(e.Lesson.Id)).ToList();
                        listStudentEnrollment.AddRange(_listStudentEnrollment);
                    }
                    #endregion
                }
            }
           

            var items = listStudentEnrollment
                        .GroupBy(e => new
                        {
                            IdLesson = e.Lesson.Id,
                            IdSubject = e.Subject.Id,
                            Subject = e.Subject.Description,
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

            return items;
        }
    }

    public class GetPositionUserRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public List<string> IdTeacherPosition { get; set; }
        public List<GetSubjectByUserResult> ListStudentEnrollment { get; set; }
        public List<string> ListPosition { get; set; }
        public List<GetSubjectByUserResult> ListSubjectTeacher { get; set; }
        public List<MsHomeroomTeacher> ListHomeroomTeacher { get; set; }
        public List<TrNonTeachingLoad> ListNonTeachingLoad { get; set; }
        public List<MsDepartmentLevel> ListDepartmentLevel { get; set; }
    }
}
