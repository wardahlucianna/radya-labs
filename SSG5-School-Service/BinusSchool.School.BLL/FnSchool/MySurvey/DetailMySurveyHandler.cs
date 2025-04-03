using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using System.Threading;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using System.Threading.Tasks;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.MySurvey;

namespace BinusSchool.School.FnSchool.MySurvey
{
    public class DetailMySurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly ISurveySummary _serviceSurveySummay;
        private readonly IParent _serviceParent;

        public DetailMySurveyHandler(ISchoolDbContext dbContext, ISurveySummary serviceSurveySummay, IParent serviceParent)
        {
            _dbContext = dbContext;
            _serviceSurveySummay = serviceSurveySummay;
            _serviceParent = serviceParent;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMySurveyDetailRequest>();

            var getSurvey1 = await _dbContext.Entity<TrSurvey>()
               .Include(x => x.PublishSurvey).ThenInclude(e => e.SurveyTemplate)
               .Select(e => new
               {
                   e.Id,
               })
               .ToListAsync(CancellationToken);

            var getSurvey = await _dbContext.Entity<TrSurvey>()
               .Include(x => x.PublishSurvey).ThenInclude(e => e.SurveyTemplate)
               .Where(e => e.Id == param.IdSurvey)
               .Select(e => new
               {
                   e.Id,
                   e.IdUser,
                   e.Status,
                   e.IdPublishSurvey,
                   e.IdSurveyChild,
                   e.IdHomeroomStudent,
                   e.PublishSurvey.AboveSubmitText,
                   e.PublishSurvey.ThankYouMessage,
                   e.PublishSurvey.AfterSurveyCloseText,
                   e.PublishSurvey.IdAcademicYear,
                   e.PublishSurvey.Semester,
                   SurveyTemplateType = e.PublishSurvey.SurveyTemplate.Type,
                   e.IsAllInOne,
                   e.IdGeneratedAllInOne,
               })
               .FirstOrDefaultAsync(CancellationToken);

            GetMySurveyDetailResult item = new GetMySurveyDetailResult();

            if (getSurvey != null)
            {
                item = new GetMySurveyDetailResult
                {
                    Id = getSurvey.Id,
                    IdUser = getSurvey.IdUser,
                    Status = getSurvey.Status,
                    IdTemplateSurveyPublish = getSurvey.IdPublishSurvey,
                    IdSurveyChild = getSurvey.IdSurveyChild,
                    IdHomeroomStudent = getSurvey.IdHomeroomStudent,
                    AboveSubmitText = getSurvey.AboveSubmitText,
                    ThankYouMessage = getSurvey.ThankYouMessage,
                    AfterSurveyCloseText = getSurvey.AfterSurveyCloseText,
                    IsAllInOne = getSurvey.IsAllInOne,
                };

                if (getSurvey.IsAllInOne)
                {
                    var IdGeneratedAllInOne = getSurvey.IdGeneratedAllInOne;

                    var getSurveyAllInOne = await _dbContext.Entity<TrSurvey>()
                                                .Include(x => x.PublishSurvey)
                                                .Where(e => e.IdGeneratedAllInOne == IdGeneratedAllInOne)
                                                .Select(e => new DetailAllInOne
                                                {
                                                    IdUser = e.IdUser,
                                                    IdSurvey = e.Id,
                                                    IdSurveyChild = e.IdSurveyChild,
                                                    IdHomeroomStudent = e.IdHomeroomStudent
                                                })
                                                .ToListAsync(CancellationToken);

                    item.DetailAllInOnes = getSurveyAllInOne;
                }
            }

            #region Submit1ReviewPerChildOr1ReviewPerFamily
            var datPublishSurvey = _dbContext.Entity<TrPublishSurvey>()
                    .Where(x => x.Id == param.IdPublishSurvey &&
                                x.IdAcademicYear == param.IdAcademicYear &&
                                x.Semester == param.Semester).FirstOrDefault();

            if (datPublishSurvey != null)
            {
                if (datPublishSurvey.SubmissionOption == PublishSurveySubmissionOption.Submit1ReviewPerChildOr1ReviewPerFamily)
                {
                    var listFamily = new List<DataFamilies>();
                    var getAllFamilyFromUser = await _serviceParent.GetChildrens(new GetChildRequest
                    {
                        IdParent = param.IdUser,
                        IdAcademicYear = param.IdAcademicYear,
                    });

                    if (getAllFamilyFromUser.Payload != null && getAllFamilyFromUser.Payload.Count() > 0)
                    {
                        foreach (var family in getAllFamilyFromUser.Payload)
                        {
                            listFamily.Add(new DataFamilies
                            {
                                CamouflageIdUser = (family.Id.Substring(0, 1) != "P") ? "P"+ family.Id : family.Id,
                                IdUser = family.Id,
                                Name = family.Name,
                                Role = family.Role,
                            });
                        }
                    }

                    if (listFamily.Any())
                    {
                        var IdUserFamilys = listFamily.Select(x => x.CamouflageIdUser).ToList();
                        var getDataSurveyOtherFamily = _dbContext.Entity<TrSurvey>()
                            .Where(x => x.Status == MySurveyStatus.Submitted && x.IdPublishSurvey == param.IdPublishSurvey &&
                            IdUserFamilys.Any(t => t == x.IdUser)).ToList();
                        if (getDataSurveyOtherFamily.Any())
                        {
                            item.FilledWithOtherFamilies = new List<FilledWithOtherFamilies>();
                            
                            var DataOtherFamilys = (from a in getDataSurveyOtherFamily
                                                    select new FilledWithOtherFamilies
                                                    {
                                                        IdUser = a.IdUser,
                                                        IdHomeroomStudent = a.IdHomeroomStudent,
                                                        IdSurvey = a.Id,
                                                        IdSurveyChild = a.IdSurveyChild
                                                    }).ToList();

                            foreach (var dataFamily in DataOtherFamilys)
                            {
                                if(!string.IsNullOrEmpty(dataFamily.IdHomeroomStudent))
                                    dataFamily.IdUser = dataFamily.IdUser.Replace("P", "");

                                dataFamily.Name = _dbContext.Entity<MsUser>().Where(x => x.Id == dataFamily.IdUser).Select(x => x.DisplayName).FirstOrDefault();
                            }

                            item.FilledWithOtherFamilies.AddRange(DataOtherFamilys);
                        }
                    }
                }
            }

            #endregion


            return Request.CreateApiResult2(item as object);
        }
    }
}

public class DataFamilies
{
    public string CamouflageIdUser { get; set; }
    public string IdUser { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
}
