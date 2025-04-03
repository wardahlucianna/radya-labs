using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Floor.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.Floor
{
    public class SaveFloorHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveFloorHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveFloorRequest, SaveFloorValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (string.IsNullOrEmpty(param.IdFloor)) //Insert new data
                {
                    var checkData = await _dbContext.Entity<MsFloor>()
                        .Where(x => x.FloorName == param.FloorName &&
                                    x.IdBuilding == param.IdBuilding)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (checkData != null)
                    {
                        throw new Exception("Floor Name already exists");
                    }

                    var data = new MsFloor
                    {
                        Id = Guid.NewGuid().ToString(),
                        FloorName = param.FloorName,
                        HasLocker = param.HasLocker,
                        IdBuilding = param.IdBuilding,
                        LockerTowerCodeName = param.LockerTowerCodeName,
                        Description = param.Description,
                        URL = param.URL,
                        FileName = param.FileName,
                        FileType = param.FileType,
                        FileSize = param.FileSize,
                        IsShowFloorLayout = param.IsShowFloorLayout
                    };

                    _dbContext.Entity<MsFloor>().Add(data);
                }
                else //update data
                {
                    var data = await _dbContext.Entity<MsFloor>()
                        .Where(x => x.Id == param.IdFloor)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (data == null)
                    {
                        throw new Exception("Data not found");
                    }

                    data.HasLocker = param.HasLocker;
                    data.IsShowFloorLayout = param.IsShowFloorLayout;
                    data.LockerTowerCodeName = param.LockerTowerCodeName;
                    data.Description = param.Description;
                    data.URL = param.URL;
                    data.FileName = param.FileName;
                    data.FileType = param.FileType;
                    data.FileSize = param.FileSize;

                    _dbContext.Entity<MsFloor>().Update(data);
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
