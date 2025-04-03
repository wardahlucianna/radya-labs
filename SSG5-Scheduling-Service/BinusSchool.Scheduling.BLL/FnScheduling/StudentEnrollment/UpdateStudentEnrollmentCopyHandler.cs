using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.StudentEnrollment.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class UpdateStudentEnrollmentCopyHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;
        private readonly IServiceProvider _provider;
        private readonly IMachineDateTime _dateTime;

        public UpdateStudentEnrollmentCopyHandler(ISchedulingDbContext dbContext, IServiceProvider provider, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _provider = provider;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateStudentEnrollmentCopyRequest>();
            (await new UpdateStudentEnrollmentCopyValidator(_provider).ValidateAsync(body)).EnsureValid();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var homeroom = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.GradePathway).ThenInclude(x => x.Grade)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Where(x=> x.Id == body.IdHomeroom).FirstOrDefaultAsync(CancellationToken);
            if (homeroom is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", body.IdHomeroom));

            var hasSecondSemester = false;
            if (homeroom.Semester == 1)
            {
                var semesterResult = await _dbContext.Entity<MsPeriod>()
                .Where(x => x.IdGrade == homeroom.IdGrade)
                .Select(x => x.Semester)
                .ToListAsync(CancellationToken);

                hasSecondSemester = semesterResult.Any(x => x == 2);
            }

            var idHomerooms = new[] { homeroom.Id }.AsEnumerable();
            var homeroom2 = default(MsHomeroom);
            if (hasSecondSemester)
            {
                homeroom2 = await _dbContext.Entity<MsHomeroom>()
                    .Include(x => x.GradePathway).ThenInclude(x => x.Grade)
                    .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                    .FirstOrDefaultAsync(x 
                        => x.IdAcademicYear == homeroom.IdAcademicYear
                        && x.Semester == 2
                        && x.IdGrade == homeroom.IdGrade
                        && x.IdGradePathwayClassRoom == homeroom.IdGradePathwayClassRoom
                        && x.IdGradePathway == homeroom.IdGradePathway, CancellationToken);
                if (homeroom2 is null)
                    throw new BadRequestException($"Homeroom {string.Format("{0}{1}", homeroom.Grade.Code, homeroom.GradePathwayClassroom.Classroom.Description)} should have Semester 2.");
                idHomerooms = idHomerooms.Concat(new[] { homeroom2.Id });

                var studentSemester2 = await _dbContext.Entity<MsHomeroomStudent>()
                        .Where(x => idHomerooms.Contains(x.IdHomeroom)).ToListAsync();

                if (!studentSemester2.Any())
                    throw new BadRequestException($"Homeroom {string.Format("{0}{1}", homeroom2.Grade.Code, homeroom2.GradePathwayClassroom.Classroom.Description)} must have students in Semester 2.");
            }

            var lessons = await _dbContext.Entity<MsLessonPathway>()
                .Where(x => idHomerooms.Contains(x.HomeroomPathway.IdHomeroom))
                .Select(x => x.Lesson)
                .Distinct()
                .ToListAsync(CancellationToken);

            var lessonsms2 = lessons.Where(x => x.Semester == 2).ToList();
            if (!lessonsms2.Any())
                throw new BadRequestException($"Homeroom {string.Format("{0}{1}", homeroom.Grade.Code, homeroom.GradePathwayClassroom.Classroom.Description)} must have lesson in Semester 2.");

            #region GetSubjects
            var ListIdSubject = lessons.Select(x => x.IdSubject).Distinct();
            var predicate = PredicateBuilder.True<MsSubject>();
            var results = _dbContext.Entity<MsSubject>()
                    .Include(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                    .Include(x => x.SubjectMappingSubjectLevels).ThenInclude(x => x.SubjectLevel)
                    .Include(x => x.Department)
                    .Where(e => ListIdSubject.Contains(e.Id))
                    .ToList();

            // collect subject group here instead in query above
            var idSubjectGroups = results.Where(x => x.IdSubjectGroup != null).Select(x => x.IdSubjectGroup).Distinct().ToArray();
            var existSubjectGroups = new List<MsSubjectGroup>();
            if (idSubjectGroups.Length != 0)
            {
                existSubjectGroups = await _dbContext
                    .Entity<MsSubjectGroup>().Where(x => idSubjectGroups.Contains(x.Id))
                    .ToListAsync(CancellationToken);
            }

            var subjectResult = results
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
                    SubjectGroup = x.IdSubjectGroup != null && existSubjectGroups.Any(y => y.Id == x.IdSubjectGroup)
                        ? new CodeWithIdVm
                        {
                            Id = x.IdSubjectGroup,
                            Code = existSubjectGroups.Find(y => y.Id == x.IdSubjectGroup)?.Code,
                            Description = existSubjectGroups.Find(y => y.Id == x.IdSubjectGroup)?.Description
                        }
                        : null,
                    MaxSession = x.MaxSession
                })
                .ToList();
            #endregion

            var enrollments = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(x => x.HomeroomStudent).ThenInclude(x => x.Homeroom)
                .Where(x => idHomerooms.Contains(x.HomeroomStudent.IdHomeroom))
                .ToListAsync(CancellationToken);
            
            var students = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                .Include(x => x.HomeroomStudentEnrollments).ThenInclude(x=>x.Lesson)
                .Where(x => idHomerooms.Contains(x.IdHomeroom))
                .ToListAsync(CancellationToken);

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus })
                .Where(x => (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date
                    || (x.StartDate < _dateTime.ServerTime.Date
                        ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
                        : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

            if (checkStudentStatus.Any())
                students = students.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();

            var updatedEnrollments = new List<MsHomeroomStudentEnrollment>();

            foreach ( var idStudent in body.IdStudents)
            {
                var currentStudent = students.Find(x => x.IdStudent == idStudent && x.Homeroom.Semester == homeroom.Semester);

                var currentStudent2 = students.Find(x => x.IdStudent == idStudent && x.Homeroom.Semester == homeroom2.Semester);
                var lesson2 = lessons.Where(x => x.Semester == homeroom2.Semester).ToList();

                if (currentStudent2 == null)
                {
                    throw new BadRequestException($"Student subject with Id: {idStudent} has not been enrolled.");
                }

                foreach (var studentEnrollment in currentStudent.HomeroomStudentEnrollments)
                {
                    var currentLesson2 = lessons.Find(x => x.ClassIdGenerated == studentEnrollment.Lesson.ClassIdGenerated && x.Semester == homeroom2.Semester);

                    if (currentStudent2 != null && currentLesson2 != null)
                    {
                        // make sure one homeroom in semeter 1 and 2
                        if(homeroom.IdGrade == homeroom2.IdGrade && homeroom.GradePathwayClassroom.IdClassroom== homeroom2.GradePathwayClassroom.IdClassroom) 
                        {
                            // make sure not any enrolled with requested idStudent & idLesson
                            var enrolled2 = enrollments.Where(x => x.IdHomeroomStudent == currentStudent2.Id).ToList();

                            enrolled2.ForEach(e => e.IsActive = false);
                            _dbContext.Entity<MsHomeroomStudentEnrollment>().UpdateRange(enrolled2);

                            MsHomeroomStudentEnrollment newEnroll = new MsHomeroomStudentEnrollment
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdHomeroomStudent = currentStudent2.Id,
                                IdLesson = currentLesson2.Id,
                                IdSubjectLevel = studentEnrollment.IdSubjectLevel,
                                IsFromMaster = true,
                                IdSubject = studentEnrollment.IdSubject
                            };
                            _dbContext.Entity<MsHomeroomStudentEnrollment>().Add(newEnroll);
                        }
                        else
                            continue;



                        // make sure not any enrolled with requested idStudent & idLesson
                        //var enrolled2 = enrollments.Find(x => x.IdHomeroomStudent == currentStudent2.Id && x.IdLesson == currentLesson2.Id);

                        //if (enrolled2 != null)
                        //{
                        //    updatedEnrollments.Add(enrolled2);
                        //    enrolled2.IdLesson = currentLesson2.Id;
                        //    enrolled2.IdSubjectLevel = studentEnrollment.IdSubjectLevel;
                        //    enrolled2.IsFromMaster = true;
                        //    enrolled2.IdSubject = studentEnrollment.IdSubject;
                        //    _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(enrolled2);
                        //}
                        //else
                        //{
                        //    enrolled2 = new MsHomeroomStudentEnrollment
                        //    {
                        //        Id = Guid.NewGuid().ToString(),
                        //        IdHomeroomStudent = currentStudent2.Id,
                        //        IdLesson = currentLesson2.Id,
                        //        IdSubjectLevel = studentEnrollment.IdSubjectLevel,
                        //        IsFromMaster = true,
                        //        IdSubject = studentEnrollment.IdSubject
                        //    };
                        //    _dbContext.Entity<MsHomeroomStudentEnrollment>().Add(enrolled2);
                        //}
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Dispose();
            return base.OnException(ex);
        }
    }
}
