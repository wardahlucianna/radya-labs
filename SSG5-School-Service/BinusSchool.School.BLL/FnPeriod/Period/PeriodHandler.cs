using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnPeriod.Period.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnPeriod.Period
{
    public class PeriodHandler : FunctionsHttpCrudHandler
    {
        private const string _termFormat = "Term {0}";

        private readonly ISchoolDbContext _dbContext;
        private readonly IServiceProvider _provider;
        private readonly ICheckUsage _checkUsageService;

        public PeriodHandler(ISchoolDbContext schoolDbContext, IServiceProvider provider, ICheckUsage checkUsage)
        {
            _dbContext = schoolDbContext;
            _provider = provider;
            _checkUsageService = checkUsage;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                var checkResult = await _checkUsageService.CheckUsageTerm(data.Id);

                // don't set inactive when row have to-many relation
                if (checkResult.Payload)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsPeriod>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsGrade>()
                .Include(x => x.Level).ThenInclude(x => x.AcademicYear).ThenInclude(x => x.School)
                .Select(x => new GetPeriodDetailResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Acadyear = new CodeWithIdVm
                    {
                        Id = x.Level.AcademicYear.Id,
                        Code = x.Level.AcademicYear.Code,
                        Description = x.Level.AcademicYear.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = x.Level.Id,
                        Code = x.Level.Code,
                        Description = x.Level.Description
                    },
                    Terms = x.Periods.Select(y => new TermDetail
                    {
                        Id = y.Id,
                        Code = y.Code,
                        Description = y.Description,
                        StartDate = y.StartDate,
                        EndDate = y.EndDate,
                        AttendanceStartDate = y.AttendanceStartDate,
                        AttendanceEndDate = y.AttendanceEndDate,
                        Semester = y.Semester
                    }).OrderBy(x => x.StartDate).ToList(),
                    School = new GetSchoolResult
                    {
                        Id = x.Level.AcademicYear.School.Id,
                        Name = x.Level.AcademicYear.School.Name,
                        Description = x.Level.AcademicYear.School.Description
                    },
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetPeriodRequest>(nameof(GetPeriodRequest.IdSchool));
            var columns = new[] { "acadyear", "code", "description", "startDate", "endDate" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "level.academicYear.code" }
            };

            // use expression for advance filtering
            var filter = PredicateBuilder.Create<MsGrade>(x
                => param.IdSchool.Contains(x.Level.AcademicYear.IdSchool)
                && x.Periods.Count > 0);

            // apply search filter
            var searchDateTime = (isSearchDt: false, dt: default(DateTime));
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                var filter2 = PredicateBuilder.Create<MsGrade>(x
                    => EF.Functions.Like(x.Level.AcademicYear.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern()));

                // if searchBy start/end provided, add date expression
                if (DateTime.TryParse(param.Search, out var dt))
                {
                    searchDateTime = (true, dt);
                    filter2 = filter2.Or(x
                        => x.Periods.Any(y => EF.Functions.DateDiffDay(y.StartDate, dt) == 0)
                        || x.Periods.Any(y => EF.Functions.DateDiffDay(y.EndDate, dt) == 0));
                }

                filter = filter.And(filter2);
            }

            // filter by dropdown filter goes here
            if (param.IdAcadyear?.Any() ?? false)
                filter = filter.And(x => param.IdAcadyear.Contains(x.Level.IdAcademicYear));
            if (param.IdLevel?.Any() ?? false)
                filter = filter.And(x => param.IdLevel.Contains(x.Level.Id));
            if (param.IdGrade?.Any() ?? false)
                filter = filter.And(x => param.IdGrade.Contains(x.Id));

            var query = _dbContext.Entity<MsGrade>()
                .Include(x => x.Level).ThenInclude(x => x.AcademicYear)
                .Include(x => x.Periods)
                .Where(filter);

            query = param.OrderBy switch
            {
                "code" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Code.Length).ThenBy(x => x.Code)
                    : query.OrderByDescending(x => x.Code.Length).ThenByDescending(x => x.Code),
                "description" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Description.Length).ThenBy(x => x.Description)
                    : query.OrderByDescending(x => x.Description.Length).ThenByDescending(x => x.Description),
                _ => query.OrderByDynamic(param, aliasColumns)
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                var results = await query
                    // dont set pagination when user search datetime
                    .If(!searchDateTime.isSearchDt, x => x.SetPagination(param))
                    .Select(x => new GetPeriodResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Acadyear = x.Level.AcademicYear.Description,
                        StartDate = x.Periods.Min(y => y.StartDate),
                        EndDate = x.Periods.Max(y => y.EndDate),
                        AttendanceStartDate = x.Periods.Min(y => y.AttendanceStartDate),
                        AttendanceEndDate = x.Periods.Max(y => y.AttendanceEndDate)
                    })
                    .ToListAsync(CancellationToken);

                // do additional search datetime & pagination on client side
                if (searchDateTime.isSearchDt && results.Count != 0)
                    results = results
                        .Where(x
                            => EF.Functions.DateDiffDay(x.StartDate, searchDateTime.dt) == 0
                            || EF.Functions.DateDiffDay(x.EndDate, searchDateTime.dt) == 0)
                        .If(param.GetAll.HasValue && param.GetAll.Value, x => x.SetPagination(param))
                        .ToList();

                items = results;
            }
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.GetBody<AddPeriodRequest>();
            (await new AddPeriodValidator(_provider).ValidateAsync(body)).EnsureValid();
            body.IdGrades = body.IdGrades.Distinct();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var acadyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcadyear);
            if (acadyear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Acadyear"], "Id", body.IdAcadyear));

            var existGrades = await _dbContext.Entity<MsGrade>()
                .Include(x => x.Level)
                .Include(x => x.Periods)
                .Where(x => x.Level.IdAcademicYear == body.IdAcadyear && body.IdGrades.Contains(x.Id))
                .ToListAsync(CancellationToken);

            var notExistIdGrades = body.IdGrades.Except(body.IdGrades.Intersect(existGrades.Select(x => x.Id)));
            if (notExistIdGrades.Any())
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", string.Join(", ", notExistIdGrades)));

            var gradeAlreadyHasPeriods = existGrades.Where(x => x.Periods.Count != 0);
            if (gradeAlreadyHasPeriods.Any())
                throw new BadRequestException($"{string.Join(", ", gradeAlreadyHasPeriods.Select(x => x.Description))} already has a Period.");

            foreach (var idGrade in body.IdGrades)
            {
                var counter = 1;
                foreach (var term in body.Terms)
                {
                    _dbContext.Entity<MsPeriod>().Add(CreatePeriod(term, idGrade, counter));
                    counter++;
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdatePeriodRequest, UpdatePeriodValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var grade = await _dbContext.Entity<MsGrade>()
                .Include(x => x.Periods)
                .FirstOrDefaultAsync(x => x.Id == body.Id, CancellationToken);
            if (grade is null)
                throw new NotFoundException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", body.Id));

            var counter = 1;
            var updatedPeriods = new List<MsPeriod>();

            foreach (var term in body.Terms)
            {
                if (!string.IsNullOrEmpty(term.Id))
                {
                    // select existing period to update
                    var existPeriod = grade.Periods.FirstOrDefault(x => x.Id == term.Id);

                    // create new if not found
                    if (existPeriod is null)
                    {
                        _dbContext.Entity<MsPeriod>().Add(CreatePeriod(term, grade.Id, counter));
                    }
                    else
                    {
                        updatedPeriods.Add(existPeriod);

                        existPeriod.StartDate = term.StartDate;
                        existPeriod.EndDate = term.EndDate;
                        existPeriod.AttendanceStartDate = term.AttendanceStartDate;
                        existPeriod.AttendanceEndDate = term.AttendanceEndDate;
                        existPeriod.Semester = term.Semester.Value;
                        existPeriod.Code = string.Format(_termFormat, counter);
                        existPeriod.Description = string.Format(_termFormat, counter);

                        _dbContext.Entity<MsPeriod>().Update(existPeriod);
                    }
                }
                else
                {
                    _dbContext.Entity<MsPeriod>().Add(CreatePeriod(term, grade.Id, counter));
                }
                counter++;
            }

            // select unupdated period and remove it
            var unupdatedPeriods = grade.Periods
                .Where(x => x.IsActive) // this will prevent new period being evaluated
                .Except(updatedPeriods);
            if (unupdatedPeriods.Any())
            {
                foreach (var unupdated in unupdatedPeriods)
                {
                    unupdated.IsActive = false;
                    _dbContext.Entity<MsPeriod>().Update(unupdated);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private MsPeriod CreatePeriod(Term term, string idGrade, int counter)
        {
            return new MsPeriod
            {
                Id = Guid.NewGuid().ToString(),
                IdGrade = idGrade,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
                AttendanceStartDate = term.AttendanceStartDate,
                AttendanceEndDate = term.AttendanceEndDate,
                Semester = term.Semester.Value,
                Code = string.Format(_termFormat, counter),
                Description = string.Format(_termFormat, counter)
            };
        }
    }
}
