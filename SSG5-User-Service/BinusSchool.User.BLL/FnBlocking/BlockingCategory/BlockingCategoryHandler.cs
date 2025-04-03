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
using BinusSchool.Data.Model.User.FnBlocking.BlockingCategory;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Employee;
using BinusSchool.User.FnBlocking.BlockingCategory.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.User.FnBlocking.BlockingCategory
{
    public class BlockingCategoryHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IUserDbContext _dbContext;
        public BlockingCategoryHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetBlockingCategory = await _dbContext.Entity<MsBlockingCategory>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(CancellationToken);

            var GetBlockingCategoryType = await _dbContext.Entity<MsBlockingCategoryType>()
           .Where(x => ids.Contains(x.IdBlockingCategory))
           .ToListAsync(CancellationToken);

            var GetBlockingCategoryMessage = await _dbContext.Entity<MsBlockingMessage>()
            .Where(x => ids.Contains(x.IdCategory))
            .ToListAsync(CancellationToken);

            var GetBlockingCategoryAssignUser = await _dbContext.Entity<MsUserBlocking>()
           .Where(x => ids.Contains(x.IdBlockingCategory))
           .ToListAsync(CancellationToken);

            GetBlockingCategory.ForEach(x => x.IsActive = false);

            GetBlockingCategoryType.ForEach(x => x.IsActive = false);

            GetBlockingCategoryMessage.ForEach(x => x.IsActive = false);

            GetBlockingCategoryAssignUser.ForEach(x => x.IsActive = false);

            _dbContext.Entity<MsBlockingCategory>().UpdateRange(GetBlockingCategory);

            _dbContext.Entity<MsBlockingCategoryType>().UpdateRange(GetBlockingCategoryType);

            _dbContext.Entity<MsBlockingMessage>().UpdateRange(GetBlockingCategoryMessage);

            _dbContext.Entity<MsUserBlocking>().UpdateRange(GetBlockingCategoryAssignUser);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsBlockingCategory>()
            .Include(x=> x.BlockingCategoryTypes).ThenInclude(x=> x.BlockingType)
            .Include(x=> x.UserBlockings).ThenInclude(x=> x.User)
            .Where(x => x.Id == id)
            .Select(x => new GetBlockingCategoryDetailResult
            {
                Id = x.Id,
                BlockingCategory = x.Name,
                BlockingType = x.BlockingCategoryTypes.Where(e => e.IdBlockingCategory == id).Select(e => new ItemValueVm
                {
                    Id = e.IdBlockingType,                    
                    Description = e.BlockingType.Name,
                }).ToList(),
                AssignUser = (from ub in _dbContext.Entity<MsUserBlocking>() 
                              join usr in _dbContext.Entity<MsUser>() on ub.IdUser equals usr.Id 
                              join s in _dbContext.Entity<MsStaff>() on usr.Id equals s.IdBinusian
                              where ub.IdBlockingCategory == id
                              select new AssignUserList
                              {
                                  Id = usr.Id,
                                  DisplayName = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                  BinusianID = s.IdBinusian,
                                  Username = usr.Username
                              }).ToList()
            }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetBlockingCategoryRequest>();
            var columns = new[] { "BlockingCategory" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "BlockingCategory" },
            };

            var query = _dbContext.Entity<MsBlockingCategory>()
            .Include(e => e.BlockingCategoryTypes).ThenInclude(e => e.BlockingType)
            .Where(x => x.IdSchool == param.IdSchool)
            .AsQueryable();
            //.OrderByDynamic(param, aliasColumns);

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Name, param.SearchPattern()));
            }

            //ordering
            query = query.OrderBy(x => x.Name);

            switch (param.OrderBy)
            {
                case "BlockingCategory":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Name)
                        : query.OrderBy(x => x.Name);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                var data = result.Select(x => new GetBlockingCategoryQueryResult
                {
                    Id = x.Id,
                    BlockingCategory = x.Name,
                    BlockingType = x.BlockingCategoryTypes.Select(e => new GetBlockingCategoryType
                    {
                        Id = e.IdBlockingType,
                        Description = e.BlockingType.Name,
                        Category = e.BlockingType.Category,
                        Order = e.BlockingType.Order
                    }).ToList(),
                }).ToList();

                items = data.Select(x => new GetBlockingCategoryResult
                {
                    Id = x.Id,
                    BlockingCategory = x.BlockingCategory,
                    BlockingType = GetBlockingType(x.BlockingType),
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                var data = result.Select(x => new GetBlockingCategoryQueryResult
                {
                    Id = x.Id,
                    BlockingCategory = x.Name,
                    BlockingType = x.BlockingCategoryTypes.Select(e => new GetBlockingCategoryType
                    {
                        Id = e.IdBlockingType,
                        Description = e.BlockingType.Name,
                        Category = e.BlockingType.Category,
                        Order = e.BlockingType.Order
                    }).ToList(),
                }).ToList();

                items = data.Select(x => new GetBlockingCategoryResult
                {
                    Id = x.Id,
                    BlockingCategory = x.BlockingCategory,
                    BlockingType = GetBlockingType(x.BlockingType),
                }).ToList();
            }
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<AddBlockingCategoryRequest, AddBlockingCategoryValidator>();

            var existsData = _dbContext.Entity<MsBlockingCategory>()
                .Any(x => x.Name == body.BlockingCategory && x.IdSchool == body.IdSchool);

            if (existsData)
            {
                throw new BadRequestException($"Blocking Category { body.BlockingCategory} already exists.");
            }

            var idBlockingCategory = Guid.NewGuid().ToString();

            var newBlockingCategory = new MsBlockingCategory
            {
                Id = idBlockingCategory,
                IdSchool = body.IdSchool,
                Name = body.BlockingCategory,
            };

            if (body.IdsBlockingType != null)
            {
                foreach (var BlockingTypeid in body.IdsBlockingType)
                {
                    var newBlockingCategoryType = new MsBlockingCategoryType
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBlockingCategory = idBlockingCategory,
                        IdBlockingType = BlockingTypeid.Id
                    };

                    _dbContext.Entity<MsBlockingCategoryType>().Add(newBlockingCategoryType);
                }
            }

            if (body.IdsAssignUser != null)
            {
                foreach (var BlockingAssignUserid in body.IdsAssignUser)
                {
                    var newBlockingCategoryAssignUser = new MsUserBlocking
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBlockingCategory = idBlockingCategory,
                        IdUser = BlockingAssignUserid.Id
                    };

                    _dbContext.Entity<MsUserBlocking>().Add(newBlockingCategoryAssignUser);
                }
            }

            _dbContext.Entity<MsBlockingCategory>().Add(newBlockingCategory);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateBlockingCategoryRequest, UpdateBlockingCategoryValidator>();
            var GetBlockingCategory = await _dbContext.Entity<MsBlockingCategory>().Where(e => e.Id == body.IdBlockingCategory).SingleOrDefaultAsync(CancellationToken);
            var GetBlockingCategoryType = await _dbContext.Entity<MsBlockingCategoryType>().Where(e => e.IdBlockingCategory == body.IdBlockingCategory).ToListAsync(CancellationToken);
            var GetBlockingCategoryAssignUser = await _dbContext.Entity<MsUserBlocking>().Where(e => e.IdBlockingCategory == body.IdBlockingCategory).ToListAsync(CancellationToken);


            if (GetBlockingCategory is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Blocking Category"], "Id", body.IdBlockingCategory));
            }

            //update data in 
            if (GetBlockingCategory.Name != body.BlockingCategory)
            {
                var checkBlockingCategory = _dbContext.Entity<MsBlockingCategory>().Where(x => x.Name == body.BlockingCategory && x.IdSchool== GetBlockingCategory.IdSchool && x.Id!=body.IdBlockingCategory).FirstOrDefault();

                if (checkBlockingCategory != null)
                    throw new BadRequestException($"Blocking Category {body.BlockingCategory} already exists");

                GetBlockingCategory.Name = body.BlockingCategory;
            }

            _dbContext.Entity<MsBlockingCategory>().Update(GetBlockingCategory);

            //update data in 
            //remove Type
            foreach (var ItemType in GetBlockingCategoryType)
            {
                var ExsisBodyTypeId = body.IdsBlockingType.Any(e => e.Id == ItemType.IdBlockingType);

                if (!ExsisBodyTypeId)
                {
                    ItemType.IsActive = false;
                    _dbContext.Entity<MsBlockingCategoryType>().Update(ItemType);
                }
            }
            ////Add Type
            if (body.IdsBlockingType != null)
            {
                foreach (var Typeid in body.IdsBlockingType)
                {
                    var ExsistdbId = GetBlockingCategoryType.Where(e => e.IdBlockingType == Typeid.Id && e.IdBlockingCategory == GetBlockingCategory.Id).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newBlockingType = new MsBlockingCategoryType
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdBlockingCategory = GetBlockingCategory.Id,
                            IdBlockingType = Typeid.Id
                        };

                        _dbContext.Entity<MsBlockingCategoryType>().Add(newBlockingType);
                    }
                }
            }


            //update data in 
            //remove Assgin User
            foreach (var ItemAssignUser in GetBlockingCategoryAssignUser)
            {
                var ExsisBodyAssignUserId = body.IdsAssignUser.Any(e => e.Id == ItemAssignUser.IdUser);

                if (!ExsisBodyAssignUserId)
                {
                    ItemAssignUser.IsActive = false;
                    _dbContext.Entity<MsUserBlocking>().Update(ItemAssignUser);
                }
            }
            ////Add Assgin User
            if (body.IdsAssignUser != null)
            {
                foreach (var AssignUserid in body.IdsAssignUser)
                {
                    var ExsistdbId = GetBlockingCategoryAssignUser.Where(e => e.IdUser == AssignUserid.Id && e.IdBlockingCategory == GetBlockingCategory.Id).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newBlockingAssignUser = new MsUserBlocking
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdBlockingCategory = GetBlockingCategory.Id,
                            IdUser = AssignUserid.Id
                        };

                        _dbContext.Entity<MsUserBlocking>().Add(newBlockingAssignUser);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        public string GetBlockingType(List<GetBlockingCategoryType> BlockingTypeData)
        {
            var BlockingTypes = "";

            var BlockingTypeData1 = BlockingTypeData
                                .Where(x => x.Category != "FEATURE")
                                .OrderBy(x => x.Order)
                                .ToList();

            var BlockingTypeData2 = BlockingTypeData
                                .Where(x => x.Category == "FEATURE")
                                .OrderBy(x => x.Description)
                                .ToList();

            BlockingTypeData = BlockingTypeData1.Concat(BlockingTypeData2).Select(y => new GetBlockingCategoryType
            {
               Id = y.Id,
               Description = y.Description,
               Category = y.Category,
               Order = y.Order
            }).ToList();

            foreach (var data in BlockingTypeData)
            {
                if (!string.IsNullOrEmpty(data.Description))
                    BlockingTypes += BlockingTypes == "" ? data.Description : $", {data.Description}";
            }

            return BlockingTypes;
        }
    }
}
