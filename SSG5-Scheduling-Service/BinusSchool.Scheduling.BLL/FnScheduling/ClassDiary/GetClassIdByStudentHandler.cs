using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.EntityFrameworkCore;
namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class GetClassIdByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetClassIdByStudentHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var body = await Request.ValidateBody<GetClassIdByStudentRequest, GetClassIdByTeacherRequestValidator>();

            var ReturnResult = await (from LessonPathway in _dbContext.Entity<MsLessonPathway>()
                                           join HomeroomPathway in _dbContext.Entity<MsHomeroomPathway>() on LessonPathway.IdHomeroomPathway equals HomeroomPathway.Id
                                           join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomPathway.IdHomeroom equals Homeroom.Id
                                           join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on Homeroom.Id equals HomeroomStudent.IdHomeroom
                                           join Grade in _dbContext.Entity<MsGrade>() on Homeroom.IdGrade equals Grade.Id
                                           join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                           join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                                           join Lesson in _dbContext.Entity<MsLesson>() on LessonPathway.IdLesson equals Lesson.Id
                                           where Lesson.IdAcademicYear == body.AcademicYearId && Lesson.IdGrade == body.GradeId && Lesson.IdSubject == body.SubjectId
                                         && body.HomeroomId.Contains(Homeroom.Id) && Homeroom.Semester == body.Semester && HomeroomStudent.IdStudent == body.UserId
                                       select new ItemValueVm()
                                       {
                                           Id = Lesson.Id,
                                           Description = Lesson.ClassIdGenerated
                                       }).Distinct().ToListAsync(CancellationToken);

            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}
