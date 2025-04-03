using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.Lesson.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class AddLessonCopyHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public AddLessonCopyHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddLessonCopyRequest, AddLessonCopyValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var ListLesson = await _dbContext.Entity<MsLesson>()
                .Where(e => body.IdLesson.Contains(e.Id))
                .Select(x => new
                {
                    Grade = new CodeWithIdVm
                    {
                        Id = x.IdGrade,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description
                    },
                    IdSubject = x.IdSubject,
                    ClassIdFormat = x.ClassIdFormat,
                    ClassIdExample = x.ClassIdExample,
                    IdWeekVariant = x.IdWeekVariant,
                    ClassIdGenerated = x.ClassIdGenerated,
                    TotalPerWeek = x.TotalPerWeek,
                    Homerooms = x.LessonPathways.Select(y => new LessonHomeroomDetail
                    {
                        Id = y.HomeroomPathway.IdHomeroom,
                        Code = y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                        Description = y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Description,
                        Pathways = new[]
                        {
                            new CodeWithIdVm
                            {
                                Id = y.IdHomeroomPathway,
                                Code = y.HomeroomPathway.GradePathwayDetail.Pathway.Code,
                                Description = y.HomeroomPathway.GradePathwayDetail.Pathway.Description
                            }
                        }
                    }),
                    HomeroomSelected = x.HomeroomSelected,
                    LessonTeacher = x.LessonTeachers
                }).ToListAsync(CancellationToken);

            var ListLessonCopyTo = await _dbContext.Entity<MsLesson>()
                .Include(e=>e.LessonPathways)
                .Where(x => x.IdAcademicYear == body.IdAcadyearCopyTo && x.Semester == body.SemesterCopyTo)
                .ToListAsync(CancellationToken);

            var listLessonPathway = ListLessonCopyTo.SelectMany(e => e.LessonPathways).ToList();

            foreach (var itemBodyListLesson in ListLesson)
            {
                var existClassIds = ListLessonCopyTo.Where(x => x.ClassIdGenerated == itemBodyListLesson.ClassIdGenerated && x.Semester == body.SemesterCopyTo).ToList();
                string idLesson = null;
               
                // get Homeroom by semester copy to
                var dataHomeroomPathway = _dbContext.Entity<MsHomeroom>()
                    .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                    .Include(x => x.HomeroomPathways).ThenInclude(x => x.GradePathwayDetail).ThenInclude(x => x.Pathway)
                    .Where(e => e.IdAcademicYear == body.IdAcadyearCopyTo && e.Semester == body.SemesterCopyTo && e.IdGrade == itemBodyListLesson.Grade.Id)
                    .ToList();

                if (existClassIds.Count == 0)
                {
                    // add lesson
                    var newLesson = new MsLesson
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = body.IdAcadyearCopyTo,
                        Semester = body.SemesterCopyTo,
                        IdGrade = itemBodyListLesson.Grade.Id,
                        IdSubject = itemBodyListLesson.IdSubject,
                        ClassIdFormat = itemBodyListLesson.ClassIdFormat,
                        ClassIdExample = itemBodyListLesson.ClassIdExample,
                        IdWeekVariant = itemBodyListLesson.IdWeekVariant,
                        ClassIdGenerated = itemBodyListLesson.ClassIdGenerated,
                        TotalPerWeek = itemBodyListLesson.TotalPerWeek,
                        HomeroomSelected = itemBodyListLesson.HomeroomSelected
                    };
                    _dbContext.Entity<MsLesson>().Add(newLesson);
                    idLesson = newLesson.Id;

                    if (itemBodyListLesson.LessonTeacher.Count != 0)
                    {
                        // add teachers
                        foreach (var lessonTeacher in itemBodyListLesson.LessonTeacher)
                        {
                            var newLessonPathwayTeacher = new MsLessonTeacher
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdLesson = idLesson,
                                IdUser = lessonTeacher.IdUser,
                                IsPrimary = lessonTeacher.IsPrimary,
                                IsAttendance = lessonTeacher.IsAttendance,
                                IsScore = lessonTeacher.IsScore
                            };
                            _dbContext.Entity<MsLessonTeacher>().Add(newLessonPathwayTeacher);
                        }
                    }
                    else
                    {
                        throw new BadRequestException($"Class ID {itemBodyListLesson.ClassIdGenerated} don't have Lesson Teacher.");
                    }
                }
                else
                    idLesson = existClassIds.Select(e => e.Id).FirstOrDefault();

                if (itemBodyListLesson.Homerooms.Count() != 0)
                {
                    // add lesson pathways
                    foreach (var homeroom in itemBodyListLesson.Homerooms)
                    {
                        var getHomeroomByCode = dataHomeroomPathway.Where(x => x.GradePathwayClassroom.Classroom.Code == homeroom.Code)
                            .Select(e => new
                            {
                                Id = e.Id,
                                Code = e.GradePathwayClassroom.Classroom.Code,
                                Description = e.GradePathwayClassroom.Classroom.Description,
                                HomeroomPathways = e.HomeroomPathways.Select(x => new CodeWithIdVm
                                {
                                    Id = x.Id,
                                    Code = x.GradePathwayDetail.Pathway.Code,
                                    Description = x.GradePathwayDetail.Pathway.Description
                                }).ToList(),
                            })
                            .FirstOrDefault();

                        if (getHomeroomByCode.HomeroomPathways.Count != 0)
                        {
                            foreach (var itemPathway in getHomeroomByCode.HomeroomPathways)
                            {
                                var existHomeroomPathways = homeroom.Pathways.Any(x => x.Code == itemPathway.Code);

                                if (existHomeroomPathways)
                                {
                                    var exsisLessonPathway = listLessonPathway.Where(e => e.IdLesson == idLesson && e.IdHomeroomPathway == itemPathway.Id).Any();

                                    if (!exsisLessonPathway)
                                    {
                                        var newLessonPathway = new MsLessonPathway
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdLesson = idLesson,
                                            IdHomeroomPathway = itemPathway.Id,
                                        };
                                        _dbContext.Entity<MsLessonPathway>().Add(newLessonPathway);
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new BadRequestException($"Cannot find homeroom pathways for Class ID {itemBodyListLesson.ClassIdGenerated} in semester {body.SemesterCopyTo}.");
                        }
                    }
                }
                else
                {
                    throw new BadRequestException($"Class ID {itemBodyListLesson.ClassIdGenerated} don't have Homerooms.");
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }
    }
}
