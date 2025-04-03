using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MasterEquipment.EquipmentType
{
    public class GetDetailEquipmentTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDetailEquipmentTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailEquipmentTypeRequest>(nameof(GetDetailEquipmentTypeRequest.IdEquipmentType));

            var getDetailEquipmentType = await _dbContext.Entity<MsEquipmentType>()
                .Include(x => x.ReservationOwner)
                .Where(x => x.Id == param.IdEquipmentType)
                .Select(x => new GetDetailEquipmentTypeResult
                {
                    IdEquipmentType = x.Id,
                    EquipmentTypeName = x.EquipmentTypeName,
                    ReservationOwner = new NameValueVm
                    {
                        Id = x.ReservationOwner.Id,
                        Name = x.ReservationOwner.OwnerName
                    }
                })
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(getDetailEquipmentType as object);
        }
    }
}
