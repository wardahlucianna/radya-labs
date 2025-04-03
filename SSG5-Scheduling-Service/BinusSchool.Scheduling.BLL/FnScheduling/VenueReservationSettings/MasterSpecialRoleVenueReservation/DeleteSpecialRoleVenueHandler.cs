using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation
{
    public class DeleteSpecialRoleVenueHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteSpecialRoleVenueHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteSpecialRoleVenueRequest, DeleteSpecialRoleVenueValidator>();

            var data = await _dbContext.Entity<MsSpecialRoleVenue>()
                .Where(x => x.Id == param.IdSpecialRoleVenue)
                .FirstOrDefaultAsync(CancellationToken);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                data.IsActive = false;
                _dbContext.Entity<MsSpecialRoleVenue>().Update(data);
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
