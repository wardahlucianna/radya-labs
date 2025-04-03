using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class GetTeacherByLessonHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetTeacherByLessonRequest.IdLesson) };
        private readonly ISchedulingDbContext _dbContext;
        
        public GetTeacherByLessonHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherByLessonRequest>(_requiredParams);

            var teachers = await _dbContext.Entity<MsLessonTeacher>()
                                           .Where(x => x.IdLesson == param.IdLesson)
                                           .Select(x => new DataModelGeneral
                                           {
                                               Id = x.IdUser,
                                               Code = x.Staff.ShortName,
                                               Description = !string.IsNullOrEmpty(x.Staff.FirstName) ? x.Staff.FirstName : x.Staff.LastName,
                                               IdFromMasterData = x.IdUser,
                                               DataIsUseInMaster = true
                                           })
                                           .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(teachers as object);
        }
    }
}
