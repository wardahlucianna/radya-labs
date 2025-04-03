using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.BLPSettingPeriod
{
    public class GetGradeSurveyPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        public GetGradeSurveyPeriodHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeSurveyPeriodRequest>(nameof(GetGradeSurveyPeriodRequest.IdAcademicYear), nameof(GetGradeSurveyPeriodRequest.Semester), nameof(GetGradeSurveyPeriodRequest.IdSurveyCategory));

            var SurveyPeriodGrade = await _dbContext.Entity<MsSurveyPeriod>()
                                                   .Include(x => x.Grade).ThenInclude(y => y.Level).ThenInclude(y => y.AcademicYear)                                                   
                                                   .Where(a => a.Grade.Level.IdAcademicYear == param.IdAcademicYear &&
                                                                a.Semester == param.Semester &&
                                                                a.IdSurveyCategory == param.IdSurveyCategory)
                                                   .Select(a => new GetGradeSurveyPeriodResult
                                                   {
                                                       IdGrade = a.IdGrade,
                                                       GradeName = a.Grade.Description
                                                   })
                                                   .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(SurveyPeriodGrade as object);
        }
    }
}
