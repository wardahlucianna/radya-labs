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
    public class TransferPrevMasterSchoolHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;

        public TransferPrevMasterSchoolHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
           
            var body = await Request.ValidateBody<TransferPrevMasterSchoolRequest,TransferPrevMasterSchoolValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            
            var GetMsPrevMasterSchool = await _dbContext.Entity<MsPreviousSchoolNew>()   
                            //.Where(x => x.IsActive == true)                          
                            .ToListAsync(CancellationToken);

            foreach (var std in body.PrevMasterSchoolList)
            {
                var existMasterSchool = GetMsPrevMasterSchool.Find(x => x.Id == std.IdPrevMasterSchool);
                if(existMasterSchool != null)
                {
                  
                    existMasterSchool.UserUp = "API0001";        
                    existMasterSchool.IsActive = true;
                    existMasterSchool.NPSN = std.NPSN;
                    existMasterSchool.TypeLevel = std.TypeLevel;                  
                    existMasterSchool.SchoolName = std.SchoolName;
                    existMasterSchool.Address = std.Address;
                    existMasterSchool.Country = std.Country;
                    existMasterSchool.Province = std.Province;
                    existMasterSchool.Kota_kab = std.Kota_kab;
                    existMasterSchool.Website = std.Website;                   

                  _dbContext.Entity<MsPreviousSchoolNew>().Update(existMasterSchool);
                }
                else
                {
                    var newPrevMasterSchool = new MsPreviousSchoolNew
                    {
                      Id = std.IdPrevMasterSchool,
                      UserIn = "API0001",                  
                      NPSN = std.NPSN,
                      TypeLevel = std.TypeLevel,                    
                      SchoolName = std.SchoolName,
                      Address = std.Address,
                      Country = std.Country,
                      Province = std.Province,
                      Kota_kab = std.Kota_kab,
                      Website = std.Website                     
                      
                    };
                    _dbContext.Entity<MsPreviousSchoolNew>().Add(newPrevMasterSchool);    
                }
                    
            }    
            
            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

             return Request.CreateApiResult2();
        }
    }
}
