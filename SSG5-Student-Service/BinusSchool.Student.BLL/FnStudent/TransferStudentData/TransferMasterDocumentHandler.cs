using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.TransferStudentData.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.TransferStudentData
{
    public class TransferMasterDocumentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public string ExNotExist = "{0} with {1}: {2} does not exist.";
        public TransferMasterDocumentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var body = await Request.ValidateBody<TransferMasterDocumentRequest,TransferMasterDocumentValidator>();
               

            var existDocumentType = await _dbContext.Entity<LtDocumentType>().FindAsync(body.IdDocumentType);            

            if (existDocumentType is null)
            {                       
                throw new NotFoundException(string.Format(ExNotExist, "Document Type",  nameof(body.IdDocumentType), body.IdDocumentType));
            }

            var getMasterDocument = await _dbContext.Entity<MsDocument>()
                                    .Where(x => x.Id == body.IdDocument) 
                                    .FirstOrDefaultAsync(CancellationToken);
            if(getMasterDocument == null)
            {
                var newDocument = new MsDocument
                {
                    Id = body.IdDocument,
                    UserIn = "API0001",       
                    DocumentName = body.DocumentName,
                    DocumentNameEng = body.DocumentNameEng,
                    IdDocumentType = body.IdDocumentType                                   
                    
                };
                _dbContext.Entity<MsDocument>().Add(newDocument);  
            }
            else
            {

                getMasterDocument.UserUp = "API0001";   
                getMasterDocument.IsActive = true;
                getMasterDocument.DocumentName = body.DocumentName;
                getMasterDocument.DocumentNameEng = body.DocumentNameEng;
                getMasterDocument.IdDocumentType = body.IdDocumentType; 
                _dbContext.Entity<MsDocument>().Update(getMasterDocument); 
            }                        
            
            await _dbContext.SaveChangesAsync(CancellationToken);           

            return Request.CreateApiResult2();
        }
    }
}
