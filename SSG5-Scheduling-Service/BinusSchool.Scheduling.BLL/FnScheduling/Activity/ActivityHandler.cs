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
using BinusSchool.Data.Model.Scheduling.FnSchedule.Activity;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.Activity.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Activity
{
    public class ActivityHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ActivityHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected async override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsActivity>()
                .Include(x => x.School)
                .Include(x => x.EventActivities)
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
                if (data.EventActivities.Any(x => x.IsActive))
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsActivity>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected async override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsActivity>()
                .Include(x => x.School)
                .Include(x => x.EventActivities)
                .Select(x => new ActivityResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    School = new CodeWithIdVm
                    {
                        Id = x.IdSchool,
                        Code = x.School.Description,
                        Description = x.School.Description
                    },
                    IsUsed = x.EventActivities.Any(y => y.IsActive)
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(data as object);
        }

        protected async override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetActivityRequest>(nameof(GetActivityRequest.IdSchool));

            var columns = new[] { "description"};
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "description" }
            };

            var predicate = PredicateBuilder.Create<MsActivity>(x => true);
            if (!string.IsNullOrEmpty(param.IdSchool))
                predicate = predicate.And(x => x.IdSchool == param.IdSchool);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Code, param.SearchPattern()));

            var query = _dbContext.Entity<MsActivity>()
                .Include(x => x.School)
                .Include(x => x.EventActivities)
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
                    .Select(x => new ActivityResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        IsUsed = x.EventActivities.Any(y => y.IsActive)
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected async override Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddActivityRequest, AddActivityValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var isCodeExist = await _dbContext.Entity<MsActivity>()
                .Where(x => x.IdSchool == body.IdSchool && x.Description.ToLower() == body.Description.ToLower())
                .FirstOrDefaultAsync(CancellationToken);
            if (isCodeExist != null)
                throw new BadRequestException($"{body.Description} already exists in this school");

            var param = new MsActivity
            {
                Id = Guid.NewGuid().ToString(),
                Code = body.Code ?? body.Description,
                Description = body.Description,
                IdSchool = body.IdSchool
            };

            _dbContext.Entity<MsActivity>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected async override Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateActivityRequest, UpdateActivityValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsActivity>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EventType"], "Id", body.Id));

            var isCodeExist = await _dbContext.Entity<MsActivity>()
                .Where(x => x.Id != body.Id && x.IdSchool == body.IdSchool && x.Description.ToLower() == body.Description.ToLower())
                .FirstOrDefaultAsync(CancellationToken);
            if (isCodeExist != null)
                throw new BadRequestException($"{body.Description} already exists in this school");

            data.Code = body.Code;
            data.Description = body.Description;
            data.IdSchool = body.IdSchool;

            _dbContext.Entity<MsActivity>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
