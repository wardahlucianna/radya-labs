using System;
using System.Collections.Generic;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using System.Linq;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.SurveySummary
{
    public class GetSurveySummaryRespondentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly ISurveySummary _serviceSurveySummay;
        private readonly IMachineDateTime _datetime;
        public GetSurveySummaryRespondentHandler(ISchoolDbContext dbContext, ISurveySummary serviceSurveySummay, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _serviceSurveySummay = serviceSurveySummay;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSurveySummaryRespondentRequest>();
            string[] _columns = { "Role", "Level", "Grade", "Homeroom", "Total", "TotalRespondent", "TotalNotAnswer", "Percent" };

            var listApiRespondentSurvey = await _serviceSurveySummay.DetailSurveySummaryRespondent(new DetailSurveySummaryRespondentRequest
            {
                IdPublishSurvey = param.Id,
            });
            var listUserRespondentAll = listApiRespondentSurvey.Payload;

            var listRespondent = listUserRespondentAll.Where(e => e.IdSurveyChild!=null).ToList();

            List<GetSurveySummaryRespondentResult> listSurveySummaryRespondent = new List<GetSurveySummaryRespondentResult>();

            var listRole = listUserRespondentAll.Select(e => e.Role).Distinct().ToList();

            var listGrade = await _dbContext.Entity<MsGrade>()
                                .Where(e=>e.Level.IdAcademicYear==param.IdAcademicYear)
                                .ToListAsync(CancellationToken);

            if(!listRole.Contains("STAFF") && !listRole.Contains("TEACHER"))
            {
                var listHomeroom = listUserRespondentAll.Select(e => e.Homeroom.Id).Distinct().ToList();

                foreach (var item in listHomeroom)
                {
                    var listRespondentAllByHomeroom = listUserRespondentAll.Where(e => e.Homeroom.Id == item).FirstOrDefault();
                    double TotalRespondentAll = listUserRespondentAll.Where(e => e.Homeroom.Id == item).Select(e => e.IdUserChild).Distinct().Count();
                    double TotalRespodent = listRespondent.Where(e => e.Homeroom.Id == item).Select(e => e.IdUserChild).Distinct().Count();
                    double TotalNotAnswer = TotalRespondentAll - TotalRespodent;
                    double PercentDouble = TotalRespodent / TotalRespondentAll * 100;
                    double Percent = Math.Round(PercentDouble, 2);
                    var orderNumber = listGrade.Where(e=>e.Id== listRespondentAllByHomeroom.Grade.Id).Select(e=>e.OrderNumber).FirstOrDefault();

                    GetSurveySummaryRespondentResult newSurveySummaryRespondent = new GetSurveySummaryRespondentResult
                    {
                        OrderNumber = orderNumber,
                        Level = listRespondentAllByHomeroom.Level.Description,
                        Grade = listRespondentAllByHomeroom.Grade.Description,
                        Homeroom = listRespondentAllByHomeroom.Homeroom.Description,
                        Total = TotalRespondentAll,
                        TotalRespondent = TotalRespodent,
                        TotalNotAnswer = TotalNotAnswer,
                        Percent = Percent,
                        Role = "STUDENT"
                    };

                    listSurveySummaryRespondent.Add(newSurveySummaryRespondent);
                }
            }
            else 
            {
                foreach (var item in listRole)
                {
                    double TotalRespondentAll = listUserRespondentAll.Where(e => e.Role == item).Select(e => e.IdUser).Distinct().Count();
                    double TotalRespodent = listRespondent.Where(e => e.Role == item).Select(e => e.IdUser).Distinct().Count();
                    double TotalNotAnswer = TotalRespondentAll - TotalRespodent;
                    double PercentDouble = TotalRespodent / TotalRespondentAll * 100;
                    double Percent = Math.Round(PercentDouble, 2);


                    GetSurveySummaryRespondentResult newSurveySummaryRespondent = new GetSurveySummaryRespondentResult
                    {
                        Role = item,
                        Total = TotalRespondentAll,
                        TotalRespondent = TotalRespodent,
                        TotalNotAnswer = TotalNotAnswer,
                        Percent = Percent,
                        Description = item
                    };

                    listSurveySummaryRespondent.Add(newSurveySummaryRespondent);
                }
                
            }

            var query = listSurveySummaryRespondent.Distinct();

            //ordering
            switch (param.OrderBy)
            {
                case "Role":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Role)
                        : query.OrderBy(x => x.Role);
                    break;

                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level)
                        : query.OrderBy(x => x.Level);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade)
                        : query.OrderBy(x => x.Grade);
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
                case "Total":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Total)
                        : query.OrderBy(x => x.Total);
                    break;
                case "TotalRespondent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TotalRespondent)
                        : query.OrderBy(x => x.TotalRespondent);
                    break;

                case "TotalNotAnswer":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TotalNotAnswer)
                        : query.OrderBy(x => x.TotalNotAnswer);
                    break;

                case "Percent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Percent)
                        : query.OrderBy(x => x.Percent);
                    break;
                default:
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.OrderNumber)
                        : query.OrderBy(x => x.OrderNumber);
                    break;

            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();

                items = result.Select(x => new GetSurveySummaryRespondentResult
                {
                    Role = x.Role,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    Total = x.Total,
                    TotalRespondent = x.TotalRespondent,
                    TotalNotAnswer = x.TotalNotAnswer,
                    Percent = x.Percent
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetSurveySummaryRespondentResult
                {
                    Role = x.Role,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    Total = x.Total,
                    TotalRespondent = x.TotalRespondent,
                    TotalNotAnswer = x.TotalNotAnswer,
                    Percent = x.Percent
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

    }
}
