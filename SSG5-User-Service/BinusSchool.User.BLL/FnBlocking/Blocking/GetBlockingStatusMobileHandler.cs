using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessage;
using BinusSchool.Persistence.UserDb;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Student;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using StackExchange.Redis;

namespace BinusSchool.User.FnBlocking.Blocking
{
    public class GetBlockingStatusMobileHandler : FunctionsHttpSingleHandler
    {
        private IDictionary<string, object> _DataMessageBlocking;
        private readonly IUserDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetBlockingStatusMobileHandler(IUserDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBlockingStatusMobileRequest>();

            var mobileBlockingStatus = new GetBlockingStatusMobileResult();

            var isFeatureActive = await _featureManager.IsEnabledAsync(FeatureFlags.StudentBlocking);

            var msUser = await _dbContext.Entity<MsUser>()
                .Include(x => x.UserSchools)
                    .ThenInclude(x => x.School)
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                        .ThenInclude(x => x.RoleGroup)
                .Where(x => x.Id == param.IdStudent && x.UserSchools.Any(a => a.IdSchool == param.IdSchool))
                .FirstOrDefaultAsync(CancellationToken);

            var userRole = msUser.UserRoles.OrderByDescending(role => role.IsDefault).Select(x => new
            {
                IdRole = x.Role.Id,
                Name = x.Role.Description,
            });

            if (isFeatureActive)
            {
                if (msUser.UserRoles.Any(x => x.Role.RoleGroup.Description.ToUpper() == RoleConstant.Student))
                {
                    var dataStudentBLocking = GetDataBlocking(msUser.UserSchools.FirstOrDefault().IdSchool, msUser.Id);

                    mobileBlockingStatus.IsBlock = string.IsNullOrEmpty(dataStudentBLocking.Result) ? false : true;
                    mobileBlockingStatus.BlockingMessage = dataStudentBLocking.Result;

                }
                else
                {
                    mobileBlockingStatus.IsBlock = false;
                }
            }

            return Request.CreateApiResult2(mobileBlockingStatus as object);
        }

        protected async Task<string> GetDataBlocking(string idSchool, string idStudent)
        {
            var GeneratedData = "";

            if (!string.IsNullOrEmpty(idSchool) && !string.IsNullOrEmpty(idStudent))
            {
                var Message = await _dbContext.Entity<MsBlockingMessage>()
                .Where(e => e.IdSchool == idSchool)
                .Select(e => e.Content)
                .FirstOrDefaultAsync(CancellationToken);
                if (Message != null)
                {
                    var pushTitle = Handlebars.Compile(Message);

                    var dataStudentBlocking = await _dbContext.Entity<MsStudentBlocking>()
                                        .Include(x => x.BlockingCategory)
                                        .Include(x => x.BlockingType)
                                        .Where(x => x.IdStudent == idStudent && x.BlockingType.Category == "WEBSITE" && x.IsBlocked)
                                        .Select(e => e.BlockingCategory.Name)
                                        .ToListAsync(CancellationToken);

                    if (dataStudentBlocking.Count != 0)
                    {
                        _DataMessageBlocking = new Dictionary<string, object>
                    {
                        { "categoryBlocking", string.Join(", ", dataStudentBlocking) },
                    };
                        GeneratedData = pushTitle(_DataMessageBlocking);
                    }
                }

            }
            return GeneratedData;
        }
    }

    public class GetBlockingStatusMobileResult
    {
        public bool IsBlock { get; set; }
        public string BlockingMessage { get; set; }
    }
}
