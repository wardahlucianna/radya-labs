using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.NotificationTemplate
{
    public class GetNotificationTemplateScenarioHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetNotificationTemplateScenarioRequest.IdSchool),
            nameof(GetNotificationTemplateScenarioRequest.IdScenario)
        });
        
        private readonly ISchoolDbContext _dbContext;

        public GetNotificationTemplateScenarioHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetNotificationTemplateScenarioRequest>(_requiredParams.Value);
            
            var result = await _dbContext.Entity<MsNotificationTemplate>()
                .Where(x => x.FeatureSchool.IdSchool == param.IdSchool && x.Scenario == param.IdScenario)
                .Select(x => new GetNotificationTemplateScenarioResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Title = x.Title,
                    Push = x.PushContent,
                    Email = x.EmailContent,
                    EmailIsHtml = x.EmailContentIsHtml,
                    Scenario = x.Scenario,
                    IdFeatureSchool = x.IdFeatureSchool,
                    FeatureCode = x.FeatureSchool.Feature.Code,
                    FeatureDescription = x.FeatureSchool.Feature.Description
                })
                .FirstOrDefaultAsync(CancellationToken);
            
            if (result is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["NotificationTemplate"], "Id", param.IdScenario));

            return Request.CreateApiResult2(result as object);
        }
    }
}
