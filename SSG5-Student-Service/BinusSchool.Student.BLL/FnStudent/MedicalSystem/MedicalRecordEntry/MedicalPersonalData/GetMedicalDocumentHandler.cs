using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class GetMedicalDocumentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;

        public GetMedicalDocumentHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetMedicalDocumentRequest>(
                nameof(GetMedicalDocumentRequest.Id),
                nameof(GetMedicalDocumentRequest.Mode));

            var response = await GetMedicalDocument(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetMedicalDocumentResponse>> GetMedicalDocument(GetMedicalDocumentRequest request)
        {
            var response = new List<GetMedicalDocumentResponse>();

            var IdUser = MedicalDecryptionValidation.ValidateDecryptionData(request.Id);

            var getMedicalDocument = _context.Entity<TrMedicalDocument>()
                .Where(a => a.IdUser == IdUser)
                .ToList();

            if (!getMedicalDocument.Any())
                return response;

            var getUser = _context.Entity<MsUser>()
                .ToList();

            response = getMedicalDocument
                .Select(a => new GetMedicalDocumentResponse
                {
                    IdMedicalDocument = a.Id,
                    MedicalDocumentName = a.MedicalDocumentName,
                    UploadTime = a.DateIn,
                    UploadBy = getUser.Where(b => b.Id == a.UserIn).Select(b => b.DisplayName).FirstOrDefault(),
                    FileName = a.FileName,
                    FileUrl = a.FilePath,
                })
                .OrderByDescending(a => a.UploadTime)
                .ToList();

            return response;
        }
    }
}
