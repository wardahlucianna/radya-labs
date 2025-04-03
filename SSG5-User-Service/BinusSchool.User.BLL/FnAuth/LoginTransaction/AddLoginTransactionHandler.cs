using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.LoginTransaction;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnAuth.LoginTransaction.Validator;

namespace BinusSchool.User.FnAuth.LoginTransaction
{
    public class AddLoginTransactionHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public AddLoginTransactionHandler(
            IUserDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<AddLoginTransactionRequest, AddLoginTransactionValidator>();

            var newLoginTransaction = new TrLoginTransactionLog
            {
                Id = Guid.NewGuid().ToString(),
                IdUser = param.IdUser,
                IpAddress = param.IpAddress,
                LoginTime = _dateTime.ServerTime,
                SignInWithActiveDirectory = param.SignInWithActiveDirectory,
                Action = param.Action.ToUpper(),
                UserIn = param.IdUser
            };

            _dbContext.Entity<TrLoginTransactionLog>().Add(newLoginTransaction);
            await _dbContext.SaveChangesAsync();

            return Request.CreateApiResult2();
        }
    }
}
