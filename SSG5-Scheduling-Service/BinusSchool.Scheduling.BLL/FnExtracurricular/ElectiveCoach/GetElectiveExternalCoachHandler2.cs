using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class GetElectiveExternalCoachHandler2 : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetElectiveExternalCoachHandler2(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetElectiveExternalCoachRequest>(nameof(GetElectiveExternalCoachRequest.IdSchool));

            var getPrivilegeUserElective = new List<GetElectiveExternalCoachResult>();

            var school = await _dbContext.Entity<MsSchool>()
                                .Where(a => a.Id == param.IdSchool).FirstOrDefaultAsync();

            if (school is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["school"], "Id", param.IdSchool));

            var predicateData = PredicateBuilder.True<CoachDataVm>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicateData = predicateData.And(x
                    => EF.Functions.Like(x.UserName, param.SearchPattern())
                    || EF.Functions.Like(x.IdExternalCoach, param.SearchPattern())
                    || EF.Functions.Like(x.NPWP, param.SearchPattern())
                    || EF.Functions.Like(x.AccountBank, param.SearchPattern())
                    || EF.Functions.Like(x.AccountBankBranch, param.SearchPattern())
                    || EF.Functions.Like(x.AccountNumber, param.SearchPattern())
                    || EF.Functions.Like(x.TaxStatusName, param.SearchPattern())
                    );

            var GetUserExternalCoach = await _dbContext.Entity<MsUserRole>()
                                        .Include(x => x.Role)
                                        .Include(x => x.User)
                                        .Where(a => a.Role.IdSchool == param.IdSchool
                                        && a.Role.Code == "EC")
                                        .Select(a => new { 
                                        IdUser = a.IdUser,
                                        UserName = a.User.DisplayName
                                        })
                                        .ToListAsync();

            var GetCoachData = GetUserExternalCoach.GroupJoin(
                                        _dbContext.Entity<MsExtracurricularExternalCoach>()
                                        .Include(x => x.ExtracurricularExtCoachTaxStatus)
                                        .Include(x => x.ExtracurricularExternalCoachAtts)
                                        .Include(x => x.ExtracurricularExtCoachMappings),
                                        user => user.IdUser,
                                        ec => ec.Id,
                                        (user, ec) => new { user, ec }
                                        ).SelectMany(x => x.ec.DefaultIfEmpty(),
                                        (user, ec) => new { user.user, ec })
                                        .Select(a => new CoachDataVm
                                        {
                                            IdUser = a.user.IdUser,
                                            UserName = a.ec?.Name??a.user.UserName, // (a.ec == null ? a.user.UserName : a.ec.Name),
                                            IdExternalCoach = a.ec?.IdExternalCoach ?? "",
                                            NPWP = a.ec?.NPWP??"",
                                            AccountBank = a.ec?.AccountBank??"",
                                            AccountBankBranch = a.ec?.AccountBankBranch??"",
                                            AccountNumber = a.ec?.AccountNumber??"",
                                            AccountName = a.ec?.AccountName??"",
                                            IdTaxStatus = a.ec?.IdExtracurricularExtCoachTaxStatus??"",
                                            TaxStatusName = a.ec?.ExtracurricularExtCoachTaxStatus.Description??"",
                                            CanDelete = a.ec == null ? false : a.ec.ExtracurricularExternalCoachAtts.Any() ? false : a.ec.ExtracurricularExtCoachMappings.Any() ? false : true
                                        })                                        
                                        .ToList();

            var queryData = GetCoachData.AsQueryable()
                        //.SearchByIds(param)
                        .Where(predicateData);


            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = queryData
                    .Select(x => new ItemValueVm
                    {
                        Id = x.IdUser,
                        Description = x.UserName
                    })
                    .ToList();
                items = result;
            }
            else
            {
                var result = queryData
                            .SetPagination(param)
                            .Select(a => new GetElectiveExternalCoachResult
                            {
                                Id = a.IdUser,
                                Description = a.UserName,
                                IdExternalCoach = (!string.IsNullOrWhiteSpace(a.IdExternalCoach) ? a.IdExternalCoach : "-"),
                                TaxStatus = new ItemValueVm() { Id = a.IdTaxStatus, Description = (!string.IsNullOrWhiteSpace(a.TaxStatusName) ? a.TaxStatusName : "-") },
                                NPWP = (!string.IsNullOrWhiteSpace(a.NPWP) ? a.NPWP : "-"),
                                AccountBank = (!string.IsNullOrWhiteSpace(a.AccountBank + " " + a.AccountBankBranch) ? (a.AccountBank + " - " + a.AccountBankBranch) : "-"),
                                AcountNumber = (!string.IsNullOrWhiteSpace(a.AccountNumber) ? a.AccountNumber : "-"),
                                AcountName = (!string.IsNullOrWhiteSpace(a.AccountName) ? a.AccountName : "-"),
                                CanDelete = a.CanDelete
                            })
                            .ToList();

                items = result;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await queryData.Select(x => x.IdUser).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));

            
        }
      
        private class CoachDataVm 
        {          
            public string IdUser { set; get; }
            public string UserName { set; get; }
            public string IdExternalCoach { set; get; }
            public string NPWP { set; get; }
            public string AccountBank { set; get; }
            public string AccountBankBranch { set; get; }
            public string AccountNumber { set; get; }
            public string AccountName { set; get; }
            public string IdTaxStatus { set; get; }
            public string TaxStatusName { set; get; }            
            public bool CanDelete { set; get; }            
        }

    }
}
