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
    public class UpdateAllUserRoleByRoleCodeHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private string[] RoleAllow = { "SES", "CE" };
        public UpdateAllUserRoleByRoleCodeHandler(
         IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateAllUserRoleByRoleCodeRequest, UpdateAllUserRoleByRoleCodeValidator>();

            var RoleAllowed = RoleAllow.Contains(body.RoleCode);
            if (!RoleAllowed)
            {
                throw new BadRequestException($"{body.RoleCode} Role is not allowed to update data");
            }

            body.UserList = body.UserList.Distinct().ToList(); 

            var dataUser = await _dbContext.Entity<MsUser>()
                            .Where(x => body.UserList.Select(b => b.IdUser).Contains(x.Id)).ToListAsync(CancellationToken);

            
            var notFoundDataUser = body.UserList.Select(a => a.IdUser).Except(dataUser.Select(x => x.Id)).ToList();

            //if (notFoundDataUser.Length != 0)
            //    throw new BadRequestException($"Some user is not found: {string.Join(", ", notFoundDataUser)}");

            var filterUserList = body.UserList
                .Where(x => dataUser.Select(y => y.Id).Contains(x.IdUser))
                .ToList();


            var dataRole = await _dbContext.Entity<LtRole>()
                   .Where(x => x.Code == body.RoleCode
                        && x.IdSchool == body.IdSchool)
                   .FirstOrDefaultAsync(CancellationToken);

            //SES => Supervisor Electives
            //EC  => External Coach

            if (dataRole == null)
                throw new BadRequestException("Role is not found");



            var DeleteUser = await _dbContext.Entity<MsUserRole>()
                            .Where(a => !filterUserList.Select(b => b.IdUser).Contains(a.IdUser) && 
                            a.Role.Code == body.RoleCode &&
                            a.Role.IdSchool == body.IdSchool)
                            .ToListAsync();


            if(DeleteUser.Count > 0)
            {

                _dbContext.Entity<MsUserRole>().RemoveRange(DeleteUser);
                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            var GetUser = await _dbContext.Entity<MsUserRole>()
                            .Where(a => a.Role.IdSchool == body.IdSchool &&
                             a.Role.Code == body.RoleCode)
                            .ToListAsync();

            if (filterUserList.Count() > 0)
            {
                var currInsertUserRole = filterUserList.Where(a => !GetUser.Select(b => b.IdUser).Contains(a.IdUser))
                                                    .Select(a => new MsUserRole() { 
                                                    Id = Guid.NewGuid().ToString(),
                                                    IdUser = a.IdUser,
                                                    IdRole = dataRole.Id,
                                                    IsDefault = true,
                                                    Username = dataUser.Where(b => b.Id == a.IdUser).FirstOrDefault().Username
                                                    })    
                                                    .ToList();
                _dbContext.Entity<MsUserRole>().AddRange(currInsertUserRole);

                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            return Request.CreateApiResult2();
        }
    }
}

