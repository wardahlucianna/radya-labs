using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.UserRole;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.UserRole.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.UserRole
{
    public class AddUserRoleByRoleCodeHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        public AddUserRoleByRoleCodeHandler(
         IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override  async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddUserRoleByRoleCodeRequest, AddUserRoleByRoleCodeValidator>();

            var dataUser = await _dbContext.Entity<MsUser>()
                    .Where(x => x.Id == body.IdUser).FirstOrDefaultAsync(CancellationToken);

            if (dataUser == null)
                throw new BadRequestException("User is not found");


            var dataRole = await _dbContext.Entity<LtRole>()
                   .Where(x => x.Code == body.RoleCode 
                        && x.IdSchool == body.IdSchool)
                   .FirstOrDefaultAsync(CancellationToken);

            //SES => Supervisor Electives
            //EC  => External Coach

            if (dataRole == null)
                throw new BadRequestException("Role is not found");


            var dataUserRole = await _dbContext.Entity<MsUserRole>()
                  .Where(x => x.IdUser == body.IdUser
                       && x.IdRole == dataRole.Id)
                  .FirstOrDefaultAsync(CancellationToken);

            if(dataUserRole == null)
            {
                var addUserRole = new MsUserRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUser = body.IdUser,
                    IdRole = dataRole.Id,
                    IsDefault = true,
                    Username = dataUser.Username
                };
                _dbContext.Entity<MsUserRole>().Add(addUserRole);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
