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
using BinusSchool.Data.Model.Scheduling.FnSchedule.SessionSet;
using BinusSchool.Data.Model.School.FnPeriod.SessionSet;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.SessionSet.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SessionSet
{
    public class SessionSetHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SessionSetHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsSessionSet>()
                .Include(x => x.Sessions)
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
                if (data.Sessions.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsSessionSet>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionSchoolRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var columns = new[] { "code" };

            var predicate = PredicateBuilder.Create<MsSessionSet>(x => param.IdSchool.Any(y => y == x.IdSchool));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, $"%{param.Search}%")
                    || EF.Functions.Like(x.Description, $"%{param.Search}%"));

            var query = _dbContext.Entity<MsSessionSet>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var model = await Request.ValidateBody<AddSessionSetRequest, AddSessionSetValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var copyInExisting = new MsSessionSet();

            if (!string.IsNullOrEmpty(model.IdSessionFrom))
            {
                copyInExisting = await _dbContext.Entity<MsSessionSet>()
                                           .Include(p => p.Sessions)
                                           .Where(p => p.Id == model.IdSessionFrom).FirstOrDefaultAsync();
                if (copyInExisting == null)
                {
                    throw new NotFoundException("Copy from Sessions Set Not found");
                }
            }

            var param = new MsSessionSet();
            param.Id = Guid.NewGuid().ToString();
            param.Code = model.Name;
            param.Description = model.Name;
            param.IdSchool = model.IdSchool;
            param.UserIn = AuthInfo.UserId;

            _dbContext.Entity<MsSessionSet>().Add(param);

            if (copyInExisting != null)
            {
                if (copyInExisting.Sessions != null)
                {
                    foreach (var item in copyInExisting.Sessions)
                    {
                        var dataSession = new MsSession();
                        dataSession.Id = Guid.NewGuid().ToString();
                        dataSession.IdSessionSet = param.Id;
                        dataSession.IdDay = item.IdDay;
                        dataSession.IdGradePathway = item.IdGradePathway;
                        dataSession.SessionID = item.SessionID;
                        dataSession.Name = item.Name;
                        dataSession.Alias = item.Alias;
                        dataSession.DurationInMinutes = item.DurationInMinutes;
                        dataSession.StartTime = item.StartTime;
                        dataSession.EndTime = item.EndTime;
                        dataSession.UserIn = AuthInfo.UserId;

                        _dbContext.Entity<MsSession>().Add(dataSession);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync();

            return Request.CreateApiResult2(param.Id as object);
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateSessionSetRequest, UpdateSessionSetValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsSessionSet>().FirstOrDefaultAsync(p => p.Id == body.Id);
            if (data == null)
            {
                throw new NotFoundException("Session Set Not Found");
            }

            data.Code = body.Name;
            data.Description = body.Name;

            _dbContext.Entity<MsSessionSet>().Update(data);
            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
