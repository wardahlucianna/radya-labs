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
    public class SaveVenueTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public SaveVenueTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveVenueTypeRequest, SaveVenueTypeValidator>();

            var venueType = await _dbContext.Entity<LtVenueType>()
                .Where(x => x.IdSchool == param.IdSchool)
                .ToListAsync(CancellationToken);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var venueTypeUpdate = venueType.Where(x => x.Id == param.IdVenueType).FirstOrDefault();

                if (venueTypeUpdate == null)
                {
                    var checkVenueType = venueType.Where(x => x.VenueTypeName == param.VenueTypeName).FirstOrDefault();

                    if (checkVenueType != null)
                    {
                        throw new Exception("Venue Type Name must be unique.");
                    }
        
                    var addData = new LtVenueType
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSchool = param.IdSchool,
                        VenueTypeName = param.VenueTypeName,
                        Description = param.Description
                    };
                    _dbContext.Entity<LtVenueType>().Add(addData);
                }
                else
                {
                    venueTypeUpdate.Description = param.Description;
                    _dbContext.Entity<LtVenueType>().Update(venueTypeUpdate);
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
