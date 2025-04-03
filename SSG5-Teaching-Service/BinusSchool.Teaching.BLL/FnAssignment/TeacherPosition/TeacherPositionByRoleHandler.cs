using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TeacherPosition
{
    public class TeacherPositionByRoleHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public TeacherPositionByRoleHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherPositionByRoleRequest>(nameof(GetTeacherPositionByRoleRequest.IdRole));

            var query = await _dbContext.Entity<TrRolePosition>()
                .Include(x => x.Role)
                .Include(x => x.TeacherPosition)
                .Where(x => x.IdRole == param.IdRole)
                .Select(x => new ItemValueVm
                {
                    Id = x.TeacherPosition.Id,
                    Description = x.TeacherPosition.Description
                }).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
