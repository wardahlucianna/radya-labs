using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;

namespace BinusSchool.User.FnUser.Role
{
    public class GetRoleMobileTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetRoleMobileTypeHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetRoleMobileTypeRequest>(nameof(GetRoleMobileTypeRequest.IdFeature));

            var query = await _dbContext.Entity<MsFeature>()
                .Where(x => x.Id == param.IdFeature && x.IsShowMobile == true)
                .FirstOrDefaultAsync(CancellationToken);

            if (query is null)
                throw new NotFoundException("Feature not found");

            var types = query.Type.Split(";");

            var result = new List<GetRoleMobileTypeResult>();

            foreach (var type in types)
            {
                Enum.TryParse(type, out RolePermissionType permissionType);
                result.Add(new GetRoleMobileTypeResult
                {
                    Id = type,
                    Description = permissionType.GetDescription(),
                });
            }

            return Request.CreateApiResult2<object>(result);
        }
    }
}
