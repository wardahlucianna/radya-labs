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
using BinusSchool.School.FnSchool.VenueEquipment.Validator;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.VenueEquipment
{
    public class DeleteVenueEquipmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public DeleteVenueEquipmentHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteVenueEquipmentRequest, DeleteVanueEquipmentValidator>();

            try
            {
                var data = await _dbContext.Entity<MsVenueEquipment>()
                    .Where(x => param.IdVenue.Contains(x.IdVenue))
                    .OrderBy(x => x.Id)
                    .ToListAsync(CancellationToken);

                data.ForEach(x => x.IsActive = false);

                _dbContext.Entity<MsVenueEquipment>().UpdateRange(data);

                await _dbContext.SaveChangesAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Request.CreateApiResult2();
        }
    }
}
