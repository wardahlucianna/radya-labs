using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.Lesson.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class AddLessonHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;
        public AddLessonHandler(ISchedulingDbContext dbContext, IMessage messageService, IEventSchool eventSchool)
        {
            _dbContext = dbContext;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddLessonRequest, AddLessonValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var QueryLesson = _dbContext.Entity<MsLesson>()
                .Where(x
                    => x.IdAcademicYear == body.IdAcadyear
                    && x.IdGrade == body.IdGrade
                    && x.IdSubject == body.IdSubject);

            if (body.Semester == 2)
            {
                QueryLesson = QueryLesson.Where(e => e.Semester == 2);
            }

            var existLesson = await QueryLesson.ToListAsync(CancellationToken);

            var semesters = new[] { body.Semester }.AsEnumerable();
            if (body.Semester == 1)
            {
                var querySemester = await _dbContext.Entity<MsPeriod>()
                    .Where(x => x.IdGrade == body.IdGrade && x.Semester > body.Semester)
                    .Select(x => x.Semester)
                    .Distinct()
                    .ToListAsync(CancellationToken);

                if (querySemester.Count != 0)
                    semesters = semesters.Concat(querySemester);
            }

            foreach (var semester in semesters)
            {
                var existClassIds = existLesson.Where(x => body.Lessons.Select(y => y.ClassIdGenerated).Contains(x.ClassIdGenerated) && x.Semester == semester).ToList();
                var coba = existClassIds.Select(x => x.ClassIdGenerated).FirstOrDefault();
                if (existClassIds.Count != 0)
                {
                    if (semester == 1)
                    {
                        throw new BadRequestException($"Lesson with ClassID: {existClassIds.Select(x => x.ClassIdGenerated).FirstOrDefault()} with semester: {semester} is already exists.");
                    }
                    else 
                    {
                        semesters = semesters.Where(x=>x==1).AsEnumerable();
                    }
                }
            }

            var duplicateClassIds = body.Lessons.GroupBy(x => x.ClassIdGenerated).Where(x => x.Count() > 1)
                                                .Select(x => x.First().ClassIdGenerated)
                                                .ToArray();
            if (duplicateClassIds.Length != 0)
                throw new BadRequestException($"You entered multiple class id on {string.Join(", ", duplicateClassIds)}.");

            var idHomerooms = body.Lessons.SelectMany(x => x.Homerooms.Select(y => y.IdHomeroom));
            var GetGradePathwayClassRoom = await _dbContext.Entity<MsHomeroom>()
                .Where(x => idHomerooms.Contains(x.Id))
                .Select(e => e.IdGradePathwayClassRoom)
                .Distinct().ToListAsync(CancellationToken);

            var GetHomerooms = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.Grade)
                .Include(x => x.HomeroomPathways)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Where(x => GetGradePathwayClassRoom.Contains(x.IdGradePathwayClassRoom))
                .ToListAsync(CancellationToken);

            //var existHomerooms = await _dbContext.Entity<MsHomeroom>()
            //    .Include(x => x.HomeroomPathways)
            //    .Where(x => idHomerooms.Contains(x.Id))
            //    .ToListAsync(CancellationToken);

            var ListPathwaySemester2 = new List<string>();
            // loop by semester
            foreach (var semester in semesters)
            {
                foreach (var lesson in body.Lessons)
                {
                    // add lesson
                    var newLesson = new MsLesson
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = body.IdAcadyear,
                        Semester = semester,
                        IdGrade = body.IdGrade,
                        IdSubject = body.IdSubject,
                        ClassIdFormat = body.ClassIdFormat,
                        ClassIdExample = body.ClassIdExample,
                        IdWeekVariant = lesson.IdWeekVarian,
                        ClassIdGenerated = lesson.ClassIdGenerated,
                        TotalPerWeek = lesson.TotalPerWeek,
                        HomeroomSelected = string.Join(", ", lesson.Homerooms.Select(x => x.Homeroom))
                    };
                    _dbContext.Entity<MsLesson>().Add(newLesson);

                    // add teachers
                    foreach (var teacher in lesson.Teachers)
                    {
                        var newLessonPathwayTeacher = new MsLessonTeacher
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdLesson = newLesson.Id,
                            IdUser = teacher.IdTeacher,
                            IsPrimary = teacher.IsPrimary,
                            IsAttendance = teacher.HasAttendance,
                            IsScore = teacher.HasScore,
                            IsClassDiary = teacher.IsClassDiary,
                            IsLessonPlan = teacher.IsLessonPlan
                        };
                        _dbContext.Entity<MsLessonTeacher>().Add(newLessonPathwayTeacher);
                    }

                    // add lesson pathways
                    var GetHomeroomPerSemester = new List<MsHomeroom>();

                    if (lesson.Homerooms.Select(x => x.Homeroom) != null)
                    {
                        GetHomeroomPerSemester = GetHomerooms.Where(e => e.Semester == semester && lesson.Homerooms.Any(x => x.Homeroom.Contains(e.GradePathwayClassroom.Classroom.Code))).ToList();
                    }
                    else
                    {
                        GetHomeroomPerSemester = GetHomerooms.Where(e => e.Semester == semester).ToList();
                    }
                    foreach (var item in GetHomeroomPerSemester)
                    {
                        foreach (var pathway in item.HomeroomPathways)
                        {
                            if (lesson.Homerooms.Any(x => x.IdPathways.Contains(pathway.Id)) || ListPathwaySemester2.Contains(pathway.Id))
                            {
                                var newLessonPathway = new MsLessonPathway
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdLesson = newLesson.Id,
                                    IdHomeroomPathway = pathway.Id,
                                };
                                _dbContext.Entity<MsLessonPathway>().Add(newLessonPathway);

                                if (semester == 1)
                                {
                                    var GetHomeroomSemester2 = new List<MsHomeroom>();

                                    if (lesson.Homerooms.Select(x => x.Homeroom) != null)
                                    {
                                        GetHomeroomSemester2 = GetHomerooms.Where(e => e.Semester == 2 && lesson.Homerooms.Any(x => x.Homeroom.Contains(e.GradePathwayClassroom.Classroom.Code))).ToList();
                                    }
                                    else
                                    {
                                        GetHomeroomSemester2 = GetHomerooms.Where(e => e.Semester == 2).ToList();
                                    }

                                    var idHomeroomPathway = GetHomeroomSemester2.SelectMany(x => x.HomeroomPathways
                                                                                    .Where(y=> y.IdGradePathwayDetail == pathway.IdGradePathwayDetail 
                                                                                    && y.Homeroom.Grade.Code==item.Grade.Code 
                                                                                    && y.Homeroom.GradePathwayClassroom.Classroom.Code==item.GradePathwayClassroom.Classroom.Code
                                                                                    && y.Homeroom.Semester==2)
                                                                                .Select(y=> y.Id)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(idHomeroomPathway))
                                        ListPathwaySemester2.Add(idHomeroomPathway);
                                }
                            }
                        }
                    }

                    //foreach (var homeroom in lesson.Homerooms)
                    //{
                    //    var existHomeroom = existHomerooms.Find(x => homeroom.IdHomeroom == x.Id);
                    //    if (existHomeroom is null)
                    //        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", homeroom.IdHomeroom));

                    //    // create MsLessonPathway based on MsHomeroomPathway from MsHomeroom
                    //    foreach (var pathway in homeroom.IdPathways)
                    //    {
                    //        var existLessonPathway = existHomeroom.HomeroomPathways.FirstOrDefault(x => x.Id == pathway);
                    //        if (existLessonPathway is null)
                    //            throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Pathway"], "Id", pathway));

                    //        var newLessonPathway = new MsLessonPathway
                    //        {
                    //            Id = Guid.NewGuid().ToString(),
                    //            IdLesson = newLesson.Id,
                    //            IdHomeroomPathway = pathway
                    //        };
                    //        _dbContext.Entity<MsLessonPathway>().Add(newLessonPathway);
                    //    }
                    //}
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                .Where(x => x.Id == body.IdAcadyear)
                .Select(e => e.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = idSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = idSchool
            });
            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }
    }
}
