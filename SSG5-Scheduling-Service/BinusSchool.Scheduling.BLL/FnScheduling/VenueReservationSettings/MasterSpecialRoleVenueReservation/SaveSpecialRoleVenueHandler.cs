using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.Validator;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation
{
    public class SaveSpecialRoleVenueHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveSpecialRoleVenueHandler (ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveSpecialRoleVenueRequest, SaveSpecialRoleVenueValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);



                //Create new
                if (param.IdSpecialRoleVenue == null)
                {
                    var checkData = await _dbContext.Entity<MsSpecialRoleVenue>()
                        .Where(x => x.IdRole == param.IdRole)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (checkData != null)
                    {
                        throw new Exception("Role already exist");
                    }

                    var addData = new MsSpecialRoleVenue()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdRole = param.IdRole,
                        SpecialDurationBookingTotalDay = param.SpecialDurationBookingTotalDay,
                        CanOverrideAnotherReservation = param.CanOverrideAnotherReservation ?? false,
                        AllSuperAccess = param.AllSuperAccess ?? false
                    };

                    _dbContext.Entity<MsSpecialRoleVenue>().Add(addData);
                }
                //Update existing
                else
                {
                    var updateData = await _dbContext.Entity<MsSpecialRoleVenue>()
                        .Where(x => x.Id == param.IdSpecialRoleVenue)
                        .FirstOrDefaultAsync(CancellationToken);

                    updateData.SpecialDurationBookingTotalDay = param.SpecialDurationBookingTotalDay;
                    updateData.CanOverrideAnotherReservation = param.CanOverrideAnotherReservation ?? false;
                    updateData.AllSuperAccess = param.AllSuperAccess ?? false;

                    _dbContext.Entity<MsSpecialRoleVenue>().Update(updateData);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
            }
            catch (Exception ex)
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
