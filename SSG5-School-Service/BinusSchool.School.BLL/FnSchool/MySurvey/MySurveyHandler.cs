using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Common.Constants;
using FluentEmail.Core;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MySurvey.Validator;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.MySurvey;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;

namespace BinusSchool.School.FnSchool.MySurvey
{
    public class MySurveyHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly ISurveySummary _serviceSurveySummay;
        private readonly IParent _serviceParent;

        public MySurveyHandler(ISchoolDbContext dbContext, IMachineDateTime datetime, ISurveySummary serviceSurveySummay, IParent serviceParent)
        {
            _dbContext = dbContext;
            _datetime = datetime;
            _serviceSurveySummay = serviceSurveySummay;
            _serviceParent = serviceParent;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var getTemplateSurvey = await _dbContext.Entity<TrSurvey>()
              .Where(x => ids.Contains(x.Id))
              .ToListAsync(CancellationToken);

            getTemplateSurvey.ForEach(x => x.IsActive = false);
            _dbContext.Entity<TrSurvey>().UpdateRange(getTemplateSurvey);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMySurveyRequest>();
            IReadOnlyList<IItemValueVm> items;
            string[] _columns = { "AcademicYear", "Semester", "SurveyName", "StartDate", "EndDate", "Status" };

            GetSurveySummaryUserRespondentRequest _paramRespondent = new GetSurveySummaryUserRespondentRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdSchool = param.IdSchool.First(),
                IdUser = param.IdUser,
                Semester = param.Semester,
                IdUserParent = param.IdUserParent,
            };

            var listApiRespondent = await _serviceSurveySummay.GetSurveySummaryUserRespondent(_paramRespondent);
            var listRespondentAll = listApiRespondent.Payload==null
                                    ? new List<GetSurveySummaryUserRespondentResult>()
                                    : listApiRespondent.Payload;
            var getDataFilterUser = new List<GetSurveySummaryUserRespondentResult>();
            //var json = JsonSerializer.Serialize(listRespondentAll);
            if (listRespondentAll.Any())
            {
                if (param.IdUser.Substring(0, 1).ToUpper() == "P")
                {
                    var apiGetChild = _serviceParent.GetChildrens(new GetChildRequest
                    {
                        IdParent = param.IdUser,
                        IdAcademicYear = param.IdAcademicYear,
                    });

                    var listChild = apiGetChild.Result.IsSuccess ? apiGetChild.Result.Payload : null;

                    var listFamilyId = listChild.Where(e => e.Role == RoleConstant.Student).Select(e => e.Id).ToList();


                    getDataFilterUser = listRespondentAll.Where(x => listFamilyId.Any(t => t == x.IdUserChild)).ToList();
                }
                else
                    getDataFilterUser = listRespondentAll.Where(x => x.IdUser == param.IdUser && x.Role != "PARENT").ToList();
            }

            var predicate = PredicateBuilder.Create<TrPublishSurvey>(x => x.IsActive 
                                                                        && (x.Status==PublishSurveyStatus.Publish || x.Status == PublishSurveyStatus.Unpublished)
                                                                        && x.StartDate <= _datetime.ServerTime.Date);

            if (getDataFilterUser.Any())
            {
                var idPublishSurvey = getDataFilterUser.Select(x => x.IdPusblishSurvey).ToList(); 
                
                if (getDataFilterUser.Select(x => x.Role).FirstOrDefault() == "PARENT" && param.IdUser.Substring(0, 1).ToUpper() == "P")
                {
                    var getIdFromChild = _dbContext.Entity<TrPublishSurvey>()
                               .Where(x => idPublishSurvey.Any(y => y == x.Id) && x.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerChild)
                               .Select(x => x.Id)
                               .ToList();

                    if (getIdFromChild.Any())
                    {
                        var finalIdPublish = idPublishSurvey.Except(getIdFromChild).ToList();
                        predicate = predicate.And(x => finalIdPublish.Any(t => t == x.Id));
                    }
                    else
                    {
                        predicate = predicate.And(x => idPublishSurvey.Any(t => t == x.Id));
                    }
                }
                else
                {
                    if(idPublishSurvey.Any())
                        predicate = predicate.And(x => idPublishSurvey.Any(t => t == x.Id));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(param.IdUserParent))
                    return Request.CreateApiResult2(new List<GetMySurveyResult>() as IReadOnlyList<IItemValueVm>, param.CreatePaginationProperty(0).AddColumnProperty(_columns));
            }

