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
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Document.FnDocument.Sourcedata;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.Sourcedata
{
    public class SourcedataHandler : FunctionsHttpCrudHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly ISchool _schoolService;

        public SourcedataHandler(IDocumentDbContext dbContext, ISchool schoolService)
        {
            _dbContext = dbContext;
            _schoolService = schoolService;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsSourceData>()
                .Select(x => new GetSourcedataDetailResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Endpoint = x.Endpoint,
                    School = new GetSchoolResult
                    {
                        Id = x.IdSchool
                    },
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            // invoke school service
            if (query != null)
            {
                var school = await _schoolService.GetSchoolDetail(query.School.Id);
                if (school.IsSuccess)
                {
                    query.School.Name = school.Payload.Name;
                    query.School.Address = school.Payload.Address;
                }
            }

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionSchoolRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var query = _dbContext.Entity<MsSourceData>()
                .Where(x => param.IdSchool.Any(y => y == x.IdSchool))
                .SearchByDynamic(param)
                .OrderByDynamic(param)
                .SetPagination(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Endpoint, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .Select(x => new GetSourcedataResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Endpoint = x.Endpoint
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);
            
            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddSourcedataRequest, AddSourcedataValidator>();
            
            // var existSchool = await _dbContext.Entity<Domain.Entities.SchoolMaster.School>().FindAsync(body.IdSchool);
            // if (existSchool is null)
            //     throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["School"], nameof(body.IdSchool), body.IdSchool));
            
            var existSourcedata = await _dbContext.Entity<MsSourceData>().FindAsync(body.IdSourcedata);
            if (existSourcedata is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Sourcedata"], nameof(body.IdSourcedata), body.IdSourcedata));

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            _dbContext.Entity<MsSourceData>().Add(new MsSourceData
            {
                Id = Guid.NewGuid().ToString(),
                IdSchool = body.IdSchool,
                Code = body.Code ?? existSourcedata.Code,
                Description = body.Description ?? existSourcedata.Description
            });

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateSourcedataRequest, UpdateSourcedataValidator>();

            var existSourcedata = await _dbContext.Entity<MsSourceData>().FindAsync(body.Id);
            if (existSourcedata is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Sourcedata"], nameof(body.Id), body.Id));

            // var existSchool = await _dbContext.Entity<Domain.Entities.SchoolMaster.School>().FindAsync(body.IdSchool);
            // if (existSchool is null)
            //     throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["School"], nameof(body.IdSchool), body.IdSchool));
            
            existSourcedata.IdSchool = body.IdSchool;
            existSourcedata.Code = body.Code ?? existSourcedata.Code;
            existSourcedata.Description = body.Description ?? existSourcedata.Description;

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            _dbContext.Entity<MsSourceData>().Update(existSourcedata);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}