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
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Venue.Validator;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;

namespace BinusSchool.School.FnSchool.Venue
{
    public class VenueHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly ISchedule _iSchedule;

        public VenueHandler(ISchoolDbContext schoolDbContext, ISchedule iSchedule)
        {
            _dbContext = schoolDbContext;
            _iSchedule = iSchedule;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var param = new GetScheduleByVenueRequest
            {
                IdVenue = ids.First()
            };

            var dataScheduleExist = _iSchedule.CheckScheduleByVenue(param).Result.Payload;
            if (dataScheduleExist == true)
                throw new BadRequestException($"Unable to delete because its already in use");

            var datas = await _dbContext.Entity<MsVenue>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas.ToArray())
            {
                data.IsActive = false;
                _dbContext.Entity<MsVenue>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsVenue>()
                .Include(x => x.Building)
                .ThenInclude(x => x.School)
                .Select(x => new GetVenueDetailResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Capacity = x.Capacity,
                    Building = new CodeWithIdVm
                    {
                        Id = x.Building.Id,
                        Code = x.Building.Code,
                        Description = x.Building.Code
                    },
                    School = new GetSchoolResult
                    {
                        Id = x.Building.School.Id,
                        Name = x.Building.School.Name,
                        Description = x.Building.School.Description
                    },
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            var param = new GetScheduleByVenueRequest
            {
                IdVenue = data.Id
            };

            data.CanEditName =  !_iSchedule.CheckScheduleByVenue(param).Result.Payload;

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.GetParams<GetVenueRequest>();
            var columns = new[] { "building", "code", "description", "capacity" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "building.code" },
                { columns[1], "code" }
            };

            var predicate = PredicateBuilder.True<MsVenue>();
            if (param.IdSchool?.Any() ?? false)
                predicate = predicate.And(x => param.IdSchool.Contains(x.Building.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Building.Code, param.SearchPattern())
                    || EF.Functions.Like(Convert.ToString(x.Capacity), param.SearchPattern()));

            if (param.Ids != null && param.Ids.Any())
                predicate = predicate.And(x => param.Ids.Contains(x.Id));

            if (!string.IsNullOrEmpty(param.IdBuilding))
                predicate = predicate.And(x => x.IdBuilding == param.IdBuilding);

            var query = _dbContext.Entity<MsVenue>()
                .SearchByIds(param)
                .Where(predicate)
                .OrderBy(x => x.Description.Length).ThenBy(x => x.Description)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                   .Select(x => new GetVenueResult
                   {
                       Id = x.Id,
                       Code = x.Code,
                       Description = string.Format("{0} - {1}", x.Building.Code, x.Code),
                       Capacity = x.Capacity,
                       Building = x.Building.Code,
                       BuildingDesc = x.Building.Description,
                   })
                    .ToListAsync(CancellationToken);
            else
            {
                var datas = await query
                    .SetPagination(param)
                    .Select(x => new GetVenueResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Capacity = x.Capacity,
                        Building = x.Building.Code,
                        BuildingDesc = x.Building.Description,
                    })
                    .ToListAsync(CancellationToken);

                foreach (var item in datas)
                {
                    item.CanModified = _iSchedule.CheckScheduleByVenue(new GetScheduleByVenueRequest
                    {
                        IdVenue = item.Id
                    }).Result.Payload == false ? true : false;
                }

                items = datas;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddVenueRequest, AddVenueValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var building = await _dbContext.Entity<MsBuilding>().FindAsync(body.IdBuilding);
            if (building is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Building"], "Id", body.IdBuilding));

            var isExist = await _dbContext.Entity<MsVenue>()
                .Where(x => x.IdBuilding == body.IdBuilding && x.Code.ToLower() == body.Code.ToLower())
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} already exists in this building");

            var param = new MsVenue
            {
                Id = Guid.NewGuid().ToString(),
                Code = body.Code,
                Description = body.Description,
                IdBuilding = body.IdBuilding,
                Capacity = body.Capacity,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsVenue>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateVenueRequest, UpdateVenueValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsVenue>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Venue"], "Id", body.Id));

            var building = await _dbContext.Entity<MsBuilding>().FindAsync(body.IdBuilding);
            if (building is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Building"], "Id", body.IdBuilding));

            var isExist = await _dbContext.Entity<MsVenue>()
                .Where(x => x.Id != body.Id && x.IdBuilding == body.IdBuilding && x.Code.ToLower() == body.Code.ToLower())
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} already exists in this building");

            data.Code = body.Code;
            data.Description = body.Description;
            data.IdBuilding = body.IdBuilding;
            data.Capacity = body.Capacity;
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsVenue>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

    }
}
