using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting;
using BinusSchool.Student.FnStudent.DigitalPickup.Validator;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.DigitalPickup.Setting
{
    public class DeleteDigitalPickupSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteDigitalPickupSettingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteDigitalPickupSettingRequest, DeleteDigitalPickupSettingValidator>();

            var getData = await _dbContext.Entity<MsDigitalPickupSetting>()
                .Where(x => param.IdGrade.Contains(x.IdGrade))
                .ToListAsync(CancellationToken);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach(var data in getData)
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsDigitalPickupSetting>().Update(data);
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