            if (!string.IsNullOrEmpty(param.IdUserParent))
            {
                var _paramRespondentParent = new GetSurveySummaryUserRespondentRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    IdSchool = param.IdSchool.First(),
                    IdUser = param.IdUserParent,
                    Semester = param.Semester
                };

                var listApiRespondentFromParent = await _serviceSurveySummay.GetSurveySummaryUserRespondent(_paramRespondentParent);
                var listRespondentAllFromParent = listApiRespondentFromParent.Payload;
                var getDataFilterUserFromParent = listRespondentAllFromParent.Where(x => x.IdUserChild == param.IdUser).ToList();

                if (getDataFilterUserFromParent.Any())
                {
                    var idPublishSurveyFromParent = getDataFilterUserFromParent.Select(x => x.IdPusblishSurvey).ToList();

                    var queryFromParent = _dbContext.Entity<TrPublishSurvey>()
                            .Where(x => idPublishSurveyFromParent.Any(y => y == x.Id) && (x.SubmissionOption == PublishSurveySubmissionOption.SubmitReviewPerChild || x.SubmissionOption == PublishSurveySubmissionOption.Submit1ReviewPerChildOr1ReviewPerFamily))
                            .ToList();

                    if (queryFromParent.Any())
                    {
                        var idSurveyFromParent = queryFromParent.Select(x => x.Id).ToList();

                        if (idSurveyFromParent.Any())
                        {
                            if(getDataFilterUser.Any())
                                predicate = predicate.Or(x => idSurveyFromParent.Any(t => t == x.Id));
                            else
                                predicate = predicate.And(x => idSurveyFromParent.Any(t => t == x.Id));
                        }
                    }
                    else
                    {
                        if (!getDataFilterUser.Any())
                            return Request.CreateApiResult2(new List<GetMySurveyResult>() as IReadOnlyList<IItemValueVm>, param.CreatePaginationProperty(0).AddColumnProperty(_columns));
                    }
                }
                else
                {
                   if(!getDataFilterUser.Any())
                        return Request.CreateApiResult2(new List<GetMySurveyResult>() as IReadOnlyList<IItemValueVm>, param.CreatePaginationProperty(0).AddColumnProperty(_columns));
                }
            }

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (param.Semester != null)
                predicate = predicate.And(x => x.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.StartDate.ToString()) && !string.IsNullOrEmpty(param.EndDate.ToString()))
                predicate = predicate.And(x => x.StartDate.Date >= Convert.ToDateTime(param.StartDate).Date && x.EndDate.Date <= Convert.ToDateTime(param.EndDate).Date);

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Title.Contains(param.Search));


            var query = _dbContext.Entity<TrPublishSurvey>()
                .Include(x => x.AcademicYear)
                .Include(x => x.SurveyTemplate)
                .Where(predicate);

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;

                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
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
                default:
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Title)
                        : query.OrderBy(x => x.Title);
                    break;

            };

            var listIdPublishSurveyLink = await query
                                            .Where(e=>e.IdPublishSurveyLink!=null)
                                            .Select(e=>e.IdPublishSurveyLink)
                                            .ToListAsync(CancellationToken);

            if (listIdPublishSurveyLink.Any())
                query = query.Where(e => !listIdPublishSurveyLink.Contains(e.Id));

            var paramIdUser = new List<string>
            {
                param.IdUser
            };

            if(!string.IsNullOrEmpty(param.IdUserParent))
            {
                paramIdUser.Add("P" + param.IdUser);
            }

            if (param.Return == CollectionType.Lov)
            {
                items = await query
                     .Select(x => new ItemValueVm(x.Id, x.Title))
                     .ToListAsync(CancellationToken);
            }
            else
            {
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetMySurveyResult
                    {
                        Id = x.Id,
                        IdSurvey = _dbContext.Entity<TrSurvey>().Where(s => s.IdPublishSurvey == x.Id && paramIdUser.Any(t => t == s.IdUser)).Select(x => x.Id).FirstOrDefault(),
                        AcademicYear = x.AcademicYear.Description,
                        Semester = x.Semester,
                        SurveyName = x.Title,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Status = (string.IsNullOrEmpty(_dbContext.Entity<TrSurvey>().Where(s => s.IdPublishSurvey == x.Id && paramIdUser.Any(t => t == s.IdUser)).Select(x => x.Id).FirstOrDefault()))
                                    ? MySurveyStatus.None.GetDescription()
                                    : _dbContext.Entity<TrSurvey>().Where(s => s.IdPublishSurvey == x.Id && paramIdUser.Any(t => t == s.IdUser)).OrderByDescending(e=>e.DateIn).Select(x => x.Status.GetDescription()).FirstOrDefault(),
                        StatusSurvey = x.Status== PublishSurveyStatus.Unpublished
                                        ? SurveyStatus.Closed.GetDescription()
                                        : _datetime.ServerTime.Date > x.EndDate
                                            ? SurveyStatus.Closed.GetDescription()
                                            : SurveyStatus.OnGoing.GetDescription(),
                        IsEntryOneTime = x.IsEntryOneTime,
                        AfterSurveyCloseText = x.AfterSurveyCloseText,
                        SubmissionOption = x.SubmissionOption.ToString(),
                        LinkPublishSurvey = x.IdPublishSurveyLink,
                        Language = x.SurveyTemplate.Language.ToString(),
                        LanguageLinkPublishSurvey = (string.IsNullOrEmpty(x.IdPublishSurveyLink)) ? string.Empty : _dbContext.Entity<TrPublishSurvey>().Include(t => t.SurveyTemplate).Where(s => s.Id == x.IdPublishSurveyLink).Select(t => t.SurveyTemplate.Language.ToString()).FirstOrDefault(),
                    })
                    .ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMySurveyRequest, AddMySurveyValidator>();

            var getMySurvey = await _dbContext.Entity<TrSurvey>()
                                        .Where(e => e.Id == body.IdSurvey)
                                        .FirstOrDefaultAsync(CancellationToken);

            var results = new List<AddMySurveyResult>();

            if (getMySurvey == null)
            {
                if (body.IsAllInOne)
                {
                    var IdGeneratedAllInOne = Guid.NewGuid().ToString();
                    foreach (var itemUser in body.Users)
                    {
                        //Validate for Users Parent
                        if (string.IsNullOrEmpty(itemUser.IdHomeroomStudent))
                        {
                            var NewMySurvey = new TrSurvey
                            {
                                Id = body.IdSurvey,
                                IdSurveyChild = body.IdSurveyChild,
                                IdUser = itemUser.IdUser,
                                Status = body.Status,
                                IdPublishSurvey = body.IdPublishSurvey,
                                IdSurveyTemplateChild = body.IdSurveyTemplateChild,
                                IdHomeroomStudent = itemUser.IdHomeroomStudent,
                                IsAllInOne = body.IsAllInOne,
                                IdGeneratedAllInOne = IdGeneratedAllInOne,
                            };
                            _dbContext.Entity<TrSurvey>().Add(NewMySurvey);

                            results.Add(new AddMySurveyResult
                            {
                                IdUsers = itemUser.IdUser,
                                IdHomeroomStudent = itemUser.IdHomeroomStudent,
                                IdSurvey = body.IdSurvey,
                                IdSurveyChild = body.IdSurveyChild
                            });
                        }
                        else
                        {
                            //Proses Saving Data for Child of Parent
                            var NewMySurvey = new TrSurvey
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdSurveyChild = Guid.NewGuid().ToString(),
                                IdUser = itemUser.IdUser,
                                Status = body.Status,
                                IdPublishSurvey = body.IdPublishSurvey,
                                IdSurveyTemplateChild = body.IdSurveyTemplateChild,
                                IdHomeroomStudent = itemUser.IdHomeroomStudent,
                                IsAllInOne = body.IsAllInOne,
                                IdGeneratedAllInOne = IdGeneratedAllInOne,
                            };
                            _dbContext.Entity<TrSurvey>().Add(NewMySurvey);

                            results.Add(new AddMySurveyResult
                            {
                                IdUsers = itemUser.IdUser,
                                IdHomeroomStudent = itemUser.IdHomeroomStudent,
                                IdSurvey = NewMySurvey.Id,
                                IdSurveyChild = NewMySurvey.IdSurveyChild
                            });
                        }
                        
                    }
                }
                else
                {
                    var user = body.Users.FirstOrDefault();
                    var NewMySurvey = new TrSurvey
                    {
                        Id = body.IdSurvey,
                        IdSurveyChild = body.IdSurveyChild,
                        IdUser = user.IdUser,
                        Status = body.Status,
                        IdPublishSurvey = body.IdPublishSurvey,
                        IdSurveyTemplateChild = body.IdSurveyTemplateChild,
                        IdHomeroomStudent = user.IdHomeroomStudent,
                        IsAllInOne = body.IsAllInOne,
                    };
                    _dbContext.Entity<TrSurvey>().Add(NewMySurvey);

                    results.Add(new AddMySurveyResult
                    {
                        IdUsers = NewMySurvey.IdUser,
                        IdHomeroomStudent = NewMySurvey.IdHomeroomStudent,
                        IdSurvey = NewMySurvey.Id,
                        IdSurveyChild = NewMySurvey.IdSurveyChild
                    });
                }
            }
            else
            {
                if (body.IsAllInOne)
                {
                    var listIdUser = body.Users.Select(e => e.IdUser).ToList();
                    var listSurvey = await _dbContext.Entity<TrSurvey>()
                                        .Where(e => e.IdGeneratedAllInOne == getMySurvey.IdGeneratedAllInOne && listIdUser.Contains(e.IdUser))
                                        .ToListAsync(CancellationToken);

                    var IdGeneratedAllInOne = Guid.NewGuid().ToString();
                    foreach (var itemSurvey in listSurvey)
                    {
                        itemSurvey.IdSurveyChild = body.IdSurveyChild;
                        itemSurvey.IdUser = itemSurvey.IdUser;
                        itemSurvey.Status = body.Status;
                        itemSurvey.IdPublishSurvey = body.IdPublishSurvey;
                        itemSurvey.IdSurveyTemplateChild = body.IdSurveyTemplateChild;
                        itemSurvey.IdHomeroomStudent = itemSurvey.IdHomeroomStudent;
                        itemSurvey.IsAllInOne = body.IsAllInOne;
                        itemSurvey.IdGeneratedAllInOne = IdGeneratedAllInOne;
                        _dbContext.Entity<TrSurvey>().Update(itemSurvey);

                        results.Add(new AddMySurveyResult
                        {
                            IdUsers = itemSurvey.IdUser,
                            IdHomeroomStudent = itemSurvey.IdHomeroomStudent,
                            IdSurvey = body.IdSurvey,
                            IdSurveyChild = body.IdSurveyChild
                        });
                    }                    

                    var listUser = listSurvey.Select(e => new
                    {
                        e.IdUser,
                        e.IdHomeroomStudent
                    }).ToList();

                    foreach (var itemUser in body.Users)
                    {
                        var exsis = listSurvey
                                    .Where(e => e.IdUser == itemUser.IdUser && e.IdHomeroomStudent == itemUser.IdHomeroomStudent)
                                    .Any();

                        if (!exsis)
                        {
                            var NewMySurvey = new TrSurvey
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdSurveyChild = Guid.NewGuid().ToString(),
                                IdUser = itemUser.IdUser,
                                Status = body.Status,
                                IdPublishSurvey = body.IdPublishSurvey,
                                IdSurveyTemplateChild = body.IdSurveyTemplateChild,
                                IdHomeroomStudent = itemUser.IdHomeroomStudent,
                                IsAllInOne = body.IsAllInOne,
                                IdGeneratedAllInOne = IdGeneratedAllInOne,
                            };
                            _dbContext.Entity<TrSurvey>().Add(NewMySurvey);

                            results.Add(new AddMySurveyResult
                            {
                                IdUsers = itemUser.IdUser,
                                IdHomeroomStudent = itemUser.IdHomeroomStudent,
                                IdSurvey = NewMySurvey.Id,
                                IdSurveyChild = NewMySurvey.IdSurveyChild
                            });
                        }
                    }
                }
                else
                {
                    if (getMySurvey.IsAllInOne)
                    {
                        //melepas flaging
                        var listSurvey = await _dbContext.Entity<TrSurvey>()
                                        .Where(e => e.IdGeneratedAllInOne == getMySurvey.IdGeneratedAllInOne && e.Id != body.IdSurvey)
                                        .ToListAsync(CancellationToken);

                        listSurvey.ForEach(e =>
                        {
                            e.IsAllInOne = body.IsAllInOne;
                            e.IdGeneratedAllInOne = null;
                        });

                        _dbContext.Entity<TrSurvey>().UpdateRange(listSurvey);
                    }

                    var user = body.Users.FirstOrDefault();

                    getMySurvey.IdSurveyChild = body.IdSurveyChild;
                    getMySurvey.IdUser = user.IdUser;
                    getMySurvey.Status = body.Status;
                    getMySurvey.IdPublishSurvey = body.IdPublishSurvey;
                    getMySurvey.IdSurveyTemplateChild = body.IdSurveyTemplateChild;
                    getMySurvey.IdHomeroomStudent = user.IdHomeroomStudent;
                    getMySurvey.IsAllInOne = body.IsAllInOne;
                    getMySurvey.IdGeneratedAllInOne = null;

                    _dbContext.Entity<TrSurvey>().Update(getMySurvey);

                    results.Add(new AddMySurveyResult
                    {
                        IdUsers = getMySurvey.IdUser,
                        IdHomeroomStudent = getMySurvey.IdHomeroomStudent,
                        IdSurvey = getMySurvey.Id,
                        IdSurveyChild = getMySurvey.IdSurveyChild
                    });
                }
            }

            //Remove ALL Data survey From Other Familly 
            if(body.IsFilledWithOtherFamilies && body.IsAllInOne)
            {
                var dataIdSurveyFromOtherFamilies = body.FilledWithOtherFamilies.Select(x => x.IdSurvey).ToList();
                var exsisSurvery = _dbContext.Entity<TrSurvey>()
                                        .Where(e => dataIdSurveyFromOtherFamilies.Any(x => x == e.Id))
                                        .ToList();
                if (exsisSurvery.Any())
                {
                    exsisSurvery.ForEach(x => x.IsActive = false);
                    _dbContext.Entity<TrSurvey>().UpdateRange(exsisSurvery);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(results as object);
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateMySurveyRequest, UpdateMySurveyValidator>();

            //var getMySurvey = await _dbContext.Entity<TrSurvey>()
            //                            .Where(e => e.Id == body.Id)
            //                            .FirstOrDefaultAsync(CancellationToken);

            //if (getMySurvey == null)
            //    throw new BadRequestException("survey is not found");

            //if (body.IsAllInOne)
            //{

            //}
            //else
            //{
            //    getMySurvey.IdSurveyChild = body.IdSurveyChild;
            //    getMySurvey.IdUser = body.IdUsers.FirstOrDefault();
            //    getMySurvey.Status = body.Status;
            //    getMySurvey.IdPublishSurvey = body.IdTemplateSurveyPublish;
            //    getMySurvey.IdSurveyTemplateChild = body.IdSurveyTemplateChild;
            //    getMySurvey.IdHomeroomStudent = body.IdHomeroomStudent;

            //    _dbContext.Entity<TrSurvey>().Update(getMySurvey);
            //}


            //await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
