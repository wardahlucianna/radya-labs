using System.Collections.Generic;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using NPOI.SS.Formula.Functions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Common.Constants;
using FluentEmail.Core;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BinusSchool.Persistence.SchoolDb.Entities.User;

namespace BinusSchool.School.FnSchool.PublishSurvey
{
    public class GetSurveyMandatoryUserHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly ISurveySummary _serviceSurveySummay;
        private readonly IParent _serviceParent;

        public GetSurveyMandatoryUserHandler(ISchoolDbContext dbContext, IMachineDateTime datetime, ISurveySummary serviceSurveySummay, IParent serviceParent)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _serviceSurveySummay = serviceSurveySummay;
            _serviceParent = serviceParent;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSurveyMandatoryUserRequest>();
            //var dataChildrens = new List<ChildrenParentData>();

            var predicate = PredicateBuilder.Create<TrPublishSurvey>(x => x.IsActive && x.IsMandatory && x.Status == PublishSurveyStatus.Publish);

            predicate = predicate.And(x => x.StartDate <= _datetime.ServerTime.Date && x.EndDate >= _datetime.ServerTime.Date);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(x => x.Semester == param.Semester);

            var listIdPublishSurveyLink = await _dbContext.Entity<TrPublishSurvey>()
                               .Where(predicate)
                               .Select(x => x.IdPublishSurveyLink)
                               .ToListAsync(CancellationToken);

            if (listIdPublishSurveyLink.Any())
                predicate = predicate.And(e => !listIdPublishSurveyLink.Contains(e.Id));

