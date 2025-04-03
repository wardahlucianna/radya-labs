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
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetTimelineBySupervisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetTimelineBySupervisorHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTimelineBySupervisorRequest>();

            var GetPeriod = await _dbContext.Entity<MsPeriod>()
                            .Include(e => e.Grade).ThenInclude(e => e.MsLevel)
                            .Where(e => param.IdAcademicYear.Contains(e.Grade.MsLevel.IdAcademicYear))
                            .ToListAsync(CancellationToken);

            //var GetExperience = await _dbContext.Entity<TrExperience>()
            //                .Include(e => e.HomeroomStudent)
            //                .Where(e => param.IdAcademicYear.Contains(e.IdAcademicYear) && e.IdUserSupervisor == param.IdUser && e.HomeroomStudent.IdStudent == param.IdStudent && (e.Status == ExperienceStatus.Approved || e.Status == ExperienceStatus.Completed))
            //                .ToListAsync(CancellationToken);

            var GetExperience = await _dbContext.Entity<TrExperience>()
                            .Include(e => e.HomeroomStudent)
                            .Where(e => param.IdAcademicYear.Contains(e.IdAcademicYear) && e.HomeroomStudent.IdStudent == param.IdStudent)
                            .ToListAsync(CancellationToken);

            if (!param.IsCASCoordinator)
            {
                GetExperience = GetExperience.Where(x => x.IdUserSupervisor == param.IdUser && (x.Status == ExperienceStatus.Approved || x.Status == ExperienceStatus.Completed)).ToList();
            }

            var StartPeriodDate = GetPeriod.Select(e => e.StartDate).Min();
            var EndPeriodDate = GetPeriod.Select(e => e.EndDate).Max();

            var startDate = new DateTime(StartPeriodDate.Year, StartPeriodDate.Month, 1);
            var EndDate = new DateTime(EndPeriodDate.Year, EndPeriodDate.Month, 1).AddMonths(1).AddDays(-1);
            List<GetHeader> Header = new List<GetHeader>();

            
            for (var day = StartPeriodDate; day.Date <= EndPeriodDate; day = day.AddMonths(1))
            {
                Header.Add(new GetHeader
                {
                    Year = day.Year,
                    Month = day.ToString("MMM"),
                    Date = new DateTime(day.Year, day.Month, 1)
                });
            }

            List<GetBody> Body = new List<GetBody>();
            foreach (var ItemExperience in GetExperience)
            {
                var StartExperienceDate = new DateTime(ItemExperience.StartDate.Year, ItemExperience.StartDate.Month, 1);
                var EndExperienceDate = new DateTime(ItemExperience.EndDate.Year, ItemExperience.EndDate.Month, 1).AddMonths(1).AddDays(-1); ;

                List<GetTimeline> Timeline = new List<GetTimeline>();

                //IDictionary<string, object> dataRespone = new Dictionary<string, object>();
                //dataRespone.Add("ExperienceName", ItemExperience.ExperienceName);

                foreach (var ItemHeader in Header)
                {
                    Timeline.Add(new GetTimeline
                    {
                        Year = ItemHeader.Year,
                        Month = ItemHeader.Month,
                        IsChecked = ItemHeader.Date.Date >= StartExperienceDate && ItemHeader.Date.Date <= EndExperienceDate ? true : false
                    });
                }

                //respone.Add(dataRespone);
                Body.Add(new GetBody
                {
                    NameExperience = ItemExperience.ExperienceName,
                    Timeline = Timeline
                });
            }

            var item = new GetTimelineBySupervisorResult
            {
                Headers = Header,
                Bodys = Body
            };

            return Request.CreateApiResult2(item as object);
        }
    }
}
