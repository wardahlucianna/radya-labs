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
    public class UpdateDigitalPickupSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public UpdateDigitalPickupSettingHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateDigitalPickupSettingRequest, UpdateDigitalPickupSettingValidator>();

            var updateData = await _dbContext.Entity<MsDigitalPickupSetting>()
                .Where(x => param.IdGrade.Contains(x.IdGrade))
                .ToListAsync(CancellationToken);

            var addData = await _dbContext.Entity<MsGrade>()
                .Include(x => x.MsLevel)
                .Where(x => param.IdGrade.Contains(x.Id))
                .Where(x => !updateData.Select(y => y.IdGrade).Contains(x.Id))
                .ToListAsync(CancellationToken);

            var StartTime = TimeSpan.Parse(param.StartScanTime);
            var EndTime = TimeSpan.Parse(param.EndScanTime);

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                foreach (var data in addData)
                {
                    var addDigitalPickupSetting = new MsDigitalPickupSetting
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = data.MsLevel.IdAcademicYear,
                        IdGrade = data.Id,
                        StartTime = StartTime,
                        EndTime = EndTime
                    };

                    _dbContext.Entity<MsDigitalPickupSetting>().Add(addDigitalPickupSetting);
                }
                foreach(var data in updateData)
                {
                    data.StartTime = StartTime;
                    data.EndTime = EndTime;

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
