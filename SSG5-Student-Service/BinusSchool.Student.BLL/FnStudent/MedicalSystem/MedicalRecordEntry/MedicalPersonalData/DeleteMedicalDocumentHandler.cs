using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class DeleteMedicalDocumentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _date;
        private readonly IConfiguration _configuration;
        private IDbContextTransaction _transaction;

        public DeleteMedicalDocumentHandler(IStudentDbContext context, IMachineDateTime date, IConfiguration configuration)
        {
            _context = context;
            _date = date;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<DeleteMedicalDocumentRequest, DeleteMedicalDocumentValidator>();

            await DeleteMedicalDocument(request);

            return Request.CreateApiResult2(code: HttpStatusCode.NoContent);
        }

        public async Task DeleteMedicalDocument(DeleteMedicalDocumentRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken))
            {
                try
                {
                    var medicalDocument = _context.Entity<TrMedicalDocument>()
                        .Where(a => a.Id == request.IdDocument)
                        .FirstOrDefault();

                    var helper = new MedicalDocumentBlobHelper(_date, _configuration);

                    bool fileExist = await helper.RemoveFileIfExists(medicalDocument.FileName);

                    if (!fileExist)
                        throw new BadRequestException("File not exist for delete");

                    medicalDocument.IsActive = false;

                    _context.Entity<TrMedicalDocument>().Update(medicalDocument);

                    await _context.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
                catch (Exception ex)
                {
                    _transaction?.Rollback();

                    throw new Exception(ex.Message.ToString());
                }
            }
        }
    }
}
