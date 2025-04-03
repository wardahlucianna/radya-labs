using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting
{
    public class GetWeekSettingEditHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public GetWeekSettingEditHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetWeekSettingEditRequest>(nameof(GetWeekSettingEditRequest.IdWeekSetting));

            var weekSetting = _dbContext.Entity<MsWeekSetting>()
                .Include(x => x.Period)
                    .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                .Include(x => x.WeekSettingDetails)
                .FirstOrDefault(x => x.Id == param.IdWeekSetting);
            if (weekSetting == null)
                throw new NotFoundException("Week setting not found");

            var result = new GetWeekSettingEditResult
            {
                AcademicYear = new CodeWithIdVm {
                    Id = weekSetting.Period.Grade.Level.IdAcademicYear,
                    Code = weekSetting.Period.Grade.Level.AcademicYear.Code,
                    Description = weekSetting.Period.Grade.Level.AcademicYear.Description
                },
                Level = new CodeWithIdVm {
                    Id = weekSetting.Period.Grade.IdLevel,
                    Code = weekSetting.Period.Grade.Level.Code,
                    Description = weekSetting.Period.Grade.Level.Description
                },
                Grade = new CodeWithIdVm {
                    Id = weekSetting.Period.IdGrade,
                    Code = weekSetting.Period.Grade.Code,
                    Description = weekSetting.Period.Grade.Description
                },
                Term = new CodeWithIdVm {
                    Id = weekSetting.IdPeriod,
                    Code = weekSetting.Period.Code,
                    Description = weekSetting.Period.Description
                },
                Method = weekSetting.Method,
                TotalWeek = weekSetting.WeekSettingDetails.Count
            };

            return Task.FromResult(Request.CreateApiResult2(result as object));
        }
    }
}