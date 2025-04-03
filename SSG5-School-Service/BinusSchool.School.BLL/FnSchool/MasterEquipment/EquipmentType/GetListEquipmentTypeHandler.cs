using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MasterEquipment.EquipmentType
{
    public class GetListEquipmentTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetListEquipmentTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListEquipmentTypeRequest>(nameof(GetListEquipmentTypeRequest.IdSchool));

            var data = await _dbContext.Entity<MsEquipmentType>()
                .Include(x => x.ReservationOwner)
                .Include(x => x.Equipments)
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new GetListEquipmentTypeResult
                {
                    IdEquipmentType = x.Id,
                    EquipmentTypeName = x.EquipmentTypeName,
                    ReservationOwner = new NameValueVm
                    {
                        Id = x.ReservationOwner.Id,
                        Name = x.ReservationOwner.OwnerName
                    },
                    CanDelete = !x.Equipments.Any()
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }

    }
}
