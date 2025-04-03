using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MasterEquipment.EquipmentDetails
{
    public class GetDetailEquipmentDetailsHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDetailEquipmentDetailsHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailEquipmentDetailsRequest>(nameof(GetDetailEquipmentDetailsRequest.IdEquipment));

            var getData = await _dbContext.Entity<MsEquipment>()
                .Include(x => x.EquipmentType)
                .Where(x => x.Id == param.IdEquipment)
                .Select(x => new GetDetailEquipmentDetailsResult
                {
                    IdEquipment = x.Id,
                    EquipmentType = new ItemValueVm
                    {
                        Id = x.EquipmentType.Id,
                        Description = x.EquipmentType.EquipmentTypeName
                    },
                    EquipmentName = x.EquipmentName,
                    TotalStockQty = x.TotalStockQty,
                    MaxQtyBorrowing = x.MaxQtyBorrowing,
                    EquipmentDescription = x.Description
                }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(getData as object);
        }
    }
}
