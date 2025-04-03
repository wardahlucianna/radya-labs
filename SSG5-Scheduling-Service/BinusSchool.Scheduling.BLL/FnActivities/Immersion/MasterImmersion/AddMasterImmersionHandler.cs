using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion.Validator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class AddMasterImmersionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public AddMasterImmersionHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<AddMasterImmersionRequest, AddMasterImmersionValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var newIdImmersion = Guid.NewGuid().ToString();

                // insert MsImmersion
                var insertImmersion = new MsImmersion()
                {
                    Id = newIdImmersion,
                    Semester = param.Semester,
                    Destination = param.Destination,
                    IdImmersionPeriod = param.IdImmersionPeriod,
                    Description = param.Description,
                    StartDate = param.StartDate <= param.EndDate ? param.StartDate : throw new BadRequestException("Start date must be less than or equal to end date"),
                    EndDate = param.EndDate >= param.StartDate ? param.EndDate : throw new BadRequestException("End date must be greater than or equal to start date"),
                    IdBinusianPIC = param.IdBinusianPIC,
                    PICEmail = StringUtil.IsValidEmailAddress(param.PICEmail) ? param.PICEmail : throw new BadRequestException("Invalid email format"),
                    PICPhone = param.PICPhone,
                    MinParticipant = param.MinParticipant <= param.MaxParticipant ? param.MinParticipant : throw new BadRequestException("Min participant must be less than or equal to max participant"),
                    MaxParticipant = param.MaxParticipant >= param.MinParticipant ? param.MaxParticipant : throw new BadRequestException("Max participant must be greater than or equal to min participant"),
                    IdCurrency = param.IdCurrency,
                    IdImmersionPaymentMethod = param.IdImmersionPaymentMethod,
                    RegistrationFee = param.RegistrationFee,
                    TotalCost = param.TotalCost
                };

                _dbContext.Entity<MsImmersion>().Add(insertImmersion);

                // insert TrImmersionGradeMapping
                foreach (var idGrade in param.IdGradeList)
                {

                    var insertGradeMapping = new TrImmersionGradeMapping()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdImmersion = newIdImmersion,
                        IdGrade = idGrade
                    };

                    _dbContext.Entity<TrImmersionGradeMapping>().Add(insertGradeMapping);
                }

                var returnResult = new AddMasterImmersionResult()
                {
                    IdImmersion = newIdImmersion
                };


                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2(returnResult as object);
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

            throw new BadRequestException(null);
        }
    }
}
