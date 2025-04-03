using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Floor.Validator;
using BinusSchool.School.FnSchool.VenueReservationOwner.Validator;
using FluentEmail.Core; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.Floor
{
    public class DeletePICHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeletePICHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeletePICOwnerRequest, DeletePICValidator>();

            var data = await _dbContext.Entity<MsReservationOwner>()
                .Include(x => x.VenueMappings)
                .Where(x => param.IdReservationOwner.Contains(x.Id))
                .ToListAsync(CancellationToken);

            if (data.Any(x => x.VenueMappings.Any()))
            {
                throw new Exception("Data cannot be deleted because it is used in another table");
            }

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                foreach (var item in data)
                {
                    var HistoryData = new HMsReservationOwner
                    {
                        IdHMsReservationOwner = Guid.NewGuid().ToString(),
                        IdReservationOwner = item.Id,
                        OwnerName = item.OwnerName,
                        IdSchool = item.IdSchool,
                        IsPICVenue = item.IsPICVenue,
                        IsPICEquipment = item.IsPICEquipment,
                    };

                    _dbContext.Entity<HMsReservationOwner>().Add(HistoryData);
                    _dbContext.Entity<MsReservationOwner>().Update(item);
                }

                data.ForEach(x => x.IsActive = false);

                var existEmails = await _dbContext.Entity<MsReservationOwnerEmail>()
                    .Where(x => param.IdReservationOwner.Contains(x.IdReservationOwner))
                    .ToListAsync(CancellationToken);

                var HistoryEmails = new List<HMsReservationOwnerEmail>();

                foreach (var email in existEmails)
                {
                    var HistoryEmail = new HMsReservationOwnerEmail
                    {
                        IdHMsReservationOwnerEmail = Guid.NewGuid().ToString(),
                        IdReservationOwnerEmail = email.Id,
                        IdReservationOwner = email.IdReservationOwner,
                        OwnerEmail = email.OwnerEmail,
                        IsOwnerEmailTo = email.IsOwnerEmailTo,
                        IsOwnerEmailCC = email.IsOwnerEmailCC,
                        IsOwnerEmailBCC = email.IsOwnerEmailBCC
                    };
                    HistoryEmails.Add(HistoryEmail);
                    email.IsActive = false;
                    _dbContext.Entity<MsReservationOwnerEmail>().Update(email);
                }

                _dbContext.Entity<HMsReservationOwnerEmail>().AddRange(HistoryEmails);

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
