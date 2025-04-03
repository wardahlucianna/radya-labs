using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetStudentEnrollmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetStudentEnrollmentHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentEnrollmentRequest>(nameof(GetStudentEnrollmentRequest.IdHomeroom));
            var columns = new[] { "lessons.name", "lessons.religion" };

            var homeroom = await _dbContext.Entity<MsHomeroom>()
                .FindAsync(new[] { param.IdHomeroom }, CancellationToken);
            if (homeroom is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", param.IdHomeroom));

            var students = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(e=>e.Student)
                .Where(x => x.IdHomeroom == param.IdHomeroom)
                .ToListAsync(CancellationToken);

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus })
            .Where(x => (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date
                || (x.StartDate < _dateTime.ServerTime.Date
                    ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
                    : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
            .ToListAsync();

            if (checkStudentStatus != null)
            {
                students = students.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();
            }

            var lessons = await _dbContext.Entity<MsLessonPathway>()
                .Where(x => x.HomeroomPathway.IdHomeroom == param.IdHomeroom && x.Lesson.Semester == homeroom.Semester)
                .Select(x => x.Lesson)
                .Distinct()
                .ToListAsync(CancellationToken);

            var enrollments = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Where(x => x.HomeroomStudent.IdHomeroom == param.IdHomeroom)
                .Select(x => new 
                {
                    x.Id,
                    x.IdLesson,
                    x.IdHomeroomStudent,
                    x.IdSubjectLevel
                })
                .ToListAsync(CancellationToken);

            #region GetClassroomMappedsByGrade
            var ListIdGrade = new[] { homeroom.IdGrade };
            var crMapResult = await _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.Classroom)
                .Include(x => x.MsGradePathway).ThenInclude(x => x.GradePathwayDetails).ThenInclude(x => x.Pathway)
                .Include(x => x.MsGradePathway).ThenInclude(x => x.Grade)
                .Where(x => x.MsGradePathway.IdGrade == homeroom.IdGrade)
                .Select(x => new GetClassroomMapByGradeResult
                {
                    Id = x.Id,
                    Code = x.Classroom.Code,
                    Description = x.Classroom.Description,
                    Formatted = $"{x.MsGradePathway.Grade.Code}{x.Classroom.Code}",
                    Grade = new CodeWithIdVm
                    {
                        Id = x.MsGradePathway.IdGrade,
                        Code = x.MsGradePathway.Grade.Code,
                        Description = x.MsGradePathway.Grade.Description
                    },
                    Pathway = new ClassroomMapPathway
                    {
                        Id = x.MsGradePathway.Id,
                        PathwayDetails = x.MsGradePathway.GradePathwayDetails.Select(y => new CodeWithIdVm
                        {
                            Id = y.Id,
                            Code = y.Pathway.Code,
                            Description = y.Pathway.Description
                        })
                    },
                    Class = new CodeWithIdVm
                    {
                        Id = x.Classroom.Id,
                        Code = x.Classroom.Code,
                        Description = x.Classroom.Description
                    }
                })
                .OrderBy(x => x.Grade.Code).ThenBy(x => x.Code)
                .ToListAsync(CancellationToken);
            #endregion

            #region GetSubject
            var ListIdSubject = lessons.Select(x => x.IdSubject);

            var result = await _dbContext.Entity<MsSubject>()
                    .Include(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                    .Include(x => x.SubjectMappingSubjectLevels).ThenInclude(x => x.SubjectLevel)
                    .Include(x => x.Department)
                    // .Include(x => x.SubjectGroup)
                .Where(e => ListIdSubject.Contains(e.Id))

                .ToListAsync();
            var idSubjectGroups = result.Where(x => x.IdSubjectGroup != null).Select(x => x.IdSubjectGroup).Distinct().ToArray();
            var existSubjectGroups = new List<MsSubjectGroup>();
            if (idSubjectGroups.Length != 0)
            {
                existSubjectGroups = await _dbContext
                    .Entity<MsSubjectGroup>().Where(x => idSubjectGroups.Contains(x.Id))
                    .ToListAsync(CancellationToken);
            }

            var subjectResult = result
                                .Select(x => new GetSubjectResult
                                {
                                    Id = x.Id,
                                    Code = x.Code,
                                    Description = x.Description,
                                    Grade = x.Grade.Description,
                                    Acadyear = x.Grade.Level.AcademicYear.Code,
                                    IdDepartment = x.IdDepartment,
                                    Department = x.Department.Description,
                                    SubjectId = x.SubjectID,
                                    SubjectLevel = x.SubjectMappingSubjectLevels.Count != 0 // force order asc one-to-many relation
                                            ? string.Join(", ", x.SubjectMappingSubjectLevels
                                                .OrderBy(y => y.SubjectLevel.Code).Select(y => y.SubjectLevel.Code))
                                            : Localizer["General"],
                                    SubjectLevels = x.SubjectMappingSubjectLevels.Count != 0 // force order asc one-to-many relation
                                            ? x.SubjectMappingSubjectLevels
                                                .OrderBy(y => y.SubjectLevel.Code)
                                                .Select(y => new SubjectLevelResult
                                                {
                                                    Id = y.SubjectLevel.Id,
                                                    Code = y.SubjectLevel.Code,
                                                    Description = y.SubjectLevel.Description,
                                                    IsDefault = y.IsDefault
                                                })
                                            : null,
                                    SubjectGroup = x.IdSubjectGroup != null && existSubjectGroups.Any(y => y.Id == x.IdSubjectGroup)
                                            ? new CodeWithIdVm
                                            {
                                                Id = x.IdSubjectGroup,
                                                Code = existSubjectGroups.Find(y => y.Id == x.IdSubjectGroup).Code,
                                                Description = existSubjectGroups.Find(y => y.Id == x.IdSubjectGroup).Description
                                            }
                                            : null,
                                    MaxSession = x.MaxSession
                                }).ToList();

            #endregion

            #region GetStudentsByGrade

            var ListIdUser = students.Select(x => x.IdStudent);
            var predicate = PredicateBuilder.Create<MsStudentGrade>(x => ListIdGrade.Contains(x.IdGrade));
            if (ListIdUser?.Any() ?? false)
                predicate = predicate.And(x => ListIdUser.Contains(x.IdStudent));

            var queryStudent = _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Student)
                .Include(x => x.StudentGradePathways)
                .Where(predicate)
                .IgnoreQueryFilters();

            var gradeResult = await _dbContext.Entity<MsGrade>()
                .Include(x => x.Level)
                .ThenInclude(x => x.AcademicYear.School)
                .Select(x => new GetGradeDetailResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                })
                .FirstOrDefaultAsync(x => ListIdGrade.Contains(x.Id), CancellationToken);

            var studentResult = queryStudent
                    .Select(x => new GetStudentByGradeResult
                    {
                        Id = x.Id,
                        StudentId = x.IdStudent,
                        Grade = gradeResult.Description,
                        FullName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                        Gender = x.Student.Gender,
                        IsActive = x.IsActive,
                    })
                    .ToList();
            #endregion

            var availableLessons = lessons
                .Join(subjectResult, lesson => lesson.IdSubject, subject => subject.Id, (lesson, subject) => (lesson, subject))
                .OrderByDescending(x => !string.IsNullOrEmpty(x.subject?.SubjectGroup?.Id))
                    .ThenBy(x => x.subject?.SubjectGroup?.Id)
                    .ThenBy(x => x.lesson.ClassIdGenerated);

            var rLessons = new List<IDictionary<string, object>>();
            foreach (var student in students)
            {
                var currentStudent = studentResult.FirstOrDefault(x => x.StudentId == student.IdStudent);
                var rLesson = new Dictionary<string, object>
                {
                    { "idStudent", student.IdStudent },
                    { "name", NameUtil.GenerateFullName(student.Student.FirstName,student.Student.MiddleName,student.Student.LastName) },
                    { "religion", student.Religion }
                };

                foreach (var (lesson, subject) in availableLessons)
                {
                    // find enrolled student to lesson
                    var currentEnrollment = enrollments.Find(x => x.IdLesson == lesson.Id && x.IdHomeroomStudent == student.Id);

                    //in the comments because when there was validation of the contents of the student's religious data, nothing came out. except for the student's religion
                    //if (currentEnrollment == null)
                    //    continue;

                    var rSubjectLevels = subject.SubjectLevels?.Select(x => new
                    {
                        id = x.Id,
                        code = x.Code,
                        isDefault = x.IsDefault,
                        isTick = currentEnrollment != null && currentEnrollment.IdSubjectLevel == x.Id
                    });
                    var rSubject = new
                    {
                        idEnrollment = currentEnrollment?.Id,
                        idLesson = lesson.Id,
                        code = subject?.Code,
                        group = subject?.SubjectGroup?.Id,
                        isTick = currentEnrollment != null,
                        subjectLevels = rSubjectLevels,
                        subjectId = subject?.Id
                    };

                    if(!rLesson.Keys.Any(x => x == lesson.ClassIdGenerated))
                        rLesson.Add(lesson.ClassIdGenerated, rSubject);
                }

                rLessons.Add(rLesson);
            }

            // filter by search
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                rLessons = rLessons
                    .Where(x
                        => (x["name"] as string).Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                        || (x["religion"] as string).Contains(param.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            param.OrderBy = "lessons.name";

            // order result
            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                rLessons = param.OrderBy switch
                {
                    "lessons.name" => param.OrderType == OrderType.Asc
                        ? rLessons.OrderBy(x => x["name"]).ToList()
                        : rLessons.OrderByDescending(x => x["name"]).ToList(),
                    "lessons.religion" => param.OrderType == OrderType.Asc
                        ? rLessons.OrderBy(x => x["religion"]).ToList()
                        : rLessons.OrderByDescending(x => x["religion"]).ToList(),
                    _ => rLessons
                };
            }

            var currentCrMap = crMapResult.FirstOrDefault(x => homeroom.IdGradePathwayClassRoom == x.Id);
            var _result = new GetStudentEnrollmentResult
            {
                Grade = new CodeWithIdVm
                {
                    Id = homeroom.IdGrade,
                    Code = currentCrMap?.Grade?.Code,
                    Description = currentCrMap?.Grade?.Description
                },
                Homeroom = new CodeWithIdVm
                {
                    Id = param.IdHomeroom,
                    Code = currentCrMap?.Code,
                    Description = currentCrMap?.Description
                },
                Semester = homeroom.Semester,
                LessonGroups = availableLessons.Any(x => x.subject.SubjectGroup != null)
                    ? availableLessons
                        .Where(x => x.subject.SubjectGroup != null)
                        .GroupBy(x => x.subject.SubjectGroup.Id, x => x.subject.SubjectGroup)
                        .Select(x => x.First()).ToList()
                    : null,
                Lessons = rLessons
            };

            return Request.CreateApiResult2(_result as object, new Dictionary<string, object>().AddColumnProperty(columns));
        }
    }
}
