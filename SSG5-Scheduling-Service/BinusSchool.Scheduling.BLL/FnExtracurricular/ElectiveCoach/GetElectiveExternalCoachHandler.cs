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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class GetElectiveExternalCoachHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetElectiveExternalCoachHandler(
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
           
            
            var predicate = PredicateBuilder.True<MsExtracurricularExternalCoach>();

            if (!string.IsNullOrWhiteSpace(param.IdSchool))
                predicate = predicate.And(x => x.IdSchool == param.IdSchool.ToString());

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Id, param.SearchPattern())
                    || EF.Functions.Like(x.Name, param.SearchPattern())
                    || EF.Functions.Like(x.NPWP, param.SearchPattern())
                    || EF.Functions.Like(x.AccountBank, param.SearchPattern())
                    || EF.Functions.Like(x.AccountBankBranch, param.SearchPattern())
                    || EF.Functions.Like(x.AccountNumber, param.SearchPattern())
                    || EF.Functions.Like(x.ExtracurricularExtCoachTaxStatus.Description, param.SearchPattern())
                    );

            var query = _dbContext.Entity<MsExtracurricularExternalCoach>()
                          .Include(x => x.ExtracurricularExtCoachTaxStatus)                         
                          //.SearchByIds(param)
                          .Where(predicate);
                  
            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.Name
                    })
                    .ToListAsync(CancellationToken);
                items = result;
            }
            else
            {                
                var result = query.Include(x => x.ExtracurricularExtCoachTaxStatus)
                                    .SetPagination(param)
                                    .Select(a => new GetElectiveExternalCoachResult
                                    {
                                        Id = a.Id,
                                        Description = a.Name,
                                        TaxStatus = new ItemValueVm() { Id = a.IdExtracurricularExtCoachTaxStatus, Description = (a.ExtracurricularExtCoachTaxStatus != null ? a.ExtracurricularExtCoachTaxStatus.Description : "-") },
                                        NPWP = (!string.IsNullOrWhiteSpace(a.NPWP) ? a.NPWP : "-"),
                                        AccountBank = (!string.IsNullOrWhiteSpace(a.AccountBank + " " + a.AccountBankBranch) ? (a.AccountBank + " - " + a.AccountBankBranch) : "-"),
                                        AcountNumber = (!string.IsNullOrWhiteSpace(a.AccountNumber) ? a.AccountNumber : "-")
                                    })                                  
                                    .ToList();

                items = result;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
