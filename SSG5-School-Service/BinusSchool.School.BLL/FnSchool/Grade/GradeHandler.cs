using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Grade.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GradeHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GradeHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsGrade>()
                .Include(x => x.GradePathways)
                .Include(x => x.Periods)
                .Include(x => x.Subjects)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                // don't set inactive when row have to-many relation
                if (data.GradePathways.Count != 0 || data.Periods.Count != 0 || data.Subjects.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsGrade>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsGrade>()
                .Include(x => x.Level)
                .ThenInclude(x => x.AcademicYear.School)
                .Select(x => new GetGradeDetailResult
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
                    School = new GetSchoolResult
                    {
                        Id = x.Level.AcademicYear.School.Id,
                        Name = x.Level.AcademicYear.School.Name,
                        Description = x.Level.AcademicYear.School.Description
                    },
                    OrderNumber = x.OrderNumber,
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetGradeRequest>(nameof(GetGradeRequest.IdSchool));
            var columns = new[] { "acadyear", "level", "description", "code", "ordernumber" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "level.academicYear.code" },
                { columns[1], "level.code" }
            };

            var predicate = PredicateBuilder.Create<MsGrade>(x => param.IdSchool.Any(y => y == x.Level.AcademicYear.School.Id));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Level.Description, $"%{param.Search}%")
                    || EF.Functions.Like(x.Level.AcademicYear.Description, $"%{param.Search}%"));

            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.Level.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);
            if (param.IsRemoveLastGrade && !string.IsNullOrEmpty(param.IdAcadyear))
            {
                var lastGrade = await _dbContext.Entity<MsGrade>()
                    .Include(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                            .ThenInclude(x => x.School)
                    .Where(x => param.IdSchool.Any(y => y == x.Level.AcademicYear.School.Id) &&
                        x.Level.IdAcademicYear == param.IdAcadyear)
                    .OrderByDescending(x => x.OrderNumber)
                    .FirstOrDefaultAsync(CancellationToken);

                if (lastGrade != null)
                    predicate = predicate.And(x => x.Id != lastGrade.Id);
            }

            var query = _dbContext.Entity<MsGrade>()
                .SearchByIds(param)
                .Where(predicate);

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
                items = await query
                    .Select(x => new CodeWithIdVm(x.Id, x.Code, x.Description))
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetGradeResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Acadyear = new CodeWithIdVm
                        {
                            Id = x.Level.IdAcademicYear,
                            Code = x.Level.AcademicYear.Code,
                            Description = x.Level.AcademicYear.Description
                        },
                        Level = new CodeWithIdVm
                        {
                            Id = x.IdLevel,
                            Code = x.Level.Code,
                            Description = x.Level.Description
                        },
                        School = new CodeWithIdVm
                        {
                            Id = x.Level.AcademicYear.IdSchool,
                            Code = x.Level.AcademicYear.School.Name,
                            Description = x.Level.AcademicYear.School.Description
                        },
                        OrderNumber = x.OrderNumber
                    })
                    .ToListAsync();
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddGradeRequest, AddGradeValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var level = await _dbContext.Entity<MsLevel>().FindAsync(body.IdLevel);
            if (level is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Level"], "Id", body.IdLevel));

            var isOrderNumberExist = await _dbContext.Entity<MsGrade>()
                .AnyAsync(x => x.Level.IdAcademicYear == level.IdAcademicYear && x.OrderNumber == body.OrderNumber, CancellationToken);

            if (isOrderNumberExist is true)
                throw new BadRequestException($"Order Number = {body.OrderNumber} already exist for this grade");

            var isExist = await _dbContext.Entity<MsGrade>()
                .Where(x => x.IdLevel == body.IdLevel && 
                (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower()))
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} / Order Number = {body.OrderNumber} already exists in this grade");

            var param = new MsGrade
            {
                Id = Guid.NewGuid().ToString(),
                Code = body.Code,
                Description = body.Description,
                IdLevel = body.IdLevel,
                OrderNumber = body.OrderNumber,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsGrade>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateGradeRequest, UpdateGradeValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsGrade>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", body.Id));

            var level = await _dbContext.Entity<MsLevel>().FindAsync(body.IdLevel);
            if (level is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Level"], "Id", body.IdLevel));

            var isOrderNumberExist = await _dbContext.Entity<MsGrade>()
                .AnyAsync(x => x.Id != body.Id && x.Level.IdAcademicYear == level.IdAcademicYear && x.OrderNumber == body.OrderNumber, CancellationToken);

            if (isOrderNumberExist is true)
                throw new BadRequestException($"Order Number = {body.OrderNumber} already exist for this grade");

            var isExist = await _dbContext.Entity<MsGrade>()
                .Where(x => x.Id != body.Id && x.IdLevel == body.IdLevel && 
                (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower() || x.OrderNumber == body.OrderNumber))
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} / Order Number = {body.OrderNumber} already exists in this grade");

            data.Code = body.Code;
            data.Description = body.Description;
            data.IdLevel = body.IdLevel;
            data.OrderNumber = body.OrderNumber;
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsGrade>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
