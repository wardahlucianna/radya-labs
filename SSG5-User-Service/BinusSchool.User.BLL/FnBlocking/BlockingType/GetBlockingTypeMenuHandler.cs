using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.BlockingType;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnBlocking.BlockingType
{
    public class GetBlockingTypeMenuHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetBlockingTypeMenuHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBlockingTypeMenuRequest>();

            var query = (from Menu in _dbContext.Entity<MsFeature>()
                               orderby Menu.OrderNumber
                               where Menu.IdParent == null
                               select new GetBlockingTypeMenuResult
                               {
                                   Id = Menu.Id,
                                   Code = Menu.Code,
                                   Description = Menu.Description,
                                   SubMenu = (from Submenu in _dbContext.Entity<MsFeature>()
                                             where Submenu.IdParent == Menu.Id
                                             orderby Submenu.OrderNumber
                                             select new CodeWithIdVm
                                             {
                                                 Id = Submenu.Id,
                                                 Code = Submenu.Code,
                                                 Description = Submenu.Description
                                             }).ToList()
                               });

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Description, param.SearchPattern()));
            }

            var result = await query
                .ToListAsync(CancellationToken);


            return Request.CreateApiResult2(result as object);
        }
    }
}
