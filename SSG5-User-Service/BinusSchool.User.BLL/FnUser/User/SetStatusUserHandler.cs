using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.User.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.User
{
    public class SetStatusUserHandler : FunctionsHttpCrudHandler
    {
        private readonly IUserDbContext _dbContext;
        public SetStatusUserHandler(IUserDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<SetStatusUserRequest, SetStatusUserValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsUser>()
                .Where(x => body.Ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var unset = new UndeletedResult2();

            // find not found ids
            body.Ids = body.Ids.Except(body.Ids.Intersect(datas.Select(x => x.Id)));
            unset.NotFound = body.Ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                data.Status = body.IsActive;
                _dbContext.Entity<MsUser>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: unset.AsErrors());
        }
    }
}
