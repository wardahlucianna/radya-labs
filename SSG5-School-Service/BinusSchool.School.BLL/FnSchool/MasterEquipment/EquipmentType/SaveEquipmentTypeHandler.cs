using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MasterEquipment.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace BinusSchool.School.FnSchool.MasterEquipment.EquipmentType
{
    public class SaveEquipmentTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public SaveEquipmentTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveEquipmentTypeRequest, SaveEquipmentTypeValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getData = await _dbContext.Entity<MsEquipmentType>()
                    .Where(x => x.IdSchool == param.IdSchool)
                    .ToListAsync(CancellationToken);

                if (string.IsNullOrEmpty(param.IdEquipmentType)) //Create new
                {
                    if(getData.Where(x => x.EquipmentTypeName == param.EquipmentTypeName).Any())
                    {
                        throw new Exception("Equipment Type already exist");
                    }

                    var addData = new MsEquipmentType
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSchool = param.IdSchool,
                        EquipmentTypeName = param.EquipmentTypeName,
                        IdReservationOwner = param.ReservationOwner,
                    };

                    _dbContext.Entity<MsEquipmentType>().Add(addData);
                }
                else // Update
                {
                    var updateData = getData.Where(x => x.Id == param.IdEquipmentType).FirstOrDefault();

                    if (updateData == null)
                    {
                        throw new Exception("Equipment Type not found");
                    }

                    updateData.IdReservationOwner = param.ReservationOwner;

                    _dbContext.Entity<MsEquipmentType>().Update(updateData);
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
