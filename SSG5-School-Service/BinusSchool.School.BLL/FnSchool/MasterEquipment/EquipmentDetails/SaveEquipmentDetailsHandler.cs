using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MasterEquipment.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.MasterEquipment.EquipmentDetails
{
    public class SaveEquipmentDetailsHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveEquipmentDetailsHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveEquipmentDetailsRequest, SaveEquipmentDetailsValidator>();

            var getData = await _dbContext.Entity<MsEquipment>()
                .Include(x => x.EquipmentType)
                .Where(x => x.EquipmentType.IdSchool == param.IdSchool)
                .ToListAsync(CancellationToken);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                
                if(param.IdEquipment == null)
                {
                    if (param.TotalStockQty <= 0)
                    {
                        throw new BadRequestException("Total stock quantity must be greater than zero");
                    }

                    if (getData.Where(x => x.EquipmentName == param.EquipmentName).Any())
                    {
                        throw new Exception("Equipment Name already exist");
                    }
                    
                    var newEquipment = new MsEquipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEquipmentType = param.IdEquipmentType,
                        EquipmentName = param.EquipmentName,
                        Description = param.EquipmentDescription,
                        TotalStockQty = param.TotalStockQty,
                        MaxQtyBorrowing = param.MaxQtyBorrowing
                    };

                    _dbContext.Entity<MsEquipment>().Add(newEquipment);
                }
                else
                {
                    if (param.TotalStockQty < 0)
                    {
                        throw new BadRequestException("Total stock quantity must be greater or equal to zero");
                    }

                    var equipment = getData.Where(x => x.Id == param.IdEquipment).FirstOrDefault();

                    if (equipment == null)
                    {
                        throw new Exception("Equipment not found");
                    }

                    if (getData.Where(x => x.EquipmentName == param.EquipmentName && x.Id != param.IdEquipment).Any())
                    {
                        throw new Exception("Equipment Name already exist");
                    }

                    var history = new HMsEquipment
                    {
                        IdHMsEquipment = Guid.NewGuid().ToString(),
                        IdEquipment = equipment.Id,
                        IdEquipmentType = equipment.IdEquipmentType,
                        EquipmentName = equipment.EquipmentName,
                        Description = equipment.Description,
                        TotalStockQty = equipment.TotalStockQty,
                        MaxQtyBorrowing = equipment.MaxQtyBorrowing
                    };

                    _dbContext.Entity<HMsEquipment>().Add(history);

                    equipment.EquipmentName = param.EquipmentName;
                    equipment.Description = param.EquipmentDescription;
                    equipment.TotalStockQty = param.TotalStockQty;
                    equipment.MaxQtyBorrowing = param.MaxQtyBorrowing;

                    _dbContext.Entity<MsEquipment>().Update(equipment);
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
