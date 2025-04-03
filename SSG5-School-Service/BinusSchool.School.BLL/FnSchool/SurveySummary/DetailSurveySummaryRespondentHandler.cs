using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;

namespace BinusSchool.School.FnSchool.SurveySummary
{
    public class DetailSurveySummaryRespondentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly ISurveySummary _serviceSurveySummay;
        private readonly IMachineDateTime _datetime;
        public DetailSurveySummaryRespondentHandler(ISchoolDbContext dbContext, ISurveySummary serviceSurveySummay, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _serviceSurveySummay = serviceSurveySummay;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailSurveySummaryRespondentRequest>();

            var predicate = PredicateBuilder.Create<TrPublishSurvey>(x => (x.Status == PublishSurveyStatus.Publish || x.Status == PublishSurveyStatus.Unpublished)
                            && x.Id == param.IdPublishSurvey
                            && ((x.StartDate.Date <= _datetime.ServerTime.Date && x.EndDate.Date >= _datetime.ServerTime.Date)
                                || x.EndDate.Date <= _datetime.ServerTime.Date));

            var PublishSurvey = await _dbContext.Entity<TrPublishSurvey>()
                .Include(e => e.AcademicYear).ThenInclude(e => e.School)
                .Where(predicate)
                .Select(x => new
                {
                    IdAcademicYear = x.AcademicYear.Id,
                    IdSchool = x.AcademicYear.School.Id,
                    Id = x.Id,
                    Semester = x.Semester,
                    SubmissionOption = x.SubmissionOption,
                    Respondent = x.Surveys
                                .Where(e => e.Status == MySurveyStatus.Submitted)
                                .Select(e => new
                                {
                                    e.IdUser,
                                    e.IdSurveyChild,
                                    e.IdHomeroomStudent,
                                    date = e.UserIn == null ? e.DateIn : e.DateUp
                                }).OrderBy(e => e.IdUser).ThenBy(e => e.date).ToList()
                }).FirstOrDefaultAsync(CancellationToken);

            if (PublishSurvey == null)
                throw new BadRequestException("publish survey is not exsis");

            GetSurveySummaryUserRespondentRequest _paramRespondent = new GetSurveySummaryUserRespondentRequest
            {
                IdAcademicYear = PublishSurvey.IdAcademicYear,
                IdSchool = PublishSurvey.IdSchool,
                Semester = PublishSurvey.Semester,
                IdPublishSurvey = PublishSurvey.Id
            };

            var listApiRespondent = await _serviceSurveySummay.GetSurveySummaryUserRespondent(_paramRespondent);
            var listRespondentAll = listApiRespondent.IsSuccess ? listApiRespondent.Payload : null;
            var listRespondent = listRespondentAll.Where(e => e.Role != RoleConstant.Parent).ToList();


            if (listRespondentAll.Where(e => e.Role == RoleConstant.Parent).Any() && PublishSurvey.SubmissionOption != null)
            {
                var listParent = listRespondentAll.Where(e => e.Role == RoleConstant.Parent).ToList();

                var role = "";
                //if (PublishSurvey.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerFamily)
                //    role = RoleConstant.Parent;
                //if (PublishSurvey.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerChild)
                //    role = RoleConstant.Student;

                if (PublishSurvey.SubmissionOption != null)
                    role = RoleConstant.Parent;

                foreach (var item in listParent)
                {
                    GetSurveySummaryUserRespondentResult newChild = new GetSurveySummaryUserRespondentResult
                    {
                        IdUser = item.IdUser,
                        IdHomeroomStudent = item.IdHomeroomStudent,
                        Level = new ItemValueVmWithOrderNumber
                        {
                            Id = item.Level.Id,
                            Description = item.Level.Description,
                        },
                        Grade = new ItemValueVmWithOrderNumber
                        {
                            Id = item.Grade.Id,
                            Description = item.Grade.Description,
                        },
                        Homeroom = new SurveySummaryUserRespondentHomeroom
                        {
                            Id = item.Homeroom.Id,
                            Description = item.Homeroom.Description,
                            Semester = item.Homeroom.Semester
                        },
                        Role = role,
                        IdUserChild = item.IdUserChild,
                    };

                    listRespondent.Add(newChild);

                    if (PublishSurvey.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerFamily 
                        || PublishSurvey.SubmissionOption == PublishSurveySubmissionOption.Submit1ReviewPerChildOr1ReviewPerFamily )
                    {
                        GetSurveySummaryUserRespondentResult newParent = new GetSurveySummaryUserRespondentResult
                        {
                            IdUser = item.IdUser.Substring(1),
                            IdHomeroomStudent = item.IdHomeroomStudent,
                            Level = new ItemValueVmWithOrderNumber
                            {
                                Id = item.Level.Id,
                                Description = item.Level.Description,
                            },
                            Grade = new ItemValueVmWithOrderNumber
                            {
                                Id = item.Grade.Id,
                                Description = item.Grade.Description,
                            },
                            Homeroom = new SurveySummaryUserRespondentHomeroom
                            {
                                Id = item.Homeroom.Id,
                                Description = item.Homeroom.Description,
                                Semester = item.Homeroom.Semester
                            },
                            Role = role,
                            IdUserChild = item.IdUserChild,
                        };
                        listRespondent.Add(newParent);
                    }
                }
            }

            List<DetailSurveySummaryRespondentResult> listDetailSurveySummaryRespondent = new List<DetailSurveySummaryRespondentResult>();
            foreach (var item in listRespondent)
            {
                string IdSurveyChild = default;

                if(item.Role == RoleConstant.Parent)
                {
                    if (PublishSurvey.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerFamily)
                        IdSurveyChild = PublishSurvey.Respondent
                                            .Where(f => f.IdUser == item.IdUser)
                                            .Select(e => e.IdSurveyChild)
                                            .LastOrDefault();
                    if (PublishSurvey.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerChild)
                        IdSurveyChild = PublishSurvey.Respondent
                                            .Where(f => f.IdUser == item.IdUser && f.IdHomeroomStudent == item.IdHomeroomStudent)
                                            .Select(e => e.IdSurveyChild)
                                            .LastOrDefault();
                    if (PublishSurvey.SubmissionOption == PublishSurveySubmissionOption.Submit1ReviewPerChildOr1ReviewPerFamily)
                        IdSurveyChild = PublishSurvey.Respondent
                                            .Where(f => f.IdUser == item.IdUser)
                                            .Select(e => e.IdSurveyChild)
                                            .LastOrDefault();
                }
                else if (item.Role == RoleConstant.Student)
                    IdSurveyChild = PublishSurvey.Respondent
                                            .Where(f => f.IdUser == item.IdUser && f.IdHomeroomStudent == item.IdHomeroomStudent)
                                            .Select(e => e.IdSurveyChild)
                                            .LastOrDefault();
                else
                    IdSurveyChild = PublishSurvey.Respondent
                                            .Where(f => f.IdUser == item.IdUser)
                                            .Select(e => e.IdSurveyChild)
                                            .LastOrDefault();

                DetailSurveySummaryRespondentResult newDetail = new DetailSurveySummaryRespondentResult
                {
                    IdUser = item.IdUser,
                    IdHomeroomStudent = item.IdHomeroomStudent,
                    Level = item.Level,
                    Grade = item.Grade,
                    Homeroom = item.Homeroom,
                    IdPusblishSurvey = item.IdPusblishSurvey,
                    Role = item.Role,
                    IdSurveyChild = IdSurveyChild,
                    IdUserChild = item.IdUserChild,
                };

                listDetailSurveySummaryRespondent.Add(newDetail);
            }

            var listRespondentUniq = listDetailSurveySummaryRespondent
                                .Select(e => new DetailSurveySummaryRespondentResult
                                {
                                    IdUser = e.IdUser,
                                    IdHomeroomStudent = e.IdHomeroomStudent,
                                    Level = e.Level,
                                    Grade = e.Grade,
                                    Homeroom = e.Homeroom,
                                    IdPusblishSurvey = e.IdPusblishSurvey,
                                    Role = e.Role,
                                    IdSurveyChild = e.IdSurveyChild,
                                    IdUserChild = e.IdUserChild,
                                })
                                .ToList();

            return Request.CreateApiResult2(listRespondentUniq as object);
        }

    }
}
