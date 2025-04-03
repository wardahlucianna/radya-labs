using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.TransferStudentData.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.TransferStudentData
{
    public class TransferOccupationTypeHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public TransferOccupationTypeHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<TransferOccupationTypeRequest,TransferOccupationTypeValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var GetLtOccupationType = await _dbContext.Entity<LtOccupationType>()   
                    //.Where(x => x.IsActive == true)                          
                    .ToListAsync(CancellationToken);

             foreach (var std in body.OccupationTypeList)
            {
                var existOccupationType = GetLtOccupationType.Find(x => x.Id == std.IdOccupationType);
                if(existOccupationType != null)
                {
                  
                    existOccupationType.UserUp = "API0001";                   
                    existOccupationType.IsActive = true;
                    existOccupationType.OccupationTypeName = std.OccupationTypeName;
                    existOccupationType.OccupationTypeNameEng = std.OccupationTypeNameEng;                              

                  _dbContext.Entity<LtOccupationType>().Update(existOccupationType);
                }
                else
                {
                    var NewOccupationType = new LtOccupationType
                    {
                      Id = std.IdOccupationType,
                      UserIn = "API0001",  
                      OccupationTypeName = std.OccupationTypeName,
                      OccupationTypeNameEng = std.OccupationTypeNameEng                                   
                      
                    };
                    _dbContext.Entity<LtOccupationType>().Add(NewOccupationType);    
                }                    
            }    
            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
