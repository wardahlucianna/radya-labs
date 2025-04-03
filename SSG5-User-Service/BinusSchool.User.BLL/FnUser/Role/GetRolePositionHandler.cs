using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;

namespace BinusSchool.User.FnUser.Role
{
    public class GetRolePositionHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetRolePositionHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetRolePositionRequest>();

            var query = _dbContext.Entity<TrRolePosition>()
                                        .Include(x => x.TeacherPosition)
                                        .Where(x => x.Role.IdSchool==param.IdSchool);

            if (!string.IsNullOrEmpty(param.IdRole))
                query = query.Where(e => e.IdRole == param.IdRole);


            if (!string.IsNullOrEmpty(param.IdRoleGroup))
                query = query.Where(e => e.Role.IdRoleGroup == param.IdRoleGroup);

            var ListRolePosition = new List<GetRolePositionHandlerResult>();

            ListRolePosition = string.IsNullOrEmpty(param.IdRole)
                ? await query
                            .GroupBy(x => new
                            {
                                Id = x.Id,
                                Code = x.TeacherPosition.Code,
                                Description = x.TeacherPosition.Description,
                                IdPosition = x.TeacherPosition.IdPosition,
                                IdTeacherPosition = x.TeacherPosition.Id
                            })
                            .Select(x => new GetRolePositionHandlerResult
                            {
                                Code = x.Key.Code,
                                Description = x.Key.Description,
                                IdPosition = x.Key.IdPosition,
                                IdTeacherPosition = x.Key.IdTeacherPosition
                            })
                            .Distinct()
                            .OrderBy(e => e.Description)
                            .ToListAsync(CancellationToken)
                : await query
                            .GroupBy(x => new
                            {
                                Id = x.Id,
                                Code = x.TeacherPosition.Code,
                                Description = x.TeacherPosition.Description,
                                IdPosition = x.TeacherPosition.IdPosition,
                                IdTeacherPosition = x.TeacherPosition.Id
                            })
                            .Select(x => new GetRolePositionHandlerResult
                            {
                                Id = x.Key.Id,
                                Code = x.Key.Code,
                                Description = x.Key.Description,
                                IdPosition = x.Key.IdPosition,
                                IdTeacherPosition = x.Key.IdTeacherPosition
                            })
                            .OrderBy(e => e.Description)
                            .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(ListRolePosition as object);
        }
    }
}
