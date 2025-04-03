using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;

namespace BinusSchool.School.FnSchool.SurveySummary
{
    public class GetSurveySummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly ISurveySummary _serviceSurveySummay;
        private readonly IMachineDateTime _datetime;
        public GetSurveySummaryHandler(ISchoolDbContext dbContext, ISurveySummary serviceSurveySummay, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _serviceSurveySummay = serviceSurveySummay;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSurveySummaryRequest>();

            var listIdPublishSurveyLink = await _dbContext.Entity<TrPublishSurvey>()
                .Include(e => e.AcademicYear)
                .Where(e=>e.IdAcademicYear==param.IdAcademicYear && e.IdPublishSurveyLink!=null)
                .Select(x => x.IdPublishSurveyLink)
                .ToListAsync(CancellationToken);
            
            var predicate = PredicateBuilder.Create<TrPublishSurvey>(x => (x.Status == PublishSurveyStatus.Publish|| x.Status == PublishSurveyStatus.Unpublished)
                            && ((x.StartDate.Date <= _datetime.ServerTime.Date && x.EndDate.Date >= _datetime.ServerTime.Date)
                                || x.StartDate.Date <= _datetime.ServerTime.Date));

            string[] _columns = { "AcademicYear", "Semester", "SurveyName", "StartDate", "EndDate", "TotalRespondent" };

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester))
                predicate = predicate.And(x => x.Semester == int.Parse(param.Semester));

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Title.Contains(param.Search));

            if(listIdPublishSurveyLink.Any())
                predicate = predicate.And(x => !listIdPublishSurveyLink.Contains(x.Id));

            var queryPublishSurvey = _dbContext.Entity<TrPublishSurvey>()
                .Include(e => e.AcademicYear)
                .Where(predicate)
                .Select(x => new
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear.Description,
                    Semeter = x.Semester,
                    SurveyName = x.Title,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Respondent = x.Surveys.Where(e => e.Status == MySurveyStatus.Submitted).Select(e => e.IdUser).ToList(),
                    SurveyTemplateTypeEnum = x.SurveyTemplate.Type,
                    SurveyTemplateType = x.SurveyTemplate.Type.GetDescription(),
                    x.IdPublishSurveyLink
                });

            var listPublishSurvey = await queryPublishSurvey.ToListAsync(CancellationToken);

            List<GetSurveySummaryResult> listSurveySummary = new List<GetSurveySummaryResult>();
            List<string> Role = new List<string>();
            foreach (var item in listPublishSurvey)
            {
                GetSurveySummaryRespondentRequest _paramRespondent = new GetSurveySummaryRespondentRequest
                {
                    Id = item.Id,
                    Type = item.SurveyTemplateTypeEnum,
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = item.Semeter,
                    GetAll = true,
                    Return = CollectionType.Lov
                };

                var listRespondent = await _serviceSurveySummay.GetSurveySummaryRespondent(_paramRespondent);

                var respondent = listRespondent.Payload;

                GetSurveySummaryResult NewSurveySummary = new GetSurveySummaryResult
                {
                    Id = item.Id,
                    AcademicYear = item.AcademicYear,
                    Semester = item.Semeter,
                    SurveyName = item.SurveyName,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    TotalRespondent = respondent.Select(e=>e.TotalRespondent).Sum(),
                    SurveyTemplateTypeEnum = item.SurveyTemplateTypeEnum,
                    SurveyTemplateType = item.SurveyTemplateType,
                    Role = respondent.Select(e=>e.Role).Distinct().ToList()
                };

                listSurveySummary.Add(NewSurveySummary);
            }


            var query = listSurveySummary.Distinct();
            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;

                case "Semeter":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "SurveyName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SurveyName)
                        : query.OrderBy(x => x.SurveyName);
                    break;
                case "StartDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartDate)
                        : query.OrderBy(x => x.StartDate);
                    break;
                case "EndDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndDate)
                        : query.OrderBy(x => x.EndDate);
                    break;
                case "TotalRespondent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TotalRespondent)
                        : query.OrderBy(x => x.TotalRespondent);
                    break;

            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();


                items = result.Select(x => new GetSurveySummaryResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Semester = x.Semester,
                    SurveyName = x.SurveyName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TotalRespondent = x.TotalRespondent,
                    SurveyTemplateType = x.SurveyTemplateType,
                    SurveyTemplateTypeEnum = x.SurveyTemplateTypeEnum,
                    Role = x.Role
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetSurveySummaryResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Semester = x.Semester,
                    SurveyName = x.SurveyName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TotalRespondent = x.TotalRespondent,
                    SurveyTemplateType = x.SurveyTemplateType,
                    SurveyTemplateTypeEnum = x.SurveyTemplateTypeEnum,
                    Role = x.Role
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

    }

}
