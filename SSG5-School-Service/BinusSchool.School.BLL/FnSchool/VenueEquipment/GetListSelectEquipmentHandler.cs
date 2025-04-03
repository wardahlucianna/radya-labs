using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueEquipment;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.School.FnSchool.VenueEquipment
{
    public class GetListSelectEquipmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetListSelectEquipmentHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListSelectEquipmentRequest>();

            var query = _dbContext.Entity<MsEquipment>()
                .Include(x => x.EquipmentType)
                .ThenInclude(x => x.ReservationOwner)
                .Where(x => (param.EquipmentType == null || x.IdEquipmentType == param.EquipmentType) && 
                            (param.EquipmentType != null || x.EquipmentType.IdSchool == param.IdSchool))
                .Select(x => new GetListSelectEquipmentResult
                {
                    IdEquipmentType = x.IdEquipmentType,
                    EquipmentType = x.Id,
                    EquipmentTypeName = x.EquipmentType.EquipmentTypeName,
                    Owner = x.EquipmentType.ReservationOwner.OwnerName,
                    EquipmentName = x.EquipmentName
                });

            var data = await query.ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
