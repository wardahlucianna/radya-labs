using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom
{

    public class GetHomeroomNewMoveStudentHomeroomHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetHomeroomNewMoveStudentHomeroomHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomNewMoveStudentHomeroomRequest>();

            var listIdSubjectByHomeroomOld = await _dbContext.Entity<MsLessonPathway>()
                                                    .Include(e => e.HomeroomPathway)
                                                    .Include(e => e.Lesson)
                                                    .Where(e=>e.Lesson.IdAcademicYear==param.IdAcademicYear 
                                                                && e.Lesson.Semester==param.Semester 
                                                                && e.HomeroomPathway.IdHomeroom==param.IdHomeroomOld)
                                                    .Select(e => e.Lesson.IdSubject)
                                                    .Distinct()
                                                    .ToListAsync(CancellationToken);

            var listLessonPathway = await _dbContext.Entity<MsLessonPathway>()
                                    .Include(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                    .Include(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                    .Where(e => listIdSubjectByHomeroomOld.Contains(e.Lesson.IdSubject)
                                        && e.Lesson.IdGrade == param.IdGradeOld
                                        && e.HomeroomPathway.IdHomeroom != param.IdHomeroomOld
                                        && e.Lesson.IdAcademicYear == param.IdAcademicYear
                                        && e.Lesson.Semester == param.Semester
                                        )
                                    .OrderBy(e => e.HomeroomPathway.Homeroom.Grade.Level.Code)
                                    .ThenBy(e => e.HomeroomPathway.Homeroom.Grade.Code)
                                    .ThenBy(e => e.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
                                    .Select(e => new
                                    {
                                        e.HomeroomPathway.IdHomeroom,
                                        GradeCode = e.HomeroomPathway.Homeroom.Grade.Code,
                                        ClassroomCode = e.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code
                                    })
                                    .ToListAsync(CancellationToken);

            var items = listLessonPathway
                            .GroupBy(e => new 
                            {
                                e.IdHomeroom,
                                e.GradeCode,
                                e.ClassroomCode
                            })
                            .Select(e => new ItemValueVm
                            {
                                Id = e.Key.IdHomeroom,
                                Description = e.Key.GradeCode + e.Key.ClassroomCode
                            })
                            .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
