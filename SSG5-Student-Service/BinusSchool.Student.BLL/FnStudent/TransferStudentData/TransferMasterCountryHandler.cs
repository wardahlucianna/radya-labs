using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
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
    public class TransferMasterCountryHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;       
        public TransferMasterCountryHandler(IStudentDbContext dbContext)
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
            var body = await Request.ValidateBody<TransferMasterCountryRequest,TransferMasterCountryValidator>();

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            
            var getCoutryList = await _dbContext.Entity<LtCountry>()   
                            //.Where(x => x.IsActive == true)                          
                            .ToListAsync(CancellationToken);

            foreach (var std in body.CountryList)
            {
                var exisCoutry = getCoutryList.Find(x => x.Id == std.IdCountry);
                    
                if(exisCoutry == null)
                {
                    var newCountry = new LtCountry
                    {
                        Id = std.IdCountry,
                        UserIn = "API0001",       
                        CountryName = std.CountryName                             
                        
                    };
                    _dbContext.Entity<LtCountry>().Add(newCountry);  
                }
                else
                {

                    exisCoutry.UserUp = "API0001";   
                    exisCoutry.IsActive = true;
                    exisCoutry.CountryName = std.CountryName;
                    
                    _dbContext.Entity<LtCountry>().Update(exisCoutry); 
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
