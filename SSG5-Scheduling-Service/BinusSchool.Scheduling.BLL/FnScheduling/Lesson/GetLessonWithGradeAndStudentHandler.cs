using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchedulingDb.Abstractions;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class GetLessonWithGradeAndStudentHandler: FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetLessonWithGradeAndStudentHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }
    }
}
