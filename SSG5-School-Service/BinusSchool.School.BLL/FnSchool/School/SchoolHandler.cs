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
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.School.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.School
{
    public class SchoolHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _schoolDbContext;

        public SchoolHandler(ISchoolDbContext schoolDbContext)
        {
            _schoolDbContext = schoolDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _schoolDbContext.Entity<MsSchool>()
                .Select(x => new GetSchoolDetailResult
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    LogoUrl = x.Logo,
                    Description = x.Description,
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.GetParams<CollectionRequest>();
            var query = _schoolDbContext.Entity<MsSchool>()
                .SearchByIds(param)
                .SearchByDynamic(param)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetSchoolResult
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Address = x.Address,
                        Description = x.Description
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);
            
            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddSchoolRequest, AddSchoolValidator>();
            Transaction = await _schoolDbContext.BeginTransactionAsync(CancellationToken);

            var school = new MsSchool
            {
                Id = Guid.NewGuid().ToString(),
                Name = body.Name,
                Address = body.Address,
                Description = body.Description,

            };

            _schoolDbContext.Entity<MsSchool>().Add(school);

            await _schoolDbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateSchoolRequest, UpdateSchoolValidator>();
            Transaction = await _schoolDbContext.BeginTransactionAsync(CancellationToken);

            var school = await _schoolDbContext.Entity<MsSchool>().FindAsync(body.Id);
            if (school is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["School"], "Id", body.Id));

            school.Name = body.Name;
            school.Address = body.Address;
            school.Description = body.Description;

            _schoolDbContext.Entity<MsSchool>().Update(school);

            await _schoolDbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
