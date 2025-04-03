using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Teaching.FnLessonPlan.WeekSetting.Validator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting
{
    public class SaveWeekSettingDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public SaveWeekSettingDetailHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private WeekSettingDetail GetNextWeekSettingDetail(List<WeekSettingDetail> weekSettingDetails, int i)
        {
            WeekSettingDetail weekSettingDetail = null;
            for (var j = i + 1; j <= (weekSettingDetails.Count - 1); j++)
            {
                if (weekSettingDetails[j].Status) 
                {
                    weekSettingDetail = weekSettingDetails[j];
                    break;
                };
            }
            return weekSettingDetail;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SaveWeekSettingDetailRequest, SaveWeekSettingDetailValidator>();

            var weekSettingDetails = body.WeekSettingDetails.ToList();
            if (weekSettingDetails.Count > 2) 
            {
                for (var i = 0; i <= (weekSettingDetails.Count - 2); i++)
                {
                    var thisWeek = weekSettingDetails[i];
                    var nextWeek = GetNextWeekSettingDetail(weekSettingDetails, i);

                    if (nextWeek != null && thisWeek.DeadlineDate > nextWeek.DeadlineDate)
                        throw new Exception("Failed to save date settings because there is a deadline date that exceeds the deadline date in the following week");
                }
            }
            else if (weekSettingDetails.Count == 2)
            {
                var thisWeek = weekSettingDetails[0];
                var nextWeek = weekSettingDetails[1];

                if (thisWeek != null && nextWeek != null && thisWeek.DeadlineDate > nextWeek.DeadlineDate)
                    throw new Exception("Failed to save date settings because there is a deadline date that exceeds the deadline date in the following week");
            }
            
            var weekSetting = _dbContext
                .Entity<MsWeekSetting>()
                .Include(x => x.WeekSettingDetails)
                .FirstOrDefault(x => x.Id == body.IdWeekSetting);
            if (weekSetting == null)
                throw new NotFoundException("Week Setting not found");
            
            foreach (var id in weekSetting.WeekSettingDetails.Select(x => x.Id).ToList())
            {
                var wsd = _dbContext.Entity<MsWeekSettingDetail>().Find(id);
                var newWsd = weekSettingDetails.Where(x => x.IdWeekSettingDetail == id).FirstOrDefault();
                if (newWsd == null)
                    throw new NotFoundException("Date setting not found");

                wsd.DeadlineDate = newWsd.DeadlineDate;
                wsd.WeekNumber = newWsd.WeekNumber;
                wsd.Status = newWsd.Status;
            };

            if (body.WeekSettingDetails.Count == weekSetting.WeekSettingDetails.Count)
            {
                weekSetting.Status = true;
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}