﻿using System;
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
using BinusSchool.Data.Model.School.FnSchool.Division;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Divisions.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Divisions
{
    public class DivisionHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public DivisionHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsDivision>()
                .Include(x => x.ClassroomDivisions)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas.ToArray())
            {
                // don't set inactive when row have to-many relation
                if (data.ClassroomDivisions.Count != 0)
                {
                    datas.Remove(data);
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsDivision>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsDivision>()
                .Include(x => x.School)
                .Select(x => new DetailResult2
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    School = new GetSchoolResult
                    {
                        Id = x.School.Id,
                        Name = x.School.Name,
                        Description = x.School.Description
                    },
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionSchoolRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var columns = new[] { "code", "description" };

            var predicate = PredicateBuilder.Create<MsDivision>(x => param.IdSchool.Any(y => y == x.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, $"%{param.Search}%")
                    || EF.Functions.Like(x.Description, $"%{param.Search}%"));

            var query = _dbContext.Entity<MsDivision>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
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
            var body = await Request.ValidateBody<AddDivisionRequest, AddDivisionValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var school = await _dbContext.Entity<MsSchool>().FindAsync(body.IdSchool);
            if (school is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["School"], "Id", body.IdSchool));

            var isExist = await _dbContext.Entity<MsDivision>()
                .Where(x => x.IdSchool == body.IdSchool && (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower()))
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} already exists");

            var param = new MsDivision
            {
                Id = Guid.NewGuid().ToString(),
                Code = body.Code,
                Description = body.Description,
                IdSchool = body.IdSchool,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsDivision>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateDivisionRequest, UpdateDivisionValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsDivision>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Building"], "Id", body.Id));

            var school = await _dbContext.Entity<MsSchool>().FindAsync(body.IdSchool);
            if (school is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["School"], "Id", body.IdSchool));

            var isExist = await _dbContext.Entity<MsDivision>()
                .Where(x => x.Id != body.Id && x.IdSchool == body.IdSchool && (x.Code.ToLower() == body.Code.ToLower() || x.Description.ToLower() == body.Description.ToLower()))
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.Code} / {body.Description} already exists");
          
            data.Code = body.Code;
            data.Description = body.Description;
            data.IdSchool = body.IdSchool;
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsDivision>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
