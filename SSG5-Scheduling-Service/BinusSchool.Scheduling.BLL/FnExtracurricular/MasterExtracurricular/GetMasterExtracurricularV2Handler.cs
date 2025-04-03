using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator;
using BinusSchool.Scheduling.FnExtracurricular.PrivilegeUserElective;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetMasterExtracurricularV2Handler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "GroupName", "ExtracurricularName", "Grade", "ScheduleDay", "ScheduleDayTime", "Supervisor", "Status" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string> { };

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetPrivilegeUserElectiveHandler _getPrivilegeUserElectiveHandler;

        public GetMasterExtracurricularV2Handler(ISchedulingDbContext dbContext, IMachineDateTime dateTime, GetPrivilegeUserElectiveHandler getPrivilegeUserElectiveHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _getPrivilegeUserElectiveHandler = getPrivilegeUserElectiveHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetMasterExtracurricularV2Request, GetMasterExtracurricularV2Validator>();

            var getPrivilegeUserElective = new List<GetPrivilegeUserElectiveResult>();

            var getUser = await _dbContext.Entity<MsUser>()
                .Where(x => x.Id == param.IdUser)
                .FirstOrDefaultAsync(CancellationToken);

            if (!string.IsNullOrWhiteSpace(param.IdUser) && getUser != null)
            {
                var IdSchool = _dbContext.Entity<MsAcademicYear>().Where(x => x.Id == param.IdAcademicYear).Select(x => x.IdSchool).SingleOrDefault();

                getPrivilegeUserElective = await _getPrivilegeUserElectiveHandler.GetAvailabilityPositionUserElective(new GetPrivilegeUserElectiveRequest
                {
                    IdUser = param.IdUser,
                    IdSchool = IdSchool,
                    IdAcademicYear = param.IdAcademicYear
                });
            }

            var listElectiveAccess = getPrivilegeUserElective.Select(x => x.IdExtracurricular).ToList();

            var predicate = PredicateBuilder.True<MsExtracurricular>();

            if (param.IdAcademicYear?.Any() ?? false)
                predicate = predicate.And(x => x.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.Id).Contains(param.IdAcademicYear));
            if (param.Semester != null)
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (param.IdLevel != null)
                predicate = predicate.And(x => x.ExtracurricularGradeMappings.Any(y => param.IdLevel.Contains(y.Grade.Level.Id)));
            if (param.IdGrade != null)
                predicate = predicate.And(x => x.ExtracurricularGradeMappings.Any(y => param.IdGrade.Contains(y.Grade.Id)));
            if (param.Status != null)
                predicate = predicate.And(x => x.Status == param.Status);
            if (param.IdElectiveGroup != null)
                predicate = predicate.And(x => x.IdExtracurricularGroup == param.IdElectiveGroup);
            if (!string.IsNullOrWhiteSpace(param.IdUser) && getUser != null)
                predicate = predicate.And(x => listElectiveAccess.Any(z => z == x.Id));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.ExtracurricularGroup.Name, param.SearchPattern())
                    || x.ExtracurricularSessionMappings.Any(y => EF.Functions.Like(y.ExtracurricularSession.Day.Description, param.SearchPattern()))
                    || x.ExtracurricularSpvCoach.Any(y => EF.Functions.Like(y.Staff.FirstName, param.SearchPattern())));

            var query = _dbContext.Entity<MsExtracurricular>()
                .Include(x => x.ExtracurricularGroup)
                .Include(x => x.ExtracurricularParticipants)
                .Include(x => x.ExtracurricularSpvCoach)
                    .ThenInclude(x => x.Staff)
                .Include(X => X.ExtracurricularSessionMappings)
                    .ThenInclude(x => x.ExtracurricularSession).ThenInclude(x => x.Day)
                .Include(x => x.ExtracurricularGradeMappings)
                    .ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                .SearchByIds(param)
                .Where(predicate);

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                query = param.OrderBy switch
                {
                    "groupName" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.ExtracurricularGroup.Name)
                        : query.OrderByDescending(x => x.ExtracurricularGroup.Name),
                    "extracurricularName" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Name)
                        : query.OrderByDescending(x => x.Name),
                    "status" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Status)
                        : query.OrderByDescending(x => x.Status),
                    _ => query.OrderByDynamic(param, _aliasColumns)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Name);
            }

            var dataElectives = query.Select(x => new GetMasterExtracurricularResult_Data
            {
                IdElectives = x.Id,
                IsRegularSchedule = x.IsRegularSchedule,
                IdElectivesGroup = x.IdExtracurricularGroup,
                ElectivesGrade = x.ExtracurricularGradeMappings
                    .Select(x => new GetMasterExtracurricularResult_Grade
                    {
                        Id = x.IdGrade,
                        Description = x.Grade.Description,
                        OrderNumber = x.Grade.OrderNumber
                    })
                    .OrderBy(x => x.OrderNumber)
                    .ToList(),
                ElectivesSession = x.ExtracurricularSessionMappings.Select(x => new ItemValueVm { Id = x.ExtracurricularSession.IdDay, Description = x.ExtracurricularSession.Day.Description }).ToList()
            }).ToList();

            var dataElectivesSearch = new List<string>();

            if (param.ScheduleDay != null)
            {
                if (param.ScheduleDay == "-")
                {
                    dataElectivesSearch = dataElectives.Where(x => x.IsRegularSchedule == false).Select(x => x.IdElectives).ToList();
                    query = query.Where(x => dataElectivesSearch.Any(y => y == x.Id));
                }
                else
                {
                    dataElectivesSearch = dataElectives.Where(x => x.ElectivesSession.Any(y => y.Id == param.ScheduleDay) && x.IsRegularSchedule == true).Select(x => x.IdElectives).ToList();
                    query = query.Where(x => dataElectivesSearch.Any(y => y == x.Id));
                }
            }

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.Name + "-" + string.Join("; ", x.ExtracurricularGradeMappings.Select(a => a.Grade.Description))
                    })
                    .ToListAsync(CancellationToken);
                items = result;
            }
            else
            {
                var queryResults = await query
                    .Include(x => x.ExtracurricularGroup)
                    .Include(x => x.ExtracurricularSpvCoach)
                        .ThenInclude(x => x.Staff)
                    .Include(x => x.ExtracurricularSessionMappings)
                        .ThenInclude(x => x.ExtracurricularSession)
                        .ThenInclude(x => x.Day)
                    .Include(x => x.ExtracurricularGradeMappings)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                var extracurricularGradeMappingRawList = queryResults
                                                        .SelectMany(y => y.ExtracurricularGradeMappings)
                                                        .Select(y => y.IdGrade)
                                                        .ToList();

                var extracurricularRuleRawList = await _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                                            .Include(ergm => ergm.ExtracurricularRule)
                                            .Where(x => extracurricularGradeMappingRawList.Any(y => y == x.IdGrade) &&
                                                        x.ExtracurricularRule.Status == true
                                                    )
                                            .ToListAsync(CancellationToken);

                var results = new List<GetMasterExtracurricularResult>();

                foreach (var x in queryResults)
                {
                    var extracurricularGradeMapping = x.ExtracurricularGradeMappings.Select(y => y.IdGrade).ToList();

                    var extracurricularRule = extracurricularRuleRawList
                                                .Where(y => extracurricularGradeMapping.Any(z => z == y.IdGrade))
                                                .FirstOrDefault();

                    var result = new GetMasterExtracurricularResult
                    {
                        Id = x.Id,
                        GroupName = x.ExtracurricularGroup.Name,
                        ExtracurricularName = x.Name,
                        Grades = string.Join("; ", x.ExtracurricularGradeMappings.OrderBy(x => x.Grade.OrderNumber).ThenBy(x => x.Grade.Description).Select(a => a.Grade.Description)),
                        Semester = x.Semester,
                        ScheduleDay = x.ExtracurricularSessionMappings.Count != 0 // force order asc one-to-many relation
                            ? (x.IsRegularSchedule == true ?
                                    string.Join("; ", x.ExtracurricularSessionMappings
                                    .OrderBy(y => y.ExtracurricularSession.Day.Id).Select(y => y.ExtracurricularSession.Day.Description))
                                : "-")
                            : Localizer["-"],
                        ScheduleDayTime = x.ExtracurricularSessionMappings.Count != 0 // force order asc one-to-many relation
                            ? (x.IsRegularSchedule == true ?
                                    string.Join("; ", x.ExtracurricularSessionMappings
                                    .OrderBy(y => y.ExtracurricularSession.Day.Id).Select(y => y.ExtracurricularSession.Day.Description + " " + y.ExtracurricularSession.StartTime.ToString(@"hh\:mm") + "-" + y.ExtracurricularSession.EndTime.ToString(@"hh\:mm")))
                                : "-")
                            : Localizer["-"],
                        Supervisor = x.ExtracurricularSpvCoach.Count != 0 // force order asc one-to-many relation
                            ? string.Join(", ", x.ExtracurricularSpvCoach
                                .OrderBy(y => y.Staff?.FirstName)
                                .Where(y => y.IsSpv == true)
                                .Select(y => NameUtil.GenerateFullName(y.Staff?.FirstName, y.Staff?.LastName)))
                            : Localizer[""],
                        Status = x.Status,
                        Price = x.Price,
                        IsAnyParticipant = x.ExtracurricularParticipants.Count() > 0 ? true : false,
                        ReviewDate = extracurricularRule?.ExtracurricularRule.ReviewDate,
                        // Can change status only if the datetime now already passed 10 hours after review date
                        CanChangeStatus = extracurricularRule?.ExtracurricularRule.ReviewDate == null ? true : (
                        _dateTime.ServerTime >= extracurricularRule.ExtracurricularRule.ReviewDate && extracurricularRule.ExtracurricularRule.ReviewDate.Value.AddHours(10) < _dateTime.ServerTime)
                    };

                    results.Add(result);
                }

                items = results;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
