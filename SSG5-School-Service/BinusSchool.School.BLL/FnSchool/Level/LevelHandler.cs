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
using BinusSchool.Data.Model.School.FnSchool.Level;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Level.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Level
{
    public class LevelHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public LevelHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsLevel>()
                .Include(x => x.Grades)
                .Include(x => x.DepartmentLevels)
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
                if (data.Grades.Count != 0 || data.DepartmentLevels.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsLevel>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsLevel>()
                .Include(x => x.AcademicYear)
                .ThenInclude(x => x.School)
                .Select(x => new GetLevelDetailResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Acadyear = new CodeWithIdVm
                    {
                        Id = x.AcademicYear.Id,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    School = new GetSchoolResult
                    {
                        Id = x.AcademicYear.School.Id,
                        Name = x.AcademicYear.School.Name,
                        Description = x.AcademicYear.School.Description
                    },
                    OrderNumber = x.OrderNumber,
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetLevelRequest>(nameof(GetLevelRequest.IdSchool));
            var columns = new[] { "acadyear", "description", "code", "ordernumber" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "academicYear.code" }
            };

            var predicate = PredicateBuilder.Create<MsLevel>(x => param.IdSchool.Any(y => y == x.AcademicYear.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.AcademicYear.Description, $"%{param.Search}%"));

            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Id == param.IdLevel);

            var query = _dbContext.Entity<MsLevel>()
                .SearchByIds(param)
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetLevelResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Acadyear = x.AcademicYear.Description,
                        OrderNumber = x.OrderNumber
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddLevelRequest, AddLevelValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var acadyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcadyear);
            if (acadyear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Academic Year"], "Id", body.IdAcadyear));

            var isExist = await _dbContext.Entity<MsLevel>()
                .Where(x => x.IdAcademicYear == body.IdAcadyear && (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower()
                || x.OrderNumber == body.OrderNumber))
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} / Order Number = {body.OrderNumber} already exists in this academic year");

            var param = new MsLevel
            {
                Id = Guid.NewGuid().ToString(),
                Code = body.Code,
                Description = body.Description,
                IdAcademicYear = body.IdAcadyear,
                OrderNumber = body.OrderNumber,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsLevel>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateLevelRequest, UpdateLevelValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsLevel>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Level"], "Id", body.Id));

            var acadyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcadyear);
            if (acadyear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Academic Year"], "Id", body.IdAcadyear));

            var isExist = await _dbContext.Entity<MsLevel>()
                .Where(x => x.Id != body.Id && x.IdAcademicYear == body.IdAcadyear && 
                (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower() || x.OrderNumber == body.OrderNumber))
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} / Order Number = {body.OrderNumber} already exists in this academic year");

            data.Code = body.Code;
            data.Description = body.Description;
            data.IdAcademicYear = body.IdAcadyear;
            data.OrderNumber = body.OrderNumber;
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsLevel>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
