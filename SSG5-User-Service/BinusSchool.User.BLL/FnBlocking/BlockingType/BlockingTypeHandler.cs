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
using BinusSchool.Data.Model.User.FnBlocking.BlockingType;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnBlocking.BlockingType.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.User.FnBlocking.BlockingType
{
    public class BlockingTypeHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IUserDbContext _dbContext;
        public BlockingTypeHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetBlockingType = await _dbContext.Entity<MsBlockingType>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(CancellationToken);

            var GetBlockingTypeSubMenu = await _dbContext.Entity<MsBlockingTypeSubFeature>()
           .Where(x => ids.Contains(x.IdBlockingType))
           .ToListAsync(CancellationToken);

            GetBlockingType.ForEach(x => x.IsActive = false);

            GetBlockingTypeSubMenu.ForEach(x => x.IsActive = false);

            _dbContext.Entity<MsBlockingType>().UpdateRange(GetBlockingType);

            _dbContext.Entity<MsBlockingTypeSubFeature>().UpdateRange(GetBlockingTypeSubMenu);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            var query = await _dbContext.Entity<MsBlockingType>()
            .Include(p => p.BlockingTypeSubFeature)
            .Include(p => p.Feature)
            .Where(x => x.Id == id)
            .Select(x => new GetBlockingTypeDetailResult
            {
                Id = x.Id,
                BlockingType = x.Name,
                Menu = new MenuBlockingTypeDetail
                {
                    Id = x.IdFeature,
                    Code = x.Feature.Code,
                    Description = x.Feature.Description,
                    SubMenu = (from Submenu in _dbContext.Entity<MsFeature>()
                               where Submenu.IdParent == x.IdFeature
                               orderby Submenu.OrderNumber
                               select new SubMenuBlockingTypeDetail
                               {
                                   Id = Submenu.Id,
                                   Code = Submenu.Code,
                                   Description = Submenu.Description,
                                   IsChecked = x.BlockingTypeSubFeature.Any(y => y.IdSubFeature == Submenu.Id && y.IdBlockingType == id) ? true : false
                               }).ToList(),
                },

            }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetBlockingTypeRequest>();

            var columns = new[] { "BlockingType", "MenuBlocking", "Category" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "BlockingType" },
                { columns[1], "MenuBlocking" },
                { columns[2], "Category" }
            };

            var subQuery = await (from bt in _dbContext.Entity<MsBlockingType>()
                                  join bts_ in _dbContext.Entity<MsBlockingTypeSubFeature>() on bt.Id equals bts_.IdBlockingType into _bts
                                  from bts in _bts.DefaultIfEmpty()
                                  join ft_ in _dbContext.Entity<MsFeature>() on bts.IdSubFeature equals ft_.Id into _ft
                                  from ft in _ft.DefaultIfEmpty()
                                  where bt.IdSchool == param.IdSchool
                                  select new
                                  {
                                      IdBlockingType = bt.Id,
                                      IdSubFeature = ft.Id,
                                      SubFeature = ft.Description
                                  }).ToListAsync(CancellationToken);

            var query = (from bt in _dbContext.Entity<MsBlockingType>()
                         join f_ in _dbContext.Entity<MsFeature>() on bt.IdFeature equals f_.Id into _f
                         from f in _f.DefaultIfEmpty()
                         join bts_ in _dbContext.Entity<MsBlockingTypeSubFeature>() on bt.IdFeature equals bts_.IdSubFeature into _bts
                         from bts in _bts.DefaultIfEmpty()
                         join ft_ in _dbContext.Entity<MsFeature>() on bts.IdSubFeature equals ft_.Id into _ft
                         from ft in _ft.DefaultIfEmpty()
                         where bt.IdSchool == param.IdSchool
                         select new
                         {
                             Id = bt.Id,
                             BlockingType = bt.Name,
                             Menu = f.Description,
                             Category = bt.Category,
                             Order = bt.Order,
                             CanEdit = bt.Name.ToLower() == "website" 
                                          ? false
                                          : true,
                         }).AsQueryable();

            var dataBlockingCategoryType = await _dbContext.Entity<MsBlockingCategoryType>()
                            .Include(x => x.BlockingCategory)
                            .Where(x => x.BlockingCategory.IdSchool == param.IdSchool)
                            .Select(x => new
                            {
                                IdCategory = x.IdBlockingCategory,
                                IdCategoryType = x.IdBlockingType
                            }).ToListAsync(CancellationToken);


            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.BlockingType, param.SearchPattern()));
            }

            //ordering
            query = query.OrderBy(x => x.Order);

            switch (param.OrderBy)
            {
                case "BlockingType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BlockingType)
                        : query.OrderBy(x => x.BlockingType);
                    break;
                case "MenuBlocking":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Menu)
                        : query.OrderBy(x => x.Menu);
                    break;
                case "Category":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Category)
                        : query.OrderBy(x => x.Category);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetBlockingTypeResult
                {
                    Id = x.Id,
                    BlockingType = x.BlockingType,
                    Menu = string.IsNullOrEmpty(x.Menu) ? "-" : x.Menu,
                    SubMenu = GetSubMenu(subQuery.Where(y => y.IdBlockingType == x.Id).Select(y => new ItemValueVm
                    {
                        Id = y.IdSubFeature,
                        Description = y.SubFeature
                    }).ToList()),
                    Category = x.Category,
                    CanDelete = (x.Category == "FEATURE" && dataBlockingCategoryType.Any(y => y.IdCategoryType == x.Id) == false) ? true : false,
                    CanEdit = x.Category == "ATTENDANCE" || (x.Category == "FEATURE" ? x.CanEdit : false),
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetBlockingTypeResult
                {
                    Id = x.Id,
                    BlockingType = x.BlockingType,
                    Menu = string.IsNullOrEmpty(x.Menu) ? "-" : x.Menu,
                    SubMenu = GetSubMenu(subQuery.Where(y => y.IdBlockingType == x.Id).Select(y => new ItemValueVm
                    {
                        Id = y.IdSubFeature,
                        Description = y.SubFeature
                    }).ToList()),
                    Category = x.Category,
                    CanDelete = (x.Category == "FEATURE" && dataBlockingCategoryType.Any(y => y.IdCategoryType == x.Id) == false) ? true : false,
                    CanEdit = x.Category == "ATTENDANCE" || (x.Category == "FEATURE" ? x.CanEdit : false),
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

            var body = await Request.ValidateBody<AddBlockingTypeRequest, AddBlockingTypeValidator>();

            var existsData = _dbContext.Entity<MsBlockingType>()
                .Any(x => x.Name == body.BlockingType && x.IdSchool == body.IdSchool);

            if (existsData)
            {
                throw new BadRequestException($"Blocking Type { body.BlockingType} already exists.");
            }

            var maxOrderby = await _dbContext.Entity<MsBlockingType>()
                           .Select(x => x.Order)
                           .MaxAsync(CancellationToken);

            var idBlockingType = Guid.NewGuid().ToString();

            var newBlockingType = new MsBlockingType
            {
                Id = idBlockingType,
                IdSchool = body.IdSchool,
                Name = body.BlockingType,
                IdFeature = body.IdMenu,
                Category = "FEATURE",
                Order = maxOrderby + 1
            };



            if (body.IdSubMenu != null)
            {
                foreach (var SubMenuid in body.IdSubMenu)
                {
                    var newBlockingTypeSubMenu = new MsBlockingTypeSubFeature
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBlockingType = newBlockingType.Id,
                        IdSubFeature = SubMenuid.Id
                    };

                    _dbContext.Entity<MsBlockingTypeSubFeature>().Add(newBlockingTypeSubMenu);
                }
            }
            _dbContext.Entity<MsBlockingType>().Add(newBlockingType);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateBlockingTypeRequest, UpdateBlockingTypeValidator>();
            var GetBlockingType = await _dbContext.Entity<MsBlockingType>().Where(e => e.Id == body.IdBlockingType).SingleOrDefaultAsync(CancellationToken);
            var GetBlockingTypeSubMenu = await _dbContext.Entity<MsBlockingTypeSubFeature>().Where(e => e.IdBlockingType == body.IdBlockingType).ToListAsync(CancellationToken);


            if (GetBlockingType is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Blocking Type"], "Id", body.IdBlockingType));
            }

            //update data in 
            if (GetBlockingType.Name != body.BlockingType)
            {
                var checkBlockingType = _dbContext.Entity<MsBlockingType>().Where(x => x.Name == body.BlockingType).FirstOrDefault();

                if (checkBlockingType != null)
                {
                    throw new BadRequestException($"Blocking Type {body.BlockingType} already exists");
                }

                GetBlockingType.Name = body.BlockingType;
            }

            GetBlockingType.IdFeature = body.IdMenu;

            _dbContext.Entity<MsBlockingType>().Update(GetBlockingType);

            //update data in 
            //remove Sub Menu
            foreach (var ItemSubMenu in GetBlockingTypeSubMenu)
            {
                var ExsisBodySubMenuId = body.IdSubMenu.Any(e => e.Id == ItemSubMenu.IdSubFeature);

                if (!ExsisBodySubMenuId)
                {
                    ItemSubMenu.IsActive = false;
                    _dbContext.Entity<MsBlockingTypeSubFeature>().Update(ItemSubMenu);
                }
            }
            ////Add Sub Menu
            if (body.IdSubMenu != null)
            {
                foreach (var SubMenuid in body.IdSubMenu)
                {
                    var ExsistdbId = GetBlockingTypeSubMenu.Where(e => e.IdSubFeature == SubMenuid.Id && e.IdBlockingType == GetBlockingType.Id).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newBlockingType = new MsBlockingTypeSubFeature
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdBlockingType = GetBlockingType.Id,
                            IdSubFeature = SubMenuid.Id
                        };

                        _dbContext.Entity<MsBlockingTypeSubFeature>().Add(newBlockingType);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        public string GetSubMenu(List<ItemValueVm> SubMenuData)
        {
            var SubMenuDatas = "";

            foreach (var data in SubMenuData)
            {
                if (!string.IsNullOrEmpty(data.Description))
                    SubMenuDatas += SubMenuDatas == "" ? data.Description : $", {data.Description}";
            }

            if (string.IsNullOrEmpty(SubMenuDatas))
                SubMenuDatas = "-";

            return SubMenuDatas;
        }
    }
}
