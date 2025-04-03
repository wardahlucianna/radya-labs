using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Scheduling.FnActivities;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion.Validator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class UpdateMasterImmersionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public UpdateMasterImmersionHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateMasterImmersionRequest, UpdateMasterImmersionValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var dataImmersion = _dbContext.Entity<MsImmersion>()
                                        .Find(param.IdImmersion);

                if (dataImmersion == null)
                {
                    throw new BadRequestException("Failed! Immersion data is not found");
                }

                // update MsImmersion
                dataImmersion.Semester = param.Semester;
                dataImmersion.Destination = param.Destination;
                dataImmersion.IdImmersionPeriod = param.IdImmersionPeriod;
                dataImmersion.Description = param.Description;
                dataImmersion.StartDate = param.StartDate <= param.EndDate ? param.StartDate : throw new BadRequestException("Start date must be less than or equal to end date");
                dataImmersion.EndDate = param.EndDate >= param.StartDate ? param.EndDate : throw new BadRequestException("End date must be greater than or equal to start date");
                dataImmersion.IdBinusianPIC = param.IdBinusianPIC;
                dataImmersion.PICEmail = StringUtil.IsValidEmailAddress(param.PICEmail) ? param.PICEmail : throw new BadRequestException("Invalid email format");
                dataImmersion.PICPhone = param.PICPhone;
                dataImmersion.MinParticipant = param.MinParticipant <= param.MaxParticipant ? param.MinParticipant : throw new BadRequestException("Min participant must be less than or equal to max participant");
                dataImmersion.MaxParticipant = param.MaxParticipant >= param.MinParticipant ? param.MaxParticipant : throw new BadRequestException("Max participant must be greater than or equal to min participant");
                dataImmersion.IdCurrency = param.IdCurrency;
                dataImmersion.IdImmersionPaymentMethod = param.IdImmersionPaymentMethod;
                dataImmersion.RegistrationFee = param.RegistrationFee;
                dataImmersion.TotalCost = param.TotalCost;

                _dbContext.Entity<MsImmersion>().Update(dataImmersion);

                // remove old TrImmersionGradeMapping
                var dataOldGradeMappingList = _dbContext.Entity<TrImmersionGradeMapping>()
                                                .Where(x => x.IdImmersion == param.IdImmersion)
                                                .ToList();

                _dbContext.Entity<TrImmersionGradeMapping>().RemoveRange(dataOldGradeMappingList);

                // add new TrImmersionGradeMapping
                foreach (var idGrade in param.IdGradeList)
                {
                    var dataNewGradeMapping = new TrImmersionGradeMapping()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdImmersion = param.IdImmersion,
                        IdGrade = idGrade
                    };

                    _dbContext.Entity<TrImmersionGradeMapping>().Add(dataNewGradeMapping);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction.Dispose();
            }

            throw new BadRequestException(null);
        }
    }
}
