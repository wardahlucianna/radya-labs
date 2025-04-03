using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using static BinusSchool.Student.FnStudent.MedicalSystem.Helper.MedicalDocumentBlobHelper;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class SaveMedicalDocumentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _date;
        private readonly IConfiguration _configuration;
        private IDbContextTransaction _transaction;

        public SaveMedicalDocumentHandler(IStudentDbContext context, IMachineDateTime date, IConfiguration configuration)
        {
            _context = context;
            _date = date;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBodyForm<SaveMedicalDocumentRequest, SaveMedicalDocumentValidator>();

            await SaveMedicalDocument(request);

            return Request.CreateApiResult2(code: HttpStatusCode.Created);
        }

        public async Task SaveMedicalDocument(SaveMedicalDocumentRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var IdUser = MedicalDecryptionValidation.ValidateDecryptionData(request.Id);
                    var container = "medical-document";
                    var helper = new MedicalDocumentBlobHelper(_date, _configuration);

                    if (string.IsNullOrEmpty(request.IdDocument)) // create
                    {
                        var file = Request.Form.Files.FirstOrDefault();

                        #region Validasi File
                        if (file is null || file.Length == 0)
                            throw new BadRequestException("Document file not found");

                        // Validasi ukuran file maksimal 2MB (2 * 1024 * 1024 bytes)
                        long maxFileSize = 2 * 1024 * 1024; // 2MB dalam byte
                        if (file.Length > maxFileSize)
                            throw new BadRequestException("File size exceeds the maximum allowed size of 2MB.");

                        var fileInfo = new FileInfo(file.FileName);
                        if (fileInfo.Extension != ".pdf")
                            throw new BadRequestException($"File not allowed. Allowed file: .pdf");
                        #endregion

                        UploadMedicalDocumentFileResult fileResult = await helper.UploadMedicalDocumentFile(file);

                        var insertMedicalDocument = new TrMedicalDocument()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUser = IdUser,
                            MedicalDocumentName = request.DocumentName,
                            FileName = fileResult.FileName,
                            FilePath = fileResult.FileUrl,
                        };

                        _context.Entity<TrMedicalDocument>().Add(insertMedicalDocument);
                    }
                    else // update
                    {
                        var file = Request.Form.Files.FirstOrDefault();

                        if (file is null || file.Length == 0)
                        {
                            var medicalDocument = _context.Entity<TrMedicalDocument>()
                                .Where(a => a.Id == request.IdDocument
                                    && a.IdUser == IdUser)
                                .FirstOrDefault();

                            medicalDocument.MedicalDocumentName = request.DocumentName;

                            _context.Entity<TrMedicalDocument>().Update(medicalDocument);
                        }
                        else
                        {
                            var medicalDocument = _context.Entity<TrMedicalDocument>()
                                .Where(a => a.Id == request.IdDocument
                                    && a.IdUser == IdUser)
                                .FirstOrDefault();

                            #region Validasi File
                            // Validasi ukuran file maksimal 2MB (2 * 1024 * 1024 bytes)
                            long maxFileSize = 2 * 1024 * 1024; // 2MB dalam byte
                            if (file.Length > maxFileSize)
                                throw new BadRequestException("File size exceeds the maximum allowed size of 2MB.");

                            var fileInfo = new FileInfo(file.FileName);
                            if (fileInfo.Extension != ".pdf")
                                throw new BadRequestException($"File not allowed. Allowed file: .pdf");
                            #endregion

                            bool fileExist = await helper.RemoveFileIfExists(medicalDocument.FileName);

                            UploadMedicalDocumentFileResult fileResult = await helper.UploadMedicalDocumentFile(file);

                            medicalDocument.MedicalDocumentName = request.DocumentName;
                            medicalDocument.FileName = fileResult.FileName;
                            medicalDocument.FilePath = fileResult.FileUrl;

                            _context.Entity<TrMedicalDocument>().Update(medicalDocument);
                        }
                    }

                    await _context.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
                catch (Exception ex)
                {
                    _transaction?.Rollback();

                    throw new Exception($"Message: {ex.Message.ToString()}, Inner Message: {ex.InnerException.Message.ToString()}");
                }
            }
        }
    }
}
