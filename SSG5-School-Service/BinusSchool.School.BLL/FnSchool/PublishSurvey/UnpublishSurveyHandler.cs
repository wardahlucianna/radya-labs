using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;

namespace BinusSchool.School.FnSchool.PublishSurvey
{
    public class UnpublishSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public UnpublishSurveyHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<UnpublishSurveyRequest>();

            var listSurveyTemplateMapping = await _dbContext.Entity<TrPublishSurvey>()
                                .Where(e=>e.Id==param.Id)
                                .FirstOrDefaultAsync(CancellationToken);

            if(listSurveyTemplateMapping==null)
                throw new BadRequestException("Id publish survey is not found");

            //listSurveyTemplateMapping.IsPublish = param.IsPublish;
            listSurveyTemplateMapping.Status = param.IsPublish?PublishSurveyStatus.Publish: PublishSurveyStatus.Unpublished;
            _dbContext.Entity<TrPublishSurvey>().Update(listSurveyTemplateMapping);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
