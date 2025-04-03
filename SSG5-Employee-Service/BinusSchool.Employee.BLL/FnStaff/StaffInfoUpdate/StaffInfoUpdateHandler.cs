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
using BinusSchool.Data.Model.Employee.FnStaff.StaffInfoUpdate;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Employee.FnStaff.StaffInfoUpdate
{
    public class StaffInfoUpdateHandler : FunctionsHttpSingleHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        public StaffInfoUpdateHandler(IEmployeeDbContext employeeDbContext)
        {
            _dbContext = employeeDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStaffInfoUpdateRequest>(nameof(GetStaffInfoUpdateRequest.IdBinusian));
            var predicate = PredicateBuilder.Create<TrStaffInfoUpdate>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IdBinusian, $"%{param.Search}%"));

            var query = _dbContext.Entity<TrStaffInfoUpdate>()
                .Where(predicate)
                //.Where(x => x.IsParentUpdate == param.IsParentUpdate)
                .OrderByDynamic(param);

            IReadOnlyList<GetStaffInfoUpdateResult> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetStaffInfoUpdateResult
                    {
                        IdBinusian = x.IdBinusian,
                        TableName = x.TableName,
                        FieldName = x.FieldName,
                        OldFieldValue = x.OldFieldValue,
                        CurrentFieldValue = x.CurrentFieldValue,
                        Constraint1 = x.Constraint1,
                        Constraint1Value = x.Constraint1Value,
                        Constraint2 = x.Constraint2,
                        Constraint2Value = x.Constraint2Value,
                        Constraint3 = x.Constraint3,
                        Constraint3Value = x.Constraint3Value
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetStaffInfoUpdateResult
                    {
                        IdBinusian = x.IdBinusian,
                        TableName = x.TableName,
                        FieldName = x.FieldName,
                        OldFieldValue = x.OldFieldValue,
                        CurrentFieldValue = x.CurrentFieldValue,
                        Constraint1 = x.Constraint1,
                        Constraint1Value = x.Constraint1Value,
                        Constraint2 = x.Constraint2,
                        Constraint2Value = x.Constraint2Value,
                        Constraint3 = x.Constraint3,
                        Constraint3Value = x.Constraint3Value
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdBinusian).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
