using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction
{
    public class SaveMasterDayRestrictionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        public SaveMasterDayRestrictionHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveMasterDayRestrictionRequest, SaveMasterDayRestrictionValidator>();

            if(param.StartRestrictionDate < _dateTime.ServerTime || param.EndRestrictionDate < _dateTime.ServerTime)
            {
                throw new Exception("Start and End Restriction Date must be greater than current date");
            }

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getRestriction = await _dbContext.Entity<MsRestrictionBookingVenue>()
                    .ToListAsync(CancellationToken);

                if (param.IdGroupRestriction != null)
                {
                    var deleteData = getRestriction
                        .Where(x => x.IdGroupRestriction == param.IdGroupRestriction)
                        .ToList();

                    deleteData.ForEach(x => x.IsActive = false);

                    _dbContext.Entity<MsRestrictionBookingVenue>().UpdateRange(deleteData);
                }

                var idGroupRestriction = param.IdGroupRestriction == null? Guid.NewGuid().ToString() : param.IdGroupRestriction;

                var addListData = new List<MsRestrictionBookingVenue>();

                var checkData = getRestriction
                    .Where(x => param.StartRestrictionDate >= x.StartRestrictionDate && param.StartRestrictionDate <= x.EndRestrictionDate)
                    .Where(x => param.EndRestrictionDate >= x.StartRestrictionDate && param.EndRestrictionDate <= x.EndRestrictionDate)
                    .Where(x => x.IdGroupRestriction != idGroupRestriction)
                    .ToList();


                if (param.IdVenue.Count() > 0)
                {
                    var getVenue = await _dbContext.Entity<MsVenue>()
                        .Where(x => param.IdVenue.Contains(x.Id)).ToListAsync(CancellationToken);

                    foreach (var venue in param.IdVenue)
                    {
                        var filteredVenue = getVenue.Where(x => x.Id == venue).FirstOrDefault();

                        if(checkData.Where(x => x.IdBuilding == filteredVenue.IdBuilding && x.IdVenue == null).Any())
                        {
                            throw new Exception("Venue / Building already have restriction date");
                        }

                        var addData = new MsRestrictionBookingVenue()
                        {
                            Id = Guid.NewGuid().ToString(),
                            StartRestrictionDate = param.StartRestrictionDate,
                            EndRestrictionDate = param.EndRestrictionDate,
                            IdBuilding = filteredVenue.IdBuilding,
                            IdVenue = venue,
                            IdGroupRestriction = idGroupRestriction
                        };
                        addListData.Add(addData);
                    }
                }
                else if(param.IdBuilding.Count() > 0)
                {
                    foreach (var building in param.IdBuilding)
                    {
                        if (checkData.Where(x => x.IdBuilding == building && x.IdVenue == null).Any())
                        {
                            throw new Exception("Building already have restriction date");
                        }

                        var addData = new MsRestrictionBookingVenue()
                        {
                            Id = Guid.NewGuid().ToString(),
                            StartRestrictionDate = param.StartRestrictionDate,
                            EndRestrictionDate = param.EndRestrictionDate,
                            IdBuilding = building,
                            IdVenue = null,
                            IdGroupRestriction = idGroupRestriction
                        };
                        addListData.Add(addData);
                    }
                }
                _dbContext.Entity<MsRestrictionBookingVenue>().AddRange(addListData);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

            }
            catch(Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
            
            return Request.CreateApiResult2();
        }
    }
}
