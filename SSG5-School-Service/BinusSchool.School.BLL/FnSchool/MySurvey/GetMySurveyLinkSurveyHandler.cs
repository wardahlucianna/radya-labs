using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Data.Model.School.FnSchool.MySurvey;

namespace BinusSchool.School.FnSchool.MySurvey
{
    public class GetMySurveyLinkSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _datetime;

        public GetMySurveyLinkSurveyHandler(ISchoolDbContext dbContext, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMySurveyLinkSurveyRequest>();

            var getSurvey = await _dbContext.Entity<TrSurvey>()
               .Include(x => x.PublishSurvey)
                    .ThenInclude(e => e.SurveyTemplate)
               .Include(x => x.PublishSurvey)
                    .ThenInclude(e => e.AcademicYear)
               .Where(e => e.IdPublishSurvey == param.IdPublishSurvey && 
               e.IdUser == param.IdUser && 
               e.PublishSurvey.IdAcademicYear == param.IdAcademicYear &&
               e.PublishSurvey.Semester == param.Semester)
               .Select(e => new
               {
                   e.Id,
                   e.IdUser,
                   e.Status,
                   e.IdPublishSurvey,
                   e.IdSurveyChild,
                   e.IdHomeroomStudent,
                   e.PublishSurvey.Title,
                   e.PublishSurvey.AboveSubmitText,
                   e.PublishSurvey.ThankYouMessage,
                   e.PublishSurvey.AfterSurveyCloseText,
                   e.PublishSurvey.AcademicYear.Description,
                   e.PublishSurvey.IdAcademicYear,
                   e.PublishSurvey.Semester,
                   e.PublishSurvey.StartDate,
                   e.PublishSurvey.EndDate,
                   SurveyTemplateType = e.PublishSurvey.SurveyTemplate.Type,
                   e.IsAllInOne,
                   e.IdGeneratedAllInOne,
                   e.PublishSurvey.IsEntryOneTime,
                   e.PublishSurvey.SubmissionOption,
                   e.PublishSurvey.IdPublishSurveyLink,
                   e.PublishSurvey.SurveyTemplate.Language
               })
               .FirstOrDefaultAsync(CancellationToken);

            var item = new GetMySurveyResult();

            if (getSurvey != null)
            {
                item = new GetMySurveyResult
                {
                    Id = getSurvey.IdPublishSurvey,
                    IdSurvey = getSurvey.Id,
                    AcademicYear = getSurvey.Description,
                    Semester = getSurvey.Semester,
                    SurveyName = getSurvey.Title,
                    StartDate = getSurvey.StartDate,
                    EndDate = getSurvey.EndDate,
                    Status = getSurvey.Status.GetDescription(),
                    StatusSurvey = _datetime.ServerTime.Date > getSurvey.StartDate
                                    ? SurveyStatus.Closed.GetDescription()
                                    : SurveyStatus.OnGoing.GetDescription(),
                    IsEntryOneTime = getSurvey.IsEntryOneTime,
                    AfterSurveyCloseText = getSurvey.AfterSurveyCloseText,
                    SubmissionOption = getSurvey.SubmissionOption.ToString(),
                    LinkPublishSurvey = getSurvey.IdPublishSurveyLink,
                    Language = getSurvey.Language.ToString(),
                    LanguageLinkPublishSurvey = (string.IsNullOrEmpty(getSurvey.IdPublishSurveyLink)) ? string.Empty : _dbContext.Entity<TrPublishSurvey>().Include(t => t.SurveyTemplate).Where(s => s.Id == getSurvey.IdPublishSurveyLink).Select(t => t.SurveyTemplate.Language.ToString()).FirstOrDefault(),
                };
            }

            return Request.CreateApiResult2(item as object);
        }
    }
}
