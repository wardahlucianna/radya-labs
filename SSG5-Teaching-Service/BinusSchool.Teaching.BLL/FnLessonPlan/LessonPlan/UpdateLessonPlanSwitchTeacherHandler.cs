using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class UpdateLessonPlanSwitchTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public UpdateLessonPlanSwitchTeacherHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateLessonPlanSwitchTeacherRequest, UpdateLessonPlanSwitchTeacherValidator>();

            var lessonPlan = await _dbContext.Entity<TrLessonPlan>()
                .Include(x => x.LessonTeacher)
                .IgnoreQueryFilters()
                .Where(x => x.LessonTeacher.IdLesson == body.IdLesson)
                .ToListAsync(CancellationToken);

            
            if (lessonPlan.Count == 0 || lessonPlan.Any(x=> x.LessonTeacher.IsActive))
            {
                return Request.CreateApiResult2();
            }
            else
            {
                lessonPlan = lessonPlan.Where(x => !x.LessonTeacher.IsActive).ToList();
                foreach(var lesson in lessonPlan)
                {
                    lesson.IdLessonTeacher = body.IdLessonTeacher;
                    _dbContext.Entity<TrLessonPlan>().Update(lesson);
                }
                await _dbContext.SaveChangesAsync(CancellationToken);
            }
            return Request.CreateApiResult2();
        }
    }
}
