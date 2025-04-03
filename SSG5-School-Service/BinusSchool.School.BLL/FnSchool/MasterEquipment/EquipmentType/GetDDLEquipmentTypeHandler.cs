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
    public class GetDDLEquipmentTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDDLEquipmentTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDDLEquipmentTypeRequest>(nameof(GetDDLEquipmentTypeRequest.IdSchool));

            var data = await _dbContext.Entity<MsEquipmentType>()
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new NameValueVm
                {
                    Id = x.Id,
                    Name = x.EquipmentTypeName
                })
                .ToListAsync(CancellationToken);


            return Request.CreateApiResult2(data as object);

            
        }
    }
}
