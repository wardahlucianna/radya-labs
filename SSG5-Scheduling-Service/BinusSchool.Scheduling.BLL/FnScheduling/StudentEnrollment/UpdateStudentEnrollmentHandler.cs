using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using BinusSchool.Scheduling.FnSchedule.StudentEnrollment.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class UpdateStudentEnrollmentHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;
        private readonly IServiceProvider _provider;

        public UpdateStudentEnrollmentHandler(ISchedulingDbContext dbContext, IServiceProvider provider)
        {
            _dbContext = dbContext;
            _provider = provider;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateStudentEnrollmentRequest>();
            (await new UpdateStudentEnrollmentValidator(_provider).ValidateAsync(body)).EnsureValid();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var homeroom = await _dbContext.Entity<MsHomeroom>().FindAsync(new[] { body.IdHomeroom }, CancellationToken);
            if (homeroom is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", body.IdHomeroom));

            // return immediately if Enrolls is empty
            if (!(body.Enrolls?.Any() ?? false))
                return Request.CreateApiResult2();

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
                    .FirstOrDefaultAsync(x 
                        => x.IdAcademicYear == homeroom.IdAcademicYear
                        && x.Semester == 2
                        && x.IdGrade == homeroom.IdGrade
                        && x.IdGradePathwayClassRoom == homeroom.IdGradePathwayClassRoom
                        && x.IdGradePathway == homeroom.IdGradePathway, CancellationToken);
                if (homeroom2 is null)
                    throw new BadRequestException($"Homeroom {body.IdHomeroom} should have Semester 2.");
                idHomerooms = idHomerooms.Concat(new[] { homeroom2.Id });
            }

            var lessons = await _dbContext.Entity<MsLessonPathway>()
                .Where(x => idHomerooms.Contains(x.HomeroomPathway.IdHomeroom))
                .Select(x => x.Lesson)
                .Distinct()
                .ToListAsync(CancellationToken);

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
                .Where(x => idHomerooms.Contains(x.IdHomeroom))
                .ToListAsync(CancellationToken);

            var studentEnrollmentHistory = new Dictionary<string, object>();
            var updatedEnrollments = new List<MsHomeroomStudentEnrollment>();
            
            foreach (var enroll in body.Enrolls)
            {
                var joined = enroll.Lessons
                    .Join(lessons, l => l.IdLesson, ls => ls.Id, (l, lesson) => lesson)
                    .Join(subjectResult, l => l.IdSubject, s => s.Id, (lesson, subject) => (lesson, subject))
                    .GroupBy(x => x.subject.SubjectGroup?.Id);
                
                // notification data
                var oldIdLessons = enrollments
                    .Where(x => x.HomeroomStudent.IdStudent == enroll.IdStudent && x.HomeroomStudent.Homeroom.Semester == homeroom.Semester)
                    .Select(x => x.IdLesson);
                var newIdLessons = new List<string>();
                
                foreach (var item in joined)
                {
                    // make sure tick one subject on each subject group
                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        if (item.Count() > 1)
                            throw new BadRequestException($"Can only select one subject on each subject group {item.First().subject.SubjectGroup.Code}");
                    }
                }

                foreach (var lesson in enroll.Lessons)
                {
                    // update enrollment per semester
                    var currentStudent = students.Find(x => x.IdStudent == enroll.IdStudent && x.Homeroom.Semester == homeroom.Semester);
                    var currentLesson = lessons.Find(x => x.Id == lesson.IdLesson && x.Semester == homeroom.Semester);

                    if (currentStudent != null && currentLesson != null)
                    {
                        newIdLessons.Add(lesson.IdLesson);
                        var enrolled = enrollments.Find(x => x.Id == lesson.IdEnrollment && x.HomeroomStudent.Homeroom.Semester == homeroom.Semester);

                        // update if enrollment already exist
                        if (enrolled != null)
                        {
                            updatedEnrollments.Add(enrolled);
                            enrolled.IdHomeroomStudent = currentStudent.Id;
                            enrolled.IdLesson = lesson.IdLesson;
                            enrolled.IdSubjectLevel =  lesson.IdSubjectLevel;
                            enrolled.IsFromMaster = true;
                            enrolled.IdSubject = lesson.IdSubject;
                            //enrolled.Semester = 1;
                            _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(enrolled);
                        }
                        else
                        {
                            // make sure not any enrolled with requested idStudent & idLesson
                            enrolled = enrollments.Find(x => x.IdHomeroomStudent == currentStudent.Id && x.IdLesson == lesson.IdLesson);

                            if (enrolled != null)
                            {
                                updatedEnrollments.Add(enrolled);
                                enrolled.IdLesson = lesson.IdLesson;
                                enrolled.IdSubjectLevel = lesson.IdSubjectLevel;
                                enrolled.IsFromMaster = true;
                                enrolled.IdSubject = lesson.IdSubject;
                                //enrolled.Semester = 1;
                                _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(enrolled);
                            }
                            else
                            {
                                enrolled = new MsHomeroomStudentEnrollment
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdHomeroomStudent = currentStudent.Id,
                                    IdLesson = lesson.IdLesson,
                                    IdSubjectLevel = lesson.IdSubjectLevel,
                                    IsFromMaster = true,
                                    //Semester = 1,
                                    IdSubject = lesson.IdSubject
                                };
                                
                                _dbContext.Entity<MsHomeroomStudentEnrollment>().Add(enrolled);
                            }
                        }
                    }

                    // update second semester
                    if (hasSecondSemester)
                    {
                        var currentStudent2 = students.Find(x => x.IdStudent == enroll.IdStudent && x.Homeroom.Semester == 2);
                        var currentLesson2 = lessons.Find(x => x.ClassIdGenerated == currentLesson.ClassIdGenerated && x.Semester == 2);
                        
                        if (currentStudent2 != null && currentLesson2 != null)
                        {
                            // make sure not any enrolled with requested idStudent & idLesson
                            var enrolled2 = enrollments.Find(x => x.IdHomeroomStudent == currentStudent2.Id && x.IdLesson == currentLesson2.Id);

                            if (enrolled2 != null)
                            {
                                updatedEnrollments.Add(enrolled2);
                                enrolled2.IdLesson = currentLesson2.Id;
                                enrolled2.IdSubjectLevel = lesson.IdSubjectLevel;
                                enrolled2.IsFromMaster = true;
                                enrolled2.IdSubject = lesson.IdSubject;
                                //enrolled2.Semester = 2;
                                _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(enrolled2);
                            }
                            else
                            {
                                enrolled2 = new MsHomeroomStudentEnrollment
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdHomeroomStudent = currentStudent2.Id,
                                    IdLesson = currentLesson2.Id,
                                    IdSubjectLevel = lesson.IdSubjectLevel,
                                    IsFromMaster = true,
                                    //Semester = 2,
                                    IdSubject = lesson.IdSubject
                                };
                                _dbContext.Entity<MsHomeroomStudentEnrollment>().Add(enrolled2);
                            }
                        }
                    }
                }   

                var oldClassIds = oldIdLessons.Join(lessons, o => o, ls => ls.Id, (o, ls) => ls.ClassIdGenerated.TrimEnd()).ToArray();
                var newClassIds = newIdLessons.Join(lessons, o => o, ls => ls.Id, (o, ls) => ls.ClassIdGenerated.TrimEnd()).ToArray();
                studentEnrollmentHistory.Add(enroll.IdStudent, new Dictionary<string, IEnumerable<string>>
                {
                    {"old", oldClassIds},
                    {"new", newClassIds}
                });
            }

            // select unupdated enrollments and remove it
            var unupdatedEnrollments = enrollments
                // this will prevent new enrollment being evaluated
                .Where(x => x.IsActive) 
                // select only requested students
                .Where(x => body.Enrolls.Select(x => x.IdStudent).Distinct().Contains(x.HomeroomStudent.IdStudent))
                .Except(updatedEnrollments);
            if (unupdatedEnrollments.Any())
            {
                foreach (var unupdated in unupdatedEnrollments)
                {
                    unupdated.IsActive = false;
                    _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(unupdated);
                }
            }
            
            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            var dateAction = DateTimeUtil.ServerTime;
            
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var dict = new Dictionary<string, object>(studentEnrollmentHistory);
                dict.Add("date", dateAction);
                
                // to student
                var toStudentMessage = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "AYS9")
                {
                    KeyValues = dict
                });
                collector.Add(toStudentMessage);
                
                // to subject teacher
                dict.Add("idGrade", homeroom.IdGrade);
                var toSubjectTeacherMessage = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "AYS8")
                {
                    KeyValues = dict
                });
                collector.Add(toSubjectTeacherMessage);
            }
            
            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Dispose();
            return base.OnException(ex);
        }
    }
}
