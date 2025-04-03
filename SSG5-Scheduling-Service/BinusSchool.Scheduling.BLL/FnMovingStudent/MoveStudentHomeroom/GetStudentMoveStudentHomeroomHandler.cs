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
using BinusSchool.Common.Utils;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom
{
    public class GetStudentMoveStudentHomeroomHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentMoveStudentHomeroomHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentMoveStudentHomeroomRequest>();

            var listIdSubjectByHomeroomOld = await _dbContext.Entity<MsLessonPathway>()
                                                   .Include(e => e.HomeroomPathway)
                                                   .Include(e => e.Lesson)
                                                   .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                                               && e.Lesson.Semester == param.Semester
                                                               && e.HomeroomPathway.IdHomeroom == param.IdHomeroomOld)
                                                   .Select(e=>e.Lesson.IdSubject)
                                                   .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                            .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Student)
                                            .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                                            .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                            .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                                        && e.Lesson.Semester == param.Semester
                                                        && e.HomeroomStudent.IdHomeroom == param.IdHomeroomOld)
                                            .ToListAsync(CancellationToken);


            var listStudentExcludeLesson = listHomeroomStudentEnrollment
                                        .Where(e => !listIdSubjectByHomeroomOld.Contains(e.IdSubject))
                                        .Select(e => e.HomeroomStudent.IdStudent)
                                        .Distinct()
                                        .ToList();

            var listStundetByHomeroomOld = listHomeroomStudentEnrollment
                                            .GroupBy(e => new 
                                            {
                                                e.IdHomeroomStudent,
                                                e.HomeroomStudent.IdStudent,
                                                e.HomeroomStudent.Student.FirstName,
                                                e.HomeroomStudent.Student.MiddleName,
                                                e.HomeroomStudent.Student.LastName,
                                                GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                                                ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                                LevelCode = e.HomeroomStudent.Homeroom.Grade.Level.Code
                                            })
                                            .Select(e => new GetStudentMoveStudentHomeroomResult
                                            {
                                                IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                                IdStudent = e.Key.IdStudent,
                                                FullName = NameUtil.GenerateFullName(e.Key.FirstName, e.Key.MiddleName, e.Key.LastName),
                                                Grade = e.Key.GradeCode,
                                                Homeroom = e.Key.GradeCode + e.Key.ClassroomCode,
                                                IsCanMove = listStudentExcludeLesson.Where(f=>f==e.Key.IdStudent).Any()?false:true,
                                                Level = e.Key.LevelCode
                                            })
                                            .ToList();


            return Request.CreateApiResult2(listStundetByHomeroomOld as object);
        }
    }
}
