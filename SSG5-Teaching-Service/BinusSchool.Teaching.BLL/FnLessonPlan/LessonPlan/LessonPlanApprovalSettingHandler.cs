using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanApprovalSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public LessonPlanApprovalSettingHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLessonPlanApprovalSettingRequest>(nameof(GetLessonPlanApprovalSettingRequest.IdAcademicYear));

            // var query = _dbContext.Entity<MsLevelApproval>()
            //     .Include(x => x.Level)
            //             .ThenInclude(x => x.AcademicYear)
            //     .AsQueryable();

            var query = _dbContext.Entity<MsLevel>()
                .Include(x => x.AcademicYear)
                .Include(x => x.LevelApprovals)
                .AsQueryable();
            
            if (param.IdAcademicYear != null)
                query = query.Where(x => x.AcademicYear.Id == param.IdAcademicYear);

            if (param.IdLevel != null)
                query = query.Where(x => x.Id == param.IdLevel);

            query = query.OrderByDescending(x => x.DateIn);

            switch(param.OrderBy)
            {
                case "academicYear":
                    query = param.OrderType == OrderType.Desc 
                        ? query.OrderByDescending(x => x.AcademicYear.Code) 
                        : query.OrderBy(x => x.AcademicYear.Code);
                    break;
                case "level":
                    query = param.OrderType == OrderType.Desc 
                        ? query.OrderByDescending(x => x.Code) 
                        : query.OrderBy(x => x.Code);
                    break;
                // case "isApprovalRequired":
                //     query = param.OrderType == OrderType.Desc 
                //         ? query.OrderByDescending(x => x.IsApproval) 
                //         : query.OrderBy(x => x.IsApproval);
                //     break;
            };

            var levelApproval = query
                .SetPagination(param)
                .Select(x => new GetLessonPlanApprovalSettingResult
                {
                    IdLevelApproval = x.LevelApprovals.Count() > 0 ? x.LevelApprovals.First().Id : x.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.IdAcademicYear,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description
                    },
                    IsApprovalRequired = x.LevelApprovals.Count() > 0 ? x.LevelApprovals.First().IsApproval : false 
                })
                .ToList();

            var count = param.CanCountWithoutFetchDb(levelApproval.Count) 
                ? levelApproval.Count 
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(levelApproval as object, param.CreatePaginationProperty(count));
        }
    }
}
