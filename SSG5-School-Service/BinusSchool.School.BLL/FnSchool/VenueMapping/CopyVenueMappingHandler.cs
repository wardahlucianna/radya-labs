using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.VenueMapping;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.VenueMapping.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.VenueMapping
{
    public class CopyVenueMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public CopyVenueMappingHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CopyVenueMappingRequest, CopyVenueMappingValidator>();

            var getVenueMapping = await _dbContext.Entity<MsVenueMapping>()
                .Where(x => x.IdAcademicYear == param.IdAcademicYearSource || x.IdAcademicYear == param.IdAcademicYearDestination)
                .ToListAsync(CancellationToken);

            var getVenueMappingApprover = await _dbContext.Entity<MsVenueMappingApproval>()
                .Include(x => x.VenueMapping)
                .Where(x => x.VenueMapping.IdAcademicYear == param.IdAcademicYearSource)
                .ToListAsync(CancellationToken);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var sourceVenueMapping = getVenueMapping.Where(x => x.IdAcademicYear == param.IdAcademicYearSource).ToList();
                var destinationVenueMapping = getVenueMapping.Where(x => x.IdAcademicYear == param.IdAcademicYearDestination).ToList();

                //Only add data that not exist in destination
                var addVenueMapping = sourceVenueMapping.Where(x => !destinationVenueMapping.Select(y => y.IdVenue).Contains(x.IdVenue)).ToList();

                foreach (var data in addVenueMapping)
                {
                    var addData = new MsVenueMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = param.IdAcademicYearDestination,
                        IdVenue = data.IdVenue,
                        IdFloor = data.IdFloor,
                        IdReservationOwner = data.IdReservationOwner,
                        IdVenueType = data.IdVenueType,
                        IsNeedApproval = data.IsNeedApproval,
                        IsVenueActive = data.IsVenueActive,
                        Description = data.Description
                    };
                    _dbContext.Entity<MsVenueMapping>().Add(addData);

                    var approver = getVenueMappingApprover.Where(x => x.IdVenueMapping == data.Id).ToList();

                    if (approver.Any())
                    {
                        foreach (var app in approver)
                        {
                            var addApprovalData = new MsVenueMappingApproval
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdVenueMapping = addData.Id,
                                IdBinusian = app.IdBinusian,
                            };
                            _dbContext.Entity<MsVenueMappingApproval>().Add(addApprovalData);
                        }
                    }
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
