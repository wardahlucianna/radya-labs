using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting
{
    public class DeleteWeekSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public DeleteWeekSettingHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DeleteWeekSettingRequest>(nameof(DeleteWeekSettingRequest.IdWeekSetting));

            var weekSetting = _dbContext.Entity<MsWeekSetting>()
                .Include(x => x.WeekSettingDetails)
                    .ThenInclude(x => x.LessonPlans)
                .Include(x => x.Period)
                    .ThenInclude(x => x.Grade)
                .FirstOrDefault(x => x.Id == param.IdWeekSetting);
            if (weekSetting == null)
                throw new NotFoundException("Week Setting not found");

            // if (weekSetting.WeekSettingDetails.Any(x => DateTime.Now >= x.DeadlineDate))
            //     throw new Exception("Can't delete week setting because it's already on progress");

            if (weekSetting.WeekSettingDetails.Any(x => x.LessonPlans.Any(y => y.Status != "Unsubmitted")))
                throw new Exception("Can't set week setting because teacher already uploaded lesson plan to this week setting");

            var lessonPlans = await _dbContext.Entity<TrLessonPlan>()
                .Include(x => x.WeekSettingDetail)
                .Where(x => x.WeekSettingDetail.IdWeekSetting == param.IdWeekSetting)
                .ToListAsync(CancellationToken);

            foreach (var lp in lessonPlans)
            {
                lp.IsActive = false;
            }
        
            weekSetting.IsActive = false;
            foreach (var id in weekSetting.WeekSettingDetails.Select(x => x.Id))
            {
                var wsd = _dbContext.Entity<MsWeekSettingDetail>().Find(id);
                wsd.IsActive = false;
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}