            GetSurveySummaryUserRespondentRequest _paramRespondent = new GetSurveySummaryUserRespondentRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdSchool = param.IdSchool,
                IdUser = param.IdUser,
                Semester = param.Semester
            };

            var listApiRespondent = await _serviceSurveySummay.GetSurveySummaryUserRespondent(_paramRespondent);
            var listRespondentAll = listApiRespondent.Payload;

            if (listRespondentAll == null)
                return Request.CreateApiResult2();

            var getDataFilterUser = new List<GetSurveySummaryUserRespondentResult>();

            var listSurveyTemplateMapping = await _dbContext.Entity<TrPublishSurvey>()
                                .Include(e => e.AcademicYear)
                                .Include(e => e.SurveyTemplate)
                                .Include(e => e.PublishSurveyLink).ThenInclude(e => e.SurveyTemplate)
                                .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyGrades).ThenInclude(e => e.Level)
                                .Where(predicate)
                                .Select(e => new GetSurveyMandatoryUserResult
                                {
                                    Id = e.Id,
                                    SurveyName = e.Title,
                                    IsMandatory = e.IsMandatory,
                                    AcademicYear = e.AcademicYear.Description,
                                    Semester = e.Semester,
                                    StartDate = e.StartDate,
                                    EndDate = e.EndDate,
                                    StatusSurvey = _datetime.ServerTime.Date > e.EndDate
                                    ? SurveyStatus.Closed.GetDescription()
                                    : SurveyStatus.OnGoing.GetDescription(),
                                    IsEntryOneTime = e.IsEntryOneTime,
                                    LinkPublishSurvey = e.IdPublishSurveyLink,
                                    SubmissionOption = e.SubmissionOption,
                                    Language = e.SurveyTemplate.Language.ToString(),
                                    LanguageLinkPublishSurvey = (string.IsNullOrEmpty(e.IdPublishSurveyLink)) ? string.Empty : e.PublishSurveyLink.SurveyTemplate.Language.ToString(),
                                    Respondent = new List<ChildrenParentData>(),
                                    Levels = e.PublishSurveyRespondents.SelectMany(x => x.PublishSurveyGrades).Select(x => x.IdLevel).ToList()
                                })
                                .GroupBy(x => new { x.Id }) // Kelompokkan berdasarkan properti unik
                                .Select(g => g.First())     // Ambil elemen pertama dari setiap grup
                                .ToListAsync(CancellationToken);

            var listIdPublishSurvey = listSurveyTemplateMapping.Select(e => e.Id).Distinct().ToList();

            var listSurvey = await _dbContext.Entity<TrSurvey>()
                        .Include(e => e.HomeroomStudent)
                        .Where(e => listIdPublishSurvey.Contains(e.IdPublishSurvey))
                        .ToListAsync(CancellationToken);

            var listUser = await _dbContext.Entity<MsUser>()
                        .ToListAsync(CancellationToken);

            string currentIdStudent = string.Empty;
            string currentIdLevel = string.Empty;

            if (param.IdUser[0] == 'P')
            {
                currentIdStudent = param.IdUser[1..];
                currentIdLevel = listRespondentAll.FirstOrDefault(x => x.IdUserChild == currentIdStudent).Level.Id;
            }

            foreach (var item in listSurveyTemplateMapping)
            {
                var listRespondent = listRespondentAll.Where(e => e.IdPusblishSurvey == item.Id).ToList();

                if (!listRespondent.Any())
                    continue;

                var user = listRespondent
                            .Select(e => new
                            {
                                e.IdUser,
                                e.Role,
                                IdLevel = e.Level.Id,
                            })
                            .FirstOrDefault();

                var userAll = listRespondent
                                .GroupBy(e => new
                                {
                                    IdUser = e.IdUserChild,
                                    Role = "STUDENT",
                                    IdLevel = e.Level.Id,
                                })
                                .Select(e =>e.Key)
                                .ToList();

                userAll.Add(user);

                List<ChildrenParentData> dataChildrens = new List<ChildrenParentData>();

                if (user.Role != "PARENT")
                {
                    var exsisSurvey = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.IdUser == user.IdUser && e.Status == MySurveyStatus.Submitted).Any();

                    if (exsisSurvey)
                        continue;

                    var listSurveyPerIdUser = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.IdUser == user.IdUser).OrderByDescending(e => e.DateIn).FirstOrDefault();

                    item.Respondent.Add(new ChildrenParentData
                    {
                        Id = user.IdUser,
                        Name = listUser.Where(e => e.Id == user.IdUser).Select(e => e.DisplayName).FirstOrDefault(),
                        IdSurvey = listSurveyPerIdUser == null ? string.Empty : listSurveyPerIdUser.Id,
                        Status = listSurveyPerIdUser == null ? MySurveyStatus.None.GetDescription() : listSurveyPerIdUser.Status.GetDescription(),
                        Role = user.Role
                    });
                }
                else
                {
                    //if (!item.Levels.Any(x => x == currentIdLevel))
                    //    continue;

                    if (item.SubmissionOption == PublishSurveySubmissionOption.Submit1ReviewPerChildOr1ReviewPerFamily)
                    {
                        var statusSumbittedSurvey = false;
                        foreach (var itemUser in userAll)
                        {
                            var listSurveyPerIdUser = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.IdUser == itemUser.IdUser).OrderByDescending(e => e.DateIn).FirstOrDefault();
                            var exsisSurvey = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.IdUser == itemUser.IdUser && e.Status == MySurveyStatus.Submitted).Any();

                            if (exsisSurvey)
                                statusSumbittedSurvey = true;
                        }

                        if (!statusSumbittedSurvey)
                        {
                            foreach (var itemUser in userAll)
                            {
                                var listSurveyPerIdUser = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.IdUser == itemUser.IdUser).OrderByDescending(e => e.DateIn).FirstOrDefault();
                                var exsisSurvey = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.IdUser == itemUser.IdUser && e.Status == MySurveyStatus.Submitted).Any();

                                if (exsisSurvey)
                                    statusSumbittedSurvey = true;

                                item.Respondent.Add(new ChildrenParentData
                                {
                                    Id = itemUser.IdUser,
                                    Name = listUser.Where(e => e.Id == itemUser.IdUser).Select(e => e.DisplayName).FirstOrDefault(),
                                    IdSurvey = listSurveyPerIdUser == null ? string.Empty : listSurveyPerIdUser.Id,
                                    Status = listSurveyPerIdUser == null ? MySurveyStatus.None.GetDescription() : listSurveyPerIdUser.Status.GetDescription(),
                                    Role = itemUser.Role
                                });
                            }
                        }

                    }
                    else if (item.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerChild)
                    {
                        var userChild = userAll.Where(e => e.Role == "STUDENT").ToList();

                        foreach (var itemUser in userChild)
                        {
                            listSurvey = listSurvey.Where(e => e.HomeroomStudent != null).ToList();
                            var listSurveyPerIdUser = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.HomeroomStudent.IdStudent == itemUser.IdUser).OrderByDescending(e => e.DateIn).FirstOrDefault();
                            var exsisSurvey = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.HomeroomStudent.IdStudent == itemUser.IdUser && e.Status == MySurveyStatus.Submitted).Any();

                            if (exsisSurvey)
                                continue;

                            item.Respondent.Add(new ChildrenParentData
                            {
                                Id = itemUser.IdUser,
                                Name = listUser.Where(e => e.Id == itemUser.IdUser).Select(e => e.DisplayName).FirstOrDefault(),
                                IdSurvey = listSurveyPerIdUser == null ? string.Empty : listSurveyPerIdUser.Id,
                                Status = listSurveyPerIdUser == null ? MySurveyStatus.None.GetDescription() : listSurveyPerIdUser.Status.GetDescription(),
                                Role = itemUser.Role
                            });
                        }
                    }
                    else if (item.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerFamily)
                    {
                        var userParent = userAll.Where(e => e.Role == "PARENT").ToList();

                        foreach (var itemUser in userParent)
                        {
                            var listSurveyPerIdUser = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.IdUser == itemUser.IdUser).OrderByDescending(e => e.DateIn).FirstOrDefault();
                            var exsisSurvey = listSurvey.Where(e => e.IdPublishSurvey == item.Id && e.IdUser == itemUser.IdUser && e.Status == MySurveyStatus.Submitted).Any();

                            if (exsisSurvey)
                                continue;

                            item.Respondent.Add(new ChildrenParentData
                            {
                                Id = itemUser.IdUser,
                                Name = listUser.Where(e => e.Id == itemUser.IdUser).Select(e => e.DisplayName).FirstOrDefault(),
                                IdSurvey = listSurveyPerIdUser == null ? string.Empty : listSurveyPerIdUser.Id,
                                Status = listSurveyPerIdUser == null ? MySurveyStatus.None.GetDescription() : listSurveyPerIdUser.Status.GetDescription(),
                                Role = itemUser.Role
                            });
                        }
                    }
                }
            }

            listSurveyTemplateMapping = listSurveyTemplateMapping.Where(e => e.Respondent.Any()).ToList();

            return Request.CreateApiResult2(listSurveyTemplateMapping as object);
        }

        private class GetDataFamilyFromParent
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
