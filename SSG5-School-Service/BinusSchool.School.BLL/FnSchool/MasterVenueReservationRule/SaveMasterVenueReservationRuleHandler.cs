using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MasterVenueReservationRule;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MasterVenueReservationRule.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.MasterVenueReservationRule
{
    public class SaveMasterVenueReservationRuleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveMasterVenueReservationRuleHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveMasterVenueReservationRuleRequest, SaveMasterVenueReservationRuleValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var data = await _dbContext.Entity<MsVenueReservationRule>()
                    .Where(x => x.IdSchool == param.IdSchool)
                    .FirstOrDefaultAsync(CancellationToken);

                if (data == null)
                {
                    var addData = new MsVenueReservationRule
                    {
                        Id = Guid.NewGuid().ToString(),
                        MaxDayBookingVenue = param.MaxDayBookingVenue,
                        MaxTimeBookingVenue = param.MaxTimeBookingVenue != null ? TimeSpan.Parse(param.MaxTimeBookingVenue) : (TimeSpan?)null,
                        MaxDayDurationBookingVenue = param.MaxDayDurationBookingVenue,
                        VenueNotes = param.VenueNotes,
                        StartTimeOperational = TimeSpan.Parse(param.StartTimeOperational),
                        EndTimeOperational = TimeSpan.Parse(param.EndTimeOperational),
                        CanBookingAnotherUser = param.CanBookingAnotherUser,
                        IdSchool = param.IdSchool
                    };

                    _dbContext.Entity<MsVenueReservationRule>().Add(addData);
                }
                else
                {
                    data.MaxDayBookingVenue = param.MaxDayBookingVenue;
                    data.MaxTimeBookingVenue = param.MaxTimeBookingVenue != null ? TimeSpan.Parse(param.MaxTimeBookingVenue) : (TimeSpan?)null;
                    data.MaxDayDurationBookingVenue = param.MaxDayDurationBookingVenue;
                    data.VenueNotes = param.VenueNotes;
                    data.StartTimeOperational = TimeSpan.Parse(param.StartTimeOperational);
                    data.EndTimeOperational = TimeSpan.Parse(param.EndTimeOperational);
                    data.CanBookingAnotherUser = param.CanBookingAnotherUser;

                    _dbContext.Entity<MsVenueReservationRule>().Update(data);
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
