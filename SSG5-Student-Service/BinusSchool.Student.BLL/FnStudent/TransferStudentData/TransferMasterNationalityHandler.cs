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
    public class TransferMasterNationalityHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public TransferMasterNationalityHandler(IStudentDbContext dbContext)
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
            var body = await Request.ValidateBody<TransferMasterNationalityRequest,TransferMasterNationalityValidator>();

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            
            var getNationalityList = await _dbContext.Entity<LtNationality>()   
                            //.Where(x => x.IsActive == true)                          
                            .ToListAsync(CancellationToken);

            foreach (var std in body.NationalityList)
            {
                var existNationality = getNationalityList.Find(x => x.Id == std.IdNationality);
                    
                if(existNationality == null)
                {
                    var newCountry = new LtNationality
                    {
                        Id = std.IdNationality,
                        UserIn = "API0001",       
                        NationalityName = std.NationalityName                             
                        
                    };
                    _dbContext.Entity<LtNationality>().Add(newCountry);  
                }
                else
                {

                    existNationality.UserUp = "API0001";   
                    existNationality.IsActive = true;
                    existNationality.NationalityName = std.NationalityName;
                    
                    _dbContext.Entity<LtNationality>().Update(existNationality); 
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
