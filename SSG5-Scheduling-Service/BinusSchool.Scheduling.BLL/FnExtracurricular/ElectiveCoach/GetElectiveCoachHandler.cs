using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class GetElectiveCoachHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetElectiveCoachHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetElectiveCoachRequest>();

            var getStaff = await _dbContext.Entity<MsUser>()
                                    .Include(x => x.UserRoles)
                                    .ThenInclude(y => y.Role)
                                    .ThenInclude(y => y.RoleGroup)            
                                    .Where(a => a.UserRoles.Any(b => b.Role.IdSchool == param.IdSchool
                                    && a.UserRoles.Any(b => b.Role.IdRoleGroup != "GU"))
                                    && a.Status == true)
                                    .Select(a => new 
                                    {
                                        IdUser = a.Id,
                                        UserName = a.DisplayName + (a.UserRoles.Any(b => b.Role.Code == "SE") ? " (SE)" : ""),
                                        IsSE = a.UserRoles.Any(b => b.Role.Code == "SE")                                    
                                    })
                                    //.OrderByDescending(a => a.IsSE)
                                    //.Take(1000)                                   
                                    .ToListAsync();


            var getstaff2 = getStaff.GroupJoin(_dbContext.Entity<MsStaff>(),
                                            p => (p.IdUser),
                                            h => (h.IdBinusian),
                                            (p, h) => new { p, h })
                                    .SelectMany(x => x.h.DefaultIfEmpty(),
                                        (user, staff) => new { user, staff })
                                    .Select(a => new GetElectiveCoachResult
                                    {
                                        IdUser = a.user.p.IdUser,
                                        UserName = (a.staff != null ? (a.staff.FirstName + " " + (a.staff.LastName != null ? a.staff.LastName : "")) : a.user.p.UserName),
                                        Order = (a.user.p.IsSE) ? 1 : 0
                                    })
                                    .OrderByDescending(a => a.Order)
                                    .ToList();

            return Request.CreateApiResult2(getstaff2 as object);
           
        }
    }
}
