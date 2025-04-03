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
    public class SaveVenueMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public SaveVenueMappingHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveVenueMappingRequest, SaveVenueMappingValidator>();

            var getVenueMapping = await _dbContext.Entity<MsVenueMapping>()
                .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                .ToListAsync(CancellationToken);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach (var data in param.VenueMappingDatas)
                {
                    var updateData = getVenueMapping.Where(x => x.IdVenue == data.Venue.Id).FirstOrDefault();

                    if(updateData == null)
                    {
                        var addData = new MsVenueMapping
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = param.IdAcademicYear,
                            IdVenue = data.Venue.Id,
                            IdFloor = data.Floor.Id,
                            IdReservationOwner = data.Owner.Id,
                            IdVenueType = data.VenueType.Id,
                            IsNeedApproval = data.NeedApproval.Count > 0 ? true : false,
                            IsVenueActive = data.VenueStatus,
                            Description = data.Description,
                        };

                        if (addData.IsNeedApproval)
                        {
                            var listApproval = new List<MsVenueMappingApproval>();
                            foreach(var approver in data.NeedApproval)
                            {
                                if(listApproval.Where(x => x.IdVenueMapping == addData.Id && x.IdBinusian == approver.Id).Any())
                                {
                                    throw new Exception("Each mapping venue can't have duplicate approver");
                                }

                                var addApprovalData = new MsVenueMappingApproval
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdVenueMapping = addData.Id,
                                    IdBinusian = approver.Id,
                                };

                                listApproval.Add(addApprovalData);
                            }
                            _dbContext.Entity<MsVenueMappingApproval>().AddRange(listApproval);
                        }

                        _dbContext.Entity<MsVenueMapping>().Add(addData);
                    }
                    else
                    {
                        updateData.IdAcademicYear = param.IdAcademicYear;
                        updateData.IdVenue = data.Venue.Id;
                        updateData.IdFloor = data.Floor.Id;
                        updateData.IdVenueType = data.VenueType.Id;
                        updateData.IdReservationOwner = data.Owner.Id;
                        updateData.IsNeedApproval = data.NeedApproval.Count > 0 ? true : false;
                        updateData.IsVenueActive = data.VenueStatus;
                        updateData.Description = data.Description;

                        _dbContext.Entity<MsVenueMapping>().Update(updateData);

                        var getApprovalData = await _dbContext.Entity<MsVenueMappingApproval>()
                            .Where(x => x.IdVenueMapping == updateData.Id)
                            .ToListAsync(CancellationToken);

                        if (getApprovalData.Any())
                        {
                            getApprovalData.ForEach(x => x.IsActive = false);
                            _dbContext.Entity<MsVenueMappingApproval>().UpdateRange(getApprovalData);
                        }

                        if (updateData.IsNeedApproval)
                        {
                            var listApproval = new List<MsVenueMappingApproval>();
                            foreach (var approver in data.NeedApproval)
                            {
                                if (listApproval.Where(x => x.IdVenueMapping == updateData.Id && x.IdBinusian == approver.Id).Any())
                                {
                                    throw new Exception("Each mapping venue can't have duplicate approver");
                                }

                                var addApprovalData = new MsVenueMappingApproval
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdVenueMapping = updateData.Id,
                                    IdBinusian = approver.Id,
                                };

                                listApproval.Add(addApprovalData);
                            }
                            _dbContext.Entity<MsVenueMappingApproval>().AddRange(listApproval);
                        }
                        _dbContext.Entity<MsVenueMapping>().Update(updateData);
                    }

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
