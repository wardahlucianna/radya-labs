using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.Role
{
    public class GetStaffRoleHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetStaffRoleHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStaffRoleRequest>(nameof(GetStaffRoleRequest.IdSchool));

            var columns = new[] { "description", "code", "date" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "description" },
                { columns[1], "code" },
                { columns[2], "dateIn" }
            };

            var predicate = PredicateBuilder.Create<LtRole>(x => param.IdSchool.Contains(x.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                       || EF.Functions.Like(x.Code, param.SearchPattern()));

            if (param.HasPosition.HasValue)
                predicate = predicate.And(x => x.RolePositions.Any() == param.HasPosition.Value);

            var query = _dbContext.Entity<LtRole>()
                                  .Include(x => x.RoleGroup)
                                  .Include(x => x.RolePositions).ThenInclude(x => x.HierarchyMappingDetails)
                                  .Include(x => x.UserRoles)
                                  .Include(x => x.ApprovalStates)
                                  .Include(x => x.MessageApprovals)
                                  .Where(predicate)
                                  .Where(x =>   x.RoleGroup.Code.ToUpper() != "PARENT" && 
                                                x.RoleGroup.Code.ToUpper() != "STUDENT")
                                  .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetStaffRoleResult
                    {
                        Id = x.Id,
                        RoleGroup = new CodeWithIdVm
                        {
                            Id = x.RoleGroup.Id,
                            Code = x.RoleGroup.Code,
                            Description = x.RoleGroup.Description
                        },
                        Code = x.Code,
                        Description = x.Description,
                        CanDeleted = x.CanDeleted
                                     && !x.UserRoles.Any()
                                     && !x.ApprovalStates.Any()
                                     && !x.MessageApprovals.Any()
                                     && !x.RolePositions.Any(y => y.HierarchyMappingDetails.Any()),
                        CreatedDate = x.DateIn
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
