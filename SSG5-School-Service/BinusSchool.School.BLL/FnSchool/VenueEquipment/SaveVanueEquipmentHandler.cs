using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueEquipment;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.VenueEquipment.Validator;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.School.FnSchool.VenueEquipment
{
    public class SaveVenueEquipmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public SaveVenueEquipmentHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveVenueEquipRequest, SaveVanueEquipValidator>();

            try
            {
                foreach(var item in param.EquipmentList)
                {
                    if(item.EquipmentQty < 1 || item.EquipmentQty > 10000)
                    {
                        throw new Exception("Invalid Quantity");
                    }
                }

                if(param.Status == 0)
                {
                    var paramVenue = param.EquipmentList.Select(x => x.IdVenue).FirstOrDefault();
                    
                    var existedData = await _dbContext.Entity<MsVenueEquipment>()
                        .Where(x => x.IdVenue == paramVenue)
                        .ToListAsync(CancellationToken);

                    if (existedData.Count() > 0)
                    {
                        throw new Exception("Venue Existed");
                    }

                    foreach (var item in param.EquipmentList)
                    {
                        var newData = new MsVenueEquipment
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdVenue = item.IdVenue,
                            IdEquipment = item.IdEquipment,
                            EquipmentQty = item.EquipmentQty
                        };

                        _dbContext.Entity<MsVenueEquipment>().Add(newData);
                    }
                }
                else
                {   
                        
                    var idVenue = param.EquipmentList.Select(x => x.IdVenue).FirstOrDefault();
                    var idVenueEquipment = param.EquipmentList.Select(x => x.IdVenueEquipment).ToList();

                    var allData = await _dbContext.Entity<MsVenueEquipment>()
                            .Where(x => x.IdVenue == idVenue)
                            .ToListAsync(CancellationToken);

                    var deletedData = await _dbContext.Entity<MsVenueEquipment>()
                        .Where(x => !(idVenueEquipment.Contains(x.Id)) && x.IsActive == true && x.IdVenue == idVenue)
                        .ToListAsync(CancellationToken);

                    deletedData.ForEach(x => x.IsActive = false);

                    foreach (var equipment in param.EquipmentList)
                    {
                        var venueEquipment = allData.FirstOrDefault(x => x.Id == equipment.IdVenueEquipment);
                        if (venueEquipment != null)
                        {
                            venueEquipment.EquipmentQty = equipment.EquipmentQty;
                        }
                        else
                        {
                            venueEquipment = new MsVenueEquipment
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdVenue = equipment.IdVenue,
                                IdEquipment = equipment.IdEquipment,
                                EquipmentQty = equipment.EquipmentQty
                            };
                            _dbContext.Entity<MsVenueEquipment>().Add(venueEquipment);
                        }
                    }

                    _dbContext.Entity<MsVenueEquipment>().UpdateRange(allData);
                }

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
