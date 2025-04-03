using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Employee.FnStaff.IMTAMajorAssignPosition;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Employee.FnStaff.IMTAMajorAssignPosition
{
    public class IMTAMajorAssignPositionHandler : FunctionsHttpSingleHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        public IMTAMajorAssignPositionHandler(IEmployeeDbContext employeeDbContext)
        {
            _dbContext = employeeDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CollectionRequest>();

            var predicate = PredicateBuilder.Create<LtIMTAMajorAssignPosition>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IMTAMajorAssignPosition, $"%{param.Search}%"));

            var query = _dbContext.Entity<LtIMTAMajorAssignPosition>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            items = await query
                .Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.IMTAMajorAssignPosition
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }
    }
}