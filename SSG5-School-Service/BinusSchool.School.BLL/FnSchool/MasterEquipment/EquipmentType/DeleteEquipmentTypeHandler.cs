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
    public class DeleteEquipmentTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public DeleteEquipmentTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteEquipmentTypeRequest, DeleteEquipmentTypeValidator>();

            var getData = await _dbContext.Entity<MsEquipmentType>()
                .Include(x => x.Equipments)
                .Where(x => param.IdEquipmentType.Contains(x.Id))
                .ToListAsync(CancellationToken);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach (var item in getData)
                {
                    if (item.Equipments.Any())
                    {
                        throw new Exception("Cannot delete Equipment Type because it has related data");
                    }

                    item.IsActive = false;
                    _dbContext.Entity<MsEquipmentType>().Update(item);
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
