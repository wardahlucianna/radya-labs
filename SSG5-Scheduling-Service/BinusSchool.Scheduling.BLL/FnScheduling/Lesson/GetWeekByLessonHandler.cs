using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class GetWeekByLessonHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetWeekByLessonHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetWeekByLessonRequest>(nameof(GetWeekByLessonRequest.IdLesson));

            var weeks = await _dbContext.Entity<MsLesson>()
                                        .Include(x => x.WeekVariant).ThenInclude(x => x.WeekVarianDetails).ThenInclude(x => x.Week)
                                        .Where(x => x.Id == param.IdLesson)
                                        .SelectMany(x => x.WeekVariant.WeekVarianDetails)
                                        .Where(x => x.Week.IsActive
                                                    && !string.IsNullOrEmpty(param.Search) ? EF.Functions.Like(x.Week.Description, $"%{param.Search}%") : true)
                                        .ToListAsync();

            return Request.CreateApiResult2(weeks.Select(x => new DataModelGeneral
            {
                Id = x.Id,
                Code = x.Week.Code,
                Description = x.Week.Description,
                IdFromMasterData = x.Week.Id,
                DataIsUseInMaster = true
            }).ToList() as object);
        }
    }
}
