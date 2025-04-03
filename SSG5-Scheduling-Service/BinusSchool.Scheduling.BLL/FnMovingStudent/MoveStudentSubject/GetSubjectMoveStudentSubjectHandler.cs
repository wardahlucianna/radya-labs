using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class GetSubjectMoveStudentSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetSubjectMoveStudentSubjectHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSubjectMoveStudentSubjectRequest>();

            var predicate = PredicateBuilder.Create<MsLessonPathway>(x => x.IsActive);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(e => e.Lesson.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(e => e.Lesson.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(e => e.HomeroomPathway.IdHomeroom == param.IdHomeroom);

            if(!string.IsNullOrEmpty(param.IdHomeroomOld) && !string.IsNullOrEmpty(param.IdLessonOld))
            {
                if(param.IdHomeroomOld==param.IdHomeroom)
                    predicate = predicate.And(e => e.Lesson.Id != param.IdLessonOld);
            }

            var items = await _dbContext.Entity<MsLessonPathway>()
                        .Include(e => e.Lesson).ThenInclude(e => e.Subject).ThenInclude(e => e.SubjectMappingSubjectLevels).ThenInclude(e => e.SubjectLevel)
                        .Where(predicate)
                        .OrderBy(e => e.Lesson.Subject.Description)
                        .Select(e => new GetSubjectMoveStudentSubjectResult
                        {
                            IdLesson = e.IdLesson,
                            IdSubject = e.Lesson.Subject.Id,
                            Subject = e.Lesson.Subject.Description,
                            IdSubjectLevel = e.Lesson.Subject.SubjectMappingSubjectLevels.Any()
                                            ? e.Lesson.Subject.SubjectMappingSubjectLevels.Select(e=>e.SubjectLevel.Id).FirstOrDefault()
                                            : null,
                            SubjectLevel = e.Lesson.Subject.SubjectMappingSubjectLevels.Any()
                                            ? e.Lesson.Subject.SubjectMappingSubjectLevels.Select(e => e.SubjectLevel.Code).FirstOrDefault()
                                            : null,
                            ClassId = e.Lesson.ClassIdGenerated
                        })
                        .ToListAsync(CancellationToken);

            items = items
                .GroupBy(e => new
                {
                    IdLesson = e.IdLesson,
                    IdSubject = e.IdSubject,
                    Subject = e.Subject,
                    IdSubjectLevel = e.IdSubjectLevel,
                    SubjectLevel = e.SubjectLevel,
                    ClassId = e.ClassId
                })
                .Select(e => new GetSubjectMoveStudentSubjectResult
                {
                    IdLesson = e.Key.IdLesson,
                    IdSubject = e.Key.IdSubject,
                    Subject = e.Key.Subject,
                    IdSubjectLevel = e.Key.IdSubjectLevel,
                    SubjectLevel = e.Key.SubjectLevel,
                    ClassId = e.Key.ClassId
                }).ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
