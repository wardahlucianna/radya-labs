using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction
{
    public class DeleteMasterDayRestrictionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public DeleteMasterDayRestrictionHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteMasterDayRestrictionRequest, DeleteMasterDayRestrictionValidator>();

            try
            {
                var deleteData = await _dbContext.Entity<MsRestrictionBookingVenue>()
                    .Where(x => x.IdGroupRestriction == param.IdGroupRestriction)
                    .ToListAsync(CancellationToken);

                if (deleteData != null)
                {
                    _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                    deleteData.ForEach(x => x.IsActive = false);
                    _dbContext.Entity<MsRestrictionBookingVenue>().UpdateRange(deleteData);

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
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
