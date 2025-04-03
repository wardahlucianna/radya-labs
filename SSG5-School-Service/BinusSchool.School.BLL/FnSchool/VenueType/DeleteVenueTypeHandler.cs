using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueType;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.VenueType.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.VenueType
{
    public class DeleteVenueTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteVenueTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteVenueTypeRequest, DeleteVenueTypeValidator>();

            var venueType = await _dbContext.Entity<LtVenueType>()
                .Include(x => x.VenueMappings)
                .Where(x => param.IdVenueType.Contains(x.Id))
                .ToListAsync();

            if (!venueType.Any())
            {
                throw new Exception("Venue Type not found");
            }

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach(var data in venueType)
                {
                    if(data.VenueMappings.Any())
                    {
                        throw new Exception("Venue Type cannot be deleted because it is used in Venue Mapping");
                    }

                    data.IsActive = false;

                    _dbContext.Entity<LtVenueType>().Update(data);
                }



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
