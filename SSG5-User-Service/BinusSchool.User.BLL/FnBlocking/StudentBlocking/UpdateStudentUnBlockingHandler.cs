using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.User.FnBlocking.StudentBlocking.Validator;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class UpdateStudentUnBlockingHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IUserDbContext _dbContext;

        private readonly IMachineDateTime _dateTime;

        public UpdateStudentUnBlockingHandler(IUserDbContext userDbContext, IMachineDateTime dateTime)
        {
            _dbContext = userDbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateStudentUnBlockingRequest, UpdateStudentUnBlockingValidator>();
            var idUserUnblocking = body.IdUsers.Select(x => x.IdUser).ToList();

            var dataUserBlocking = await _dbContext.Entity<MsStudentBlocking>()
                .Where(x => idUserUnblocking.Any(t => t == x.IdStudent)
                && x.IdBlockingCategory == body.IdBlockingCategory)
                .ToListAsync(CancellationToken);

            if(!string.IsNullOrEmpty(body.IdBlockingType))
                dataUserBlocking = dataUserBlocking.Where(x => x.IdBlockingType == body.IdBlockingType).ToList();
            
            foreach (var student in dataUserBlocking)
            {
                student.IsBlocked = false;
            }

            _dbContext.Entity<MsStudentBlocking>().UpdateRange(dataUserBlocking);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await UpdateHistoryBlocking(body, idUserUnblocking);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected async Task<object> UpdateHistoryBlocking(UpdateStudentUnBlockingRequest body, List<string> idUserUnblocking)
        {

            var historyAllUser = await _dbContext.Entity<HMsStudentBlocking>()
                            .Where(x => idUserUnblocking.Any(t => t == x.IdStudent)
                            && x.IdBlockingCategory == body.IdBlockingCategory
                            && x.EndDate == null)
                            .ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(body.IdBlockingType))
                historyAllUser = historyAllUser.Where(x => x.IdBlockingType == body.IdBlockingType).ToList();

            foreach (var item in historyAllUser)
            {
                item.EndDate = _dateTime.ServerTime;
            }

            _dbContext.Entity<HMsStudentBlocking>().UpdateRange(historyAllUser);
            
            await _dbContext.SaveChangesAsync(CancellationToken);

            return historyAllUser;
        }
    }
}
