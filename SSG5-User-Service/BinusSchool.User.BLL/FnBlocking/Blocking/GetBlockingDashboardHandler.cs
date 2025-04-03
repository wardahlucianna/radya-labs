using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Student;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.User.FnBlocking.Blocking
{
    public class GetBlockingDashboardHandler : FunctionsHttpSingleHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly IUserDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetBlockingDashboardHandler(IUserDbContext userDbContext, [FromServices] IFeatureManagerSnapshot featureManager)
        {
            _dbContext = userDbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new GetBlockingDashboardResult();
            var idAttendanceSummary = "35";
            var idDisciplineSystem = "84";
            var idInformation = "97";
            var idSchedule = "28";

            var isFeatureActive = await _featureManager.IsEnabledAsync(FeatureFlags.StudentBlocking);
            if (isFeatureActive)
            {
                var param = Request.ValidateParams<GetBlockingDashboardRequest>();

                var Message = await _dbContext.Entity<MsBlockingMessage>()
                                .Include(e=> e.Category)
                                .Where(e => e.IdSchool == param.IdSchool)
                                .ToListAsync(CancellationToken);

                if (Message.Count > 0)
                {

                    var GetStudentBlocking = await (from StudentBlocking in _dbContext.Entity<MsStudentBlocking>()
                                                    join Student in _dbContext.Entity<MsStudent>() on StudentBlocking.IdStudent equals Student.Id
                                                    join BlockingType in _dbContext.Entity<MsBlockingType>() on StudentBlocking.IdBlockingType equals BlockingType.Id
                                                    join BlockingCategory in _dbContext.Entity<MsBlockingCategory>() on StudentBlocking.IdBlockingCategory equals BlockingCategory.Id
                                                    join BlockingMessage in _dbContext.Entity<MsBlockingMessage>() on BlockingCategory.Id equals BlockingMessage.IdCategory
                                                    join Feature in _dbContext.Entity<MsFeature>() on BlockingType.IdFeature equals Feature.Id into JoinedFeature
                                                    from Feature in JoinedFeature.DefaultIfEmpty()
                                                    join BlockingTypeSubFeature in _dbContext.Entity<MsBlockingTypeSubFeature>() on BlockingType.Id equals BlockingTypeSubFeature.IdBlockingType into JoinedTypeSubFeature
                                                    from BlockingTypeSubFeature in JoinedTypeSubFeature.DefaultIfEmpty()
                                                    join SubFeature in _dbContext.Entity<MsFeature>() on BlockingTypeSubFeature.IdSubFeature equals SubFeature.Id into JoinedSubFeature
                                                    from SubFeature in JoinedSubFeature.DefaultIfEmpty()
                                                    where StudentBlocking.IsBlocked == true && StudentBlocking.IdStudent == param.IdStudent
                                                    orderby BlockingMessage.DateIn
                                                    select new
                                                    {
                                                        BlockingTypeCategoryId = BlockingCategory.Id,
                                                        BlockingTypeCategory = BlockingType.Category,
                                                        BlockingCategoryName = BlockingCategory.Name,
                                                        IdFeature = BlockingType.IdFeature,
                                                        FeatureName = Feature.Description,
                                                        IdSubFeature = BlockingTypeSubFeature.IdSubFeature,
                                                        SubFeatureName = SubFeature.Description,
                                                    })
                                                    .ToListAsync(CancellationToken);

                    List<string> GetCategoryId = default;
                    var dataBlockingFeature = new List<string>();

                    if (GetStudentBlocking.Any(e => e.BlockingTypeCategory.ToUpper() == "WEBSITE"))
                    {
                        GetCategoryId = GetStudentBlocking
                                    .Where(e => e.BlockingTypeCategory.ToUpper() == "WEBSITE")
                                    .Select(e => e.BlockingTypeCategoryId)
                                    .ToList();

                        var GeneratedTitle = "";
                        if (GetCategoryId != null)
                        {
                            if (GetCategoryId.Count > 0)
                            {
                                foreach (var idCategoryBlocking in GetCategoryId)
                                {
                                    var Content = Message.Where(x => x.IdCategory == idCategoryBlocking).FirstOrDefault();
                                    if (Content != null)
                                    {
                                        var pushTitle = Handlebars.Compile(Content.Content);
                                        GeneratedTitle += pushTitle(Content.Category.Name);
                                    }
                                }
                            }
                        }

                        result = new GetBlockingDashboardResult
                        {
                            IsBlockingWebsite = true,
                            IsBlockingFeature = false,
                            BlockingMessageWebsite = GeneratedTitle,
                            ShowAttendanceSummary = false,
                            ShowDisciplineSystem = false,
                            ShowInformation = false,
                            ShowSchedule = false,
                            ShowWorkhabbit = false,
                        };
                    }
                    else
                    {
                        if (GetStudentBlocking.Any(e => e.BlockingTypeCategory.ToUpper() == "FEATURE"))
                        {
                            var GetStudentBlockingFeature = GetStudentBlocking.Where(x => x.BlockingTypeCategory == "FEATURE");
                            foreach (var item in GetStudentBlockingFeature.Select(x => new { BlockingCategoryName = x.BlockingCategoryName }).Distinct().ToList())
                            {
                                var dataFeature = GetStudentBlockingFeature.Where(x => x.IdSubFeature == null).Select(x => x.FeatureName).ToList();
                                var dataBLocking = string.Join(", ", dataFeature);
                                dataFeature = GetStudentBlockingFeature.Where(x => x.IdSubFeature != null).Select(x => x.SubFeatureName).ToList();
                                dataBLocking = dataBLocking + string.Join(", ", dataFeature);
                                dataBlockingFeature.Add($"This student are blocked by { item.BlockingCategoryName } and have no access to: { dataBLocking }");
                            }
                            result = new GetBlockingDashboardResult
                            {
                                IsBlockingWebsite = false,
                                IsBlockingFeature = true,
                                BlockingMessageFeature = dataBlockingFeature,
                                ShowAttendanceSummary = GetStudentBlockingFeature.Any(x => x.IdSubFeature == idAttendanceSummary) ? false : true,
                                ShowDisciplineSystem = GetStudentBlockingFeature.Any(x => x.IdFeature == idDisciplineSystem) ? false : true,
                                ShowInformation = GetStudentBlockingFeature.Any(x => x.IdFeature == idInformation) ? false : true,
                                ShowSchedule = GetStudentBlockingFeature.Any(x => x.IdFeature == idSchedule) ? false : true,
                                ShowWorkhabbit = GetStudentBlockingFeature.Any(x => x.IdSubFeature == idAttendanceSummary) ? false : true,
                            };
                        }
                        else
                        {
                            result = new GetBlockingDashboardResult
                            {
                                IsBlockingWebsite = false,
                                IsBlockingFeature = false,
                                ShowAttendanceSummary = true,
                                ShowDisciplineSystem = true,
                                ShowInformation = true,
                                ShowSchedule = true,
                                ShowWorkhabbit = true,
                            };
                        }
                    }
                }
                else
                {
                    result = new GetBlockingDashboardResult
                    {
                        IsBlockingWebsite = false,
                        IsBlockingFeature = false,
                        ShowAttendanceSummary = true,
                        ShowDisciplineSystem = true,
                        ShowInformation = true,
                        ShowSchedule = true,
                        ShowWorkhabbit = true,
                    };
                }
            }
            else
            {
                result = new GetBlockingDashboardResult
                {
                    IsBlockingWebsite = false,
                    IsBlockingFeature = false,
                    ShowAttendanceSummary = true,
                    ShowDisciplineSystem = true,
                    ShowInformation = true,
                    ShowSchedule = true,
                    ShowWorkhabbit = true,
                };
            }         
            return Request.CreateApiResult2(result as object);
        }
    }
}
