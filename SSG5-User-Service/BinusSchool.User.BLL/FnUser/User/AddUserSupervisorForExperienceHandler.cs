using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.User.Validator;
using BinusSchool.User.FnUser.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.User.FnUser.User
{
    public class AddUserSupervisorForExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        protected IDbContextTransaction Transaction;

        public AddUserSupervisorForExperienceHandler(
            IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddUserSupervisorForExperienceRequest, AddUserSupervisorForExperienceValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var dataRole = await _dbContext.Entity<LtRole>()
                       .Where(x => x.Id == body.IdRole).FirstOrDefaultAsync(CancellationToken);

            if(dataRole == null)
                throw new BadRequestException("Role is not found");

            var dataEmail = await _dbContext.Entity<MsUser>()
                       .Where(x => x.Email.ToLower() == body.Email).FirstOrDefaultAsync(CancellationToken);

            // if(dataEmail != null)
            //     throw new BadRequestException("Email is taken");

            // var userId = Guid.NewGuid().ToString();
            var userId = body.IdUser;
            _dbContext.Entity<MsUser>().Add(new MsUser
            {
                Id = userId,
                Username = body.Email,
                DisplayName = body.Email,
                Email = body.Email,
                IsActiveDirectory = body.IsActiveDirectory,
                Status = true
            });

            _dbContext.Entity<MsUserSchool>().Add(new MsUserSchool
            {
                Id = Guid.NewGuid().ToString(),
                IdUser = userId,
                IdSchool = body.IdSchool
            });

            _dbContext.Entity<MsUserRole>().Add(new MsUserRole
            {
                Id = body.IdUserRole,
                IdUser = userId,
                IdRole = body.IdRole,
                IsDefault = true,
                Username = body.Email
            });

            // add random password
            // var password = Generator.GenerateRandomPassword(6);
            // var password = "qwerty@123";
            var password = body.Password;

            var salt = Generator.GenerateSalt();
            _dbContext.Entity<MsUserPassword>().Add(new MsUserPassword
            {
                Id = userId,
                Salt = salt,
                HashedPassword = (password + salt).ToSHA512()
            });

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(new { userId = userId, userEmail = body.Email, userSchool = body.IdSchool} as object);
        }
    }
}
