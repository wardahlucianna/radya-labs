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
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessageV2;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnBlocking.BlockingMessageV2.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnBlocking.BlockingMessageV2
{
    public class BlockingMessageHandler : FunctionsHttpCrudHandler
    {
        private readonly IUserDbContext _dbContext;

        public BlockingMessageHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetBlockingMessage = await _dbContext.Entity<MsBlockingMessage>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(CancellationToken);

            GetBlockingMessage.ForEach(x => x.IsActive = false);

            _dbContext.Entity<MsBlockingMessage>().UpdateRange(GetBlockingMessage);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsBlockingMessage>()
                .Include(x => x.School)
                .Include(x => x.Category)
                .Select(x => new GetBlockingMessageResultV2
                {
                    Id = x.Id,
                    Content = x.Content,
                    School = new ItemValueVm
                    {
                        Id = x.School.Id,
                        Description = x.School.Name
                    },
                    Category = new ItemValueVm
                    {
                        Id = x.Category.Id,
                        Description = x.Category.Name
                    }
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetBlockingMessageRequestV2>();
            var columns = new[] { "Category" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Category" },
            };

            var query = _dbContext.Entity<MsBlockingMessage>()
            .Include(e => e.Category)
            .Where(x => x.IdSchool == param.IdSchool)
            .Where(x => x.IdCategory != null)
            .AsQueryable()
            .OrderByDynamic(param, aliasColumns);

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Category.Name, param.SearchPattern()));
            }

            query = query.OrderBy(x => x.Category.Name);

            switch (param.OrderBy)
            {
                case "Category":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Category.Name)
                        : query.OrderBy(x => x.Category.Name);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id))
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetBlockingMessageResultV2
                    {
                        Id = x.Id,
                        School = new ItemValueVm
                        {
                            Id = x.School.Id,
                            Description = x.School.Name
                        },
                        Category = new ItemValueVm
                        {
                            Id = x.Category.Id,
                            Description = x.Category.Name
                        }
                    })
                    .ToListAsync();
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddBlockingMessageRequestV2, AddBlockingMessageValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var isExist = await _dbContext.Entity<MsBlockingMessage>()
                .Where(x => x.IdSchool == body.IdSchool && x.IdCategory == body.IdCategory)
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"Category already exists");

            var param = new MsBlockingMessage
            {
                Id = Guid.NewGuid().ToString(),
                IdSchool = body.IdSchool,
                IdCategory = body.IdCategory,
                Content = body.Content,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsBlockingMessage>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateBlockingMessageRequestV2, UpdateBlockingMessageValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsBlockingMessage>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Blocking Message"], "Id", body.Id));

            var isExist = await _dbContext.Entity<MsBlockingMessage>()
                .Where(x => x.Id != body.Id && x.IdCategory == body.IdCategory)
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"Category already exists");

            data.IdCategory = body.IdCategory;
            data.Content = body.Content;
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsBlockingMessage>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
