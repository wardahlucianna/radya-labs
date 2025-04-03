using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.LoginAs.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.User.FnUser.LoginAs.ManageAccessRoleLoginAs
{
    public class AddUpdateAccessRoleLoginAsHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public AddUpdateAccessRoleLoginAsHandler(
            IUserDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddUpdateAccessRoleLoginAsRequest, AddUpdateAccessRoleLoginAsValidator>();
            var getNewAccessRolesList = body.AccessRolesList.Distinct().ToList();
            var addAccessTransactionList = new List<TrRoleLoginAs>();

            var getRoleLoginAsList = _dbContext.Entity<TrRoleLoginAs>()
                    .Where(x => x.IdRole == body.IdRole)
                    .ToList();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                #region Remove Exisiting 
                var getRoleLoginAsListToDelete = getRoleLoginAsList
                        .Where(x => getNewAccessRolesList.All(y => y != x.IdAuthorizedRole))
                        .ToList();

                if (getRoleLoginAsListToDelete.Count() > 0)
                    _dbContext.Entity<TrRoleLoginAs>().RemoveRange(getRoleLoginAsListToDelete);

                if (getNewAccessRolesList.Count() == 0)
                    _dbContext.Entity<TrRoleLoginAs>().RemoveRange(getRoleLoginAsList);
                #endregion

                #region Add AccessRoleList
                var getAccessRolesListToAdd = getNewAccessRolesList
                        .Where(x => getRoleLoginAsList.All(y => y.IdAuthorizedRole != x))
                        .ToList();

                foreach (var addAccess in getAccessRolesListToAdd)
                {
                    var newAccessTransaction = new TrRoleLoginAs
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdRole = body.IdRole,
                        IdAuthorizedRole = addAccess,
                    };
                    addAccessTransactionList.Add(newAccessTransaction);
                }

                _dbContext.Entity<TrRoleLoginAs>().AddRange(addAccessTransactionList);
                #endregion

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
