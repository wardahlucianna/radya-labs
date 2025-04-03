using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Document.FnDocument.Category;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.Category
{
    public class CategoryHandler : FunctionsHttpCrudHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly ISchool _schoolService;

        public CategoryHandler(IDocumentDbContext dbContext, ISchool schoolService)
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
            var query = await _dbContext.Entity<MsDocCategory>()
                .Include(x => x.DocType)
                .Where(p => p.Id == id)
                .Select(x => new GetCategoryDetailResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Type = new CodeWithIdVm
                    {
                        Id = x.DocType.Id,
                        Code = x.DocType.Code,
                        Description = x.DocType.Description
                    },
                    School = new GetSchoolResult
                    {
                        Id = x.Id
                    },
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(CancellationToken);

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
            var param = Request.ValidateParams<GetCategoryRequest>(nameof(GetCategoryRequest.IdSchool));
            var query = _dbContext.Entity<MsDocCategory>()
                .Include(x => x.DocType)
                .SearchByDynamic(param)
                .OrderByDynamic(param)
                .Where(x => param.IdSchool.Any(y => y == x.IdSchool)
                         && x.DocType.Code.Contains(param.DocumentType));

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, string.Format("{0} - {1}",x.DocType.Description , x.Description)))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetCategoryResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Type = new CodeWithIdVm
                        {
                            Id = x.DocType.Id,
                            Code = x.DocType.Code,
                            Description = x.DocType.Description
                        }
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            // var body = await request.ValidateBody<AddCategoryRequest, AddCategoryValidator>();
            
            // var existCategory = await _dbContext.Entity<DocumentCategory>().FindAsync(body.IdDocumentCategory);
            // if (existCategory is null)
            //     throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["Category"], nameof(body.IdDocumentCategory), body.IdDocumentCategory));
            
            // var existType = await _dbContext.Entity<SchoolDocumentType>().FindAsync(body.IdSchoolDocumentType);
            // if (existType is null)
            //     throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["Type"], nameof(body.IdSchoolDocumentType), body.IdSchoolDocumentType));
            
            // // TODO: also check current admin have access to idSchool

            // _transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
            // _dbContext.Entity<SchoolDocumentCategory>().Add(new SchoolDocumentCategory
            // {
            //     Id = Guid.NewGuid().ToString(),
            //     IdSchool = existType.IdSchool,
            //     IdDocumentCategory = existCategory.Id,
            //     IdSchoolDocumentType = existType.Id,
            //     Code = body.Code ?? existCategory.Code,
            //     Description = body.Description ?? existCategory.Description
            // });

            // await _dbContext.SaveChangesAsync(cancellationToken);
            // await _transaction.CommitAsync(cancellationToken);
            
            // return request.CreateApiResult();

            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            // var body = await request.ValidateBody<UpdateCategoryRequest, UpdateCategoryValidator>();

            // var existSchCategory = await _dbContext.Entity<SchoolDocumentCategory>().FindAsync(body.Id);
            // if (existSchCategory is null)
            //     throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["Category"], nameof(body.Id), body.Id));

            // var existCategory = await _dbContext.Entity<DocumentCategory>().FindAsync(body.IdDocumentCategory);
            // if (existCategory is null)
            //     throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["Category"], nameof(body.IdDocumentCategory), body.IdDocumentCategory));
            
            // var existType = await _dbContext.Entity<SchoolDocumentType>().FindAsync(body.IdSchoolDocumentType);
            // if (existType is null)
            //     throw new BadRequestException(string.Format(_localizer["ExNotExist"], _localizer["Type"], nameof(body.IdSchoolDocumentType), body.IdSchoolDocumentType));
            // if (existType.IdSchool != existSchCategory.IdSchool)
            //     throw new BadRequestException(string.Format(_localizer["ExUpdateDifferent"], nameof(existSchCategory.IdSchool)));

            // _transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

            // existSchCategory.IdDocumentCategory = body.IdDocumentCategory;
            // existSchCategory.IdSchoolDocumentType = body.IdSchoolDocumentType;
            // existSchCategory.Code = body.Code ?? existCategory.Code;
            // existSchCategory.Description = body.Description ?? existCategory.Description;

            // _dbContext.Entity<SchoolDocumentCategory>().Update(existSchCategory);

            // await _dbContext.SaveChangesAsync(cancellationToken);
            // await _transaction.CommitAsync(cancellationToken);
            
            // return request.CreateApiResult();
            
            throw new System.NotImplementedException();
        }
    }
}