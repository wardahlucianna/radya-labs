using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
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
    public class GetBlockingHandler : FunctionsHttpSingleHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly IUserDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetBlockingHandler(IUserDbContext userDbContext, [FromServices] IFeatureManagerSnapshot featureManager)
        {
            _dbContext = userDbContext;
            _featureManager = featureManager;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            List<string> GetCategoryId = default;
            var GeneratedTitle = string.Empty;
            var isFeatureActive = await _featureManager.IsEnabledAsync(FeatureFlags.StudentBlocking);
            if (isFeatureActive)
            {
                var param = Request.ValidateParams<GetBlockingRequest>();

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
                                                    join BlockingTypeSubFeature in _dbContext.Entity<MsBlockingTypeSubFeature>() on BlockingType.Id equals BlockingTypeSubFeature.IdBlockingType into JoinedSubFeature
                                                    from BlockingTypeSubFeature in JoinedSubFeature.DefaultIfEmpty()
                                                    where StudentBlocking.IsBlocked == true && StudentBlocking.IdStudent == param.IdStudent
                                                    orderby BlockingMessage.DateIn
                                                    select new
                                                    {
                                                        BlockingTypeCategory = BlockingType.Category,
                                                        BlockingCategoryId = BlockingCategory.Id,
                                                        BlockingCategoryName = BlockingCategory.Name,
                                                        IdFeature = BlockingType.IdFeature,
                                                        IdSubFeature = BlockingTypeSubFeature.IdSubFeature,
                                                    })
                                                    .ToListAsync(CancellationToken);



                    if (GetStudentBlocking.Any(e => e.BlockingTypeCategory.ToUpper() == "WEBSITE"))
                    {
                        GetCategoryId = GetStudentBlocking
                                    .Where(e => e.BlockingTypeCategory.ToUpper() == "WEBSITE")
                                    .Select(e => e.BlockingCategoryId)
                                    .ToList();

                    }
                    else
                    {
                        if (GetStudentBlocking.Any(e => e.BlockingTypeCategory.ToUpper() == "FEATURE"))
                        {
                            if (string.IsNullOrEmpty(param.IdFeature))
                                throw new BadRequestException("Id feature can't null");

                            var GetFeature = await _dbContext.Entity<MsFeature>()
                                            .Where(e => e.Id == param.IdFeature)
                                            .SingleOrDefaultAsync(CancellationToken);

                            if (GetFeature == null)
                                throw new BadRequestException("Feature is not exsis");

                            GetCategoryId = GetFeature.IdParent == null
                                        ? GetStudentBlocking
                                        .Where(e => e.BlockingTypeCategory.ToUpper() == "FEATURE" && e.IdFeature == param.IdFeature)
                                        .Select(e => e.BlockingCategoryId)
                                        .ToList()
                                        : GetStudentBlocking
                                        .Where(e => e.BlockingTypeCategory.ToUpper() == "FEATURE" && e.IdSubFeature == param.IdFeature)
                                        .Select(e => e.BlockingCategoryId)
                                        .ToList();

                        }
                    }
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
                }                
            }       

            var result = new GetBlockingResult { Message = GeneratedTitle };
            return Request.CreateApiResult2(result as object);
        }
    }
}
