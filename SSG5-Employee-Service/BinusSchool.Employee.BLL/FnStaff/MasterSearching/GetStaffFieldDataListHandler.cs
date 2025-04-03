using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.MasterSearching;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Employee.FnStaff.MasterSearching
{
    public class GetStaffFieldDataListHandler : FunctionsHttpSingleHandler
    {

        private readonly IEmployeeDbContext _dbContext;
        public GetStaffFieldDataListHandler(IEmployeeDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var query = await _dbContext.Entity<MsFieldStaffData>()
                        .Select(x => new GetFieldDataListforMasterSearchingStaffResult
                        {
                            FieldId = Convert.ToInt32(x.IdFieldStaffData),
                            StaffDataFieldName = x.StaffData,
                            AliasName = x.AliasName
                        }).OrderBy(x => x.FieldId).ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
