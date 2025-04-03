using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
    public class TransferMasterDistrictHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public TransferMasterDistrictHandler(IStudentDbContext dbContext)
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

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<TransferMasterDistrictRequest,TransferMasterDistrictValidator>();

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            
            var getDistrictList = await _dbContext.Entity<LtDistrict>()   
                            //.Where(x => x.IsActive == true)                          
                            .ToListAsync(CancellationToken);

            foreach (var std in body.DistrictList)
            {
                var existDistrict = getDistrictList.Find(x => x.Id == std.IdDistrict);
                    
                if(existDistrict == null)
                {
                    var newCountry = new LtDistrict
                    {
                        Id = std.IdDistrict,
                        UserIn = "API0001",       
                        DistrictName = std.DistrictName                             
                        
                    };
                    _dbContext.Entity<LtDistrict>().Add(newCountry);  
                }
                else
                {

                    existDistrict.UserUp = "API0001";   
                    existDistrict.IsActive = true;
                    existDistrict.DistrictName = std.DistrictName;
                    
                    _dbContext.Entity<LtDistrict>().Update(existDistrict); 
                }                        

            }
                     
            await _dbContext.SaveChangesAsync(CancellationToken);          
            await Transaction.CommitAsync(CancellationToken); 

            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new System.NotImplementedException();
        }
    }
}